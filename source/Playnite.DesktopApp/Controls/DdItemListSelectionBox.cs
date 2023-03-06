using Playnite.Common;
using Playnite.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Linq;

namespace Playnite.DesktopApp.Controls
{
    [TemplatePart(Name = "PART_ToggleSelectedOnly", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_SearchBox", Type = typeof(SearchBox))]
    [TemplatePart(Name = "PART_ElemSearchHost", Type = typeof(FrameworkElement))]
    public class DdItemListSelectionBox : ComboBoxListBase
    {
        internal ToggleButton ToggleSelectedOnly;
        internal SearchBox TextSearchBox;
        internal FrameworkElement ElemSearchHost;

        public bool ShowSearchBox
        {
            get => (bool)GetValue(ShowSearchBoxProperty);
            set => SetValue(ShowSearchBoxProperty, value);
        }

        public static readonly DependencyProperty ShowSearchBoxProperty = DependencyProperty.Register(
            nameof(ShowSearchBox),
            typeof(bool),
            typeof(ComboBoxListBase),
            new PropertyMetadata(false));

        public SelectableDbItemList ItemsList
        {
            get
            {
                return (SelectableDbItemList)GetValue(ItemsListProperty);
            }

            set
            {
                SetValue(ItemsListProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsListProperty = DependencyProperty.Register(
            nameof(ItemsList),
            typeof(SelectableDbItemList),
            typeof(DdItemListSelectionBox),
            new PropertyMetadata(null, ItemsListPropertyChangedCallback));

        private static void ItemsListPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as DdItemListSelectionBox;
            var oldVal = (SelectableDbItemList)e.NewValue;
            if (oldVal != null)
            {
                oldVal.SelectionChanged -= obj.List_SelectionChanged;
            }

            var list = (SelectableDbItemList)e.NewValue;
            list.SelectionChanged += obj.List_SelectionChanged;
            obj.UpdateTextStatus();
        }

        private void List_SelectionChanged(object sender, EventArgs e)
        {
            if (!IgnoreChanges)
            {
                IgnoreChanges = true;
                BoundIds = ItemsList.GetSelectedIds();
                IgnoreChanges = false;
                UpdateTextStatus();
            }
        }

        public object BoundIds
        {
            get
            {
                return GetValue(BoundIdsProperty);
            }

            set
            {
                SetValue(BoundIdsProperty, value);
            }
        }

        public static readonly DependencyProperty BoundIdsProperty = DependencyProperty.Register(
            nameof(BoundIds),
            typeof(object),
            typeof(DdItemListSelectionBox),
            new PropertyMetadata(null, BoundIdsPropertyChangedCallback));

        private static void BoundIdsPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as DdItemListSelectionBox;
            if (obj.IgnoreChanges)
            {
                return;
            }

            obj.IgnoreChanges = true;
            obj.ItemsList?.SetSelection(obj.BoundIds as IEnumerable<Guid>);
            obj.IgnoreChanges = false;
            obj.UpdateTextStatus();
        }

        static DdItemListSelectionBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DdItemListSelectionBox), new FrameworkPropertyMetadata(typeof(DdItemListSelectionBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (ItemsPanel != null)
            {
                BindingTools.ClearBinding(ItemsPanel, ItemsControl.ItemsSourceProperty);
                BindingTools.SetBinding(
                    ItemsPanel,
                    ItemsControl.ItemsSourceProperty,
                    this,
                    "ItemsList.CollectionView");

                XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                ItemsPanel.ItemTemplate = Xaml.FromString<DataTemplate>(new XDocument(
                    new XElement(pns + nameof(DataTemplate),
                        new XElement(pns + nameof(CheckBox),
                            new XAttribute(nameof(CheckBox.IsChecked), "{Binding Selected}"),
                            new XAttribute(nameof(CheckBox.Content), "{Binding Item.Name}"),
                            new XAttribute(nameof(CheckBox.IsThreeState), "{Binding IsThreeState, Mode=OneWay, RelativeSource={RelativeSource AncestorType=DdItemListSelectionBox}}"),
                            new XAttribute(nameof(CheckBox.Style), $"{{DynamicResource ComboBoxListItemStyle}}")))
                ).ToString());
            }

            ToggleSelectedOnly = Template.FindName("PART_ToggleSelectedOnly", this) as ToggleButton;
            if (ToggleSelectedOnly != null)
            {
                BindingTools.SetBinding(
                   ToggleSelectedOnly,
                   ToggleButton.IsCheckedProperty,
                   this,
                   nameof(ItemsList) + "." + nameof(ItemsList.ShowSelectedOnly),
                   BindingMode.TwoWay);
            }

            ElemSearchHost = Template.FindName("PART_ElemSearchHost", this) as FrameworkElement;
            if (ElemSearchHost != null)
            {
                BindingTools.SetBinding(
                    ElemSearchHost,
                    FrameworkElement.VisibilityProperty,
                    this,
                    nameof(ShowSearchBox),
                    converter: new Converters.BooleanToVisibilityConverter());
            }

            TextSearchBox = Template.FindName("PART_SearchBox", this) as SearchBox;
            if (TextSearchBox != null)
            {
                BindingTools.SetBinding(
                    TextSearchBox,
                    SearchBox.TextProperty,
                    this,
                    nameof(ItemsList) + "." + nameof(ItemsList.SearchText),
                    BindingMode.TwoWay);
            }

            UpdateTextStatus();

            if (Template.FindName("Popup", this) is Popup popup)
            {
                popup.Opened += (_, __) =>
                {
                    if (ShowSearchBox && TextSearchBox != null)
                    {
                        TextSearchBox.IsFocused = true;
                    }
                };

                popup.Closed += (_, __) =>
                {
                    if (ShowSearchBox && TextSearchBox != null)
                    {
                        TextSearchBox.IsFocused = false;
                        TextSearchBox.Text = string.Empty;
                    }
                };

                popup.PreviewKeyUp += (_, keyArgs) =>
                {
                    if (keyArgs.Key == Key.Escape)
                    {
                        popup.IsOpen = false;
                    }
                };
            }
        }

        public override void ClearButtonAction(RoutedEventArgs e)
        {
            ItemsList.SetSelection(null);
            BoundIds = null;
        }

        private void UpdateTextStatus()
        {
            if (TextFilterString != null)
            {
                TextFilterString.Text = ItemsList?.AsString;
            }
        }
    }
}

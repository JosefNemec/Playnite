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
    [TemplatePart(Name = "PART_CheckedOnly", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_SearchBox", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_SearchOptions", Type = typeof(Grid))]
    public class DdItemListSelectionBox : ComboBoxListBase
    {
        internal ToggleButton ButtonCheckedOnly;
        internal SearchBox TextSearchBox;
        internal Grid SearchOptions;

        private string searchText = string.Empty;
        public string SearchText
        {
            get
            {
                return searchText;
            }

            set
            {
                searchText = value;
                if (TextSearchBox != null && ItemsList != null)
                {
                    ItemsList.SearchText = searchText;
                }
            }
        }

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
            
            ButtonCheckedOnly = Template.FindName("PART_CheckedOnly", this) as ToggleButton;
            TextSearchBox = Template.FindName("PART_SearchBox", this) as SearchBox;
            SearchOptions = Template.FindName("PART_SearchOptions", this) as Grid;

            if (ItemsPanel != null)
            {
                XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                ItemsPanel.ItemTemplate = Xaml.FromString<DataTemplate>(new XDocument(
                    new XElement(pns + nameof(DataTemplate),
                        new XElement(pns + nameof(CheckBox),
                            new XAttribute(nameof(CheckBox.IsChecked), "{Binding Selected}"),
                            new XAttribute(nameof(CheckBox.Content), "{Binding Item.Name}"),
                            new XAttribute(nameof(CheckBox.IsThreeState), "{Binding IsThreeState, Mode=OneWay, RelativeSource={RelativeSource AncestorType=DdItemListSelectionBox}}"),
                            new XAttribute(nameof(CheckBox.Visibility), "{Binding IsVisible, Converter={StaticResource BooleanToVisibilityConverter}}"),
                            new XAttribute(nameof(CheckBox.Style), $"{{DynamicResource ComboBoxListItemStyle}}")))
                ).ToString());
            }

            if (ButtonCheckedOnly != null)
            {
                ButtonCheckedOnly.Click += (_, e) => ButtonCheckedOnlyAction(_, e);
            }

            if (SearchOptions != null)
            {
                BindingTools.SetBinding(
                    SearchOptions,
                    Grid.VisibilityProperty,
                    this,
                    nameof(ShowSearchBox),
                    converter: new Converters.BooleanToVisibilityConverter());

                BindingTools.SetBinding(
                    TextSearchBox,
                    SearchBox.TextProperty,
                    this,
                    nameof(SearchText),
                    BindingMode.TwoWay,
                    delay: 100);
            }

            UpdateTextStatus();
        }

        public void ButtonCheckedOnlyAction(object sender, RoutedEventArgs e)
        {
            ItemsList.ShowSelectedOnly = (bool)((ToggleButton)sender).IsChecked;
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

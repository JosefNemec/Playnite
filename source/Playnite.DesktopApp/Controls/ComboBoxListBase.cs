using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Xml.Linq;

namespace Playnite.DesktopApp.Controls
{
    [TemplatePart(Name = "PART_ItemsPanel", Type = typeof(ItemsControl))]
    [TemplatePart(Name = "PART_ButtonClearFilter", Type = typeof(Button))]
    [TemplatePart(Name = "PART_TextFilterString", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_CheckedOnly", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_SearchBox", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_SearchOptions", Type = typeof(Grid))]
    public abstract class ComboBoxListBase : Control
    {
        internal ItemsControl ItemsPanel;
        internal Button ButtonClearFilter;
        internal TextBlock TextFilterString;
        internal ToggleButton ButtonCheckedOnly;
        internal SearchBox TextSearchBox;
        internal Grid SearchOptions;

        internal bool IgnoreChanges { get; set; }

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
                if (TextSearchBox != null)
                {
                    TextSearchBox_KeyUp(TextSearchBox, null);
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


        public bool IsThreeState
        {
            get => (bool)GetValue(IsThreeStateProperty);
            set => SetValue(IsThreeStateProperty, value);
        }

        public static readonly DependencyProperty IsThreeStateProperty = DependencyProperty.Register(
            nameof(IsThreeState),
            typeof(bool),
            typeof(ComboBoxListBase));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ButtonClearFilter = Template.FindName("PART_ButtonClearFilter", this) as Button;
            TextFilterString = Template.FindName("PART_TextFilterString", this) as TextBlock;
            ItemsPanel = Template.FindName("PART_ItemsPanel", this) as ItemsControl;
            ButtonCheckedOnly = Template.FindName("PART_CheckedOnly", this) as ToggleButton;
            TextSearchBox = Template.FindName("PART_SearchBox", this) as SearchBox;
            SearchOptions = Template.FindName("PART_SearchOptions", this) as Grid;

            if (ButtonClearFilter != null)
            {
                ButtonClearFilter.Click += (_, e) => ClearButtonAction(e);
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
                    System.Windows.Data.BindingMode.TwoWay,
                    delay: 100);
            }

            if (TextSearchBox != null)
            {
                TextSearchBox.KeyUp += TextSearchBox_KeyUp;
            }

            if (ItemsPanel != null)
            {
                BindingTools.SetBinding(
                    ItemsPanel,
                    ItemsControl.ItemsSourceProperty,
                    this,
                    "ItemsList");

                XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";

                ItemsPanel.ItemsPanel = Xaml.FromString<ItemsPanelTemplate>(new XDocument(
                    new XElement(pns + nameof(ItemsPanelTemplate),
                        new XElement(pns + nameof(VirtualizingStackPanel)))
                ).ToString());

                ItemsPanel.Template = Xaml.FromString<ControlTemplate>(new XDocument(
                     new XElement(pns + nameof(ControlTemplate),
                        new XElement(pns + nameof(ScrollViewer),
                            new XAttribute(nameof(ScrollViewer.Focusable), false),
                            new XElement(pns + nameof(ItemsPresenter))))
                ).ToString());

                ItemsPanel.ItemTemplate = Xaml.FromString<DataTemplate>(new XDocument(
                    new XElement(pns + nameof(DataTemplate),
                        new XElement(pns + nameof(CheckBox),
                            new XAttribute(nameof(CheckBox.IsChecked), "{Binding Selected}"),
                            new XAttribute(nameof(CheckBox.Content), "{Binding Item}"),
                            new XAttribute(nameof(CheckBox.Style), $"{{DynamicResource ComboBoxListItemStyle}}")))
                ).ToString());

                ScrollViewer.SetCanContentScroll(ItemsPanel, true);
                KeyboardNavigation.SetDirectionalNavigation(ItemsPanel, KeyboardNavigationMode.Contained);
                VirtualizingPanel.SetIsVirtualizing(ItemsPanel, true);
                VirtualizingPanel.SetVirtualizationMode(ItemsPanel, VirtualizationMode.Recycling);
            }
        }

        public virtual void ClearButtonAction(RoutedEventArgs e)
        {
        }

        public virtual void TextSearchBox_KeyUp(object sender, KeyEventArgs e)
        {
        }

        public virtual void ButtonCheckedOnlyAction(object sender, RoutedEventArgs e)
        {
        }
    }
}

using Playnite.Common;
using Playnite.Converters;
using Playnite.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;

namespace Playnite.DesktopApp.Controls
{
    [TemplatePart(Name = "PART_TextFilterInput", Type = typeof(TextBox))]
    public abstract class FilterSelectionBoxBase : ComboBoxListBase
    {
        internal TextBox TextFilterInput;

        public bool IsFullTextEnabled
        {
            get => (bool)GetValue(IsFullTextEnabledProperty);
            set => SetValue(IsFullTextEnabledProperty, value);
        }

        public static readonly DependencyProperty IsFullTextEnabledProperty = DependencyProperty.Register(
            nameof(IsFullTextEnabled),
            typeof(bool),
            typeof(FilterSelectionBoxBase),
            new PropertyMetadata(false));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            TextFilterInput = Template.FindName("PART_TextFilterInput", this) as TextBox;

            if (ItemsPanel != null)
            {
                XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                ItemsPanel.ItemTemplate = Xaml.FromString<DataTemplate>(new XDocument(
                    new XElement(pns + nameof(DataTemplate),
                        new XElement(pns + nameof(CheckBox),
                            new XAttribute(nameof(CheckBox.IsChecked), "{Binding Selected}"),
                            new XAttribute(nameof(CheckBox.Content), "{Binding Item}"),
                            new XAttribute(nameof(CheckBox.Style), $"{{DynamicResource FilterSelectionBoxItemStyle}}")))
                ).ToString());
            }

            if (TextFilterInput != null)
            {
                BindingTools.SetBinding(
                    TextFilterInput,
                    TextBox.VisibilityProperty,
                    this,
                    nameof(IsFullTextEnabled),
                    converter: new Converters.BooleanToVisibilityConverter());
            }

            if (TextFilterString != null)
            {
                BindingTools.SetBinding(
                    TextFilterString,
                    TextBox.VisibilityProperty,
                    this,
                    nameof(IsFullTextEnabled),
                    converter: new InvertedBooleanToVisibilityConverter());
            }
        }
    }

    public class FilterSelectionBox : FilterSelectionBoxBase
    {
        private BindingExpressionBase textInputBinding;

        public SelectableIdItemList ItemsList
        {
            get
            {
                return (SelectableIdItemList)GetValue(ItemsListProperty);
            }

            set
            {
                SetValue(ItemsListProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsListProperty = DependencyProperty.Register(
            nameof(ItemsList),
            typeof(SelectableIdItemList),
            typeof(FilterSelectionBox),
            new PropertyMetadata(null, ItemsListPropertyChangedCallback));

        private static void ItemsListPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var box = sender as FilterSelectionBox;
            if (box.IgnoreChanges)
            {
                return;
            }

            var oldVal = (SelectableIdItemList)e.OldValue;
            if (oldVal != null)
            {
                oldVal.SelectionChanged -= box.List_SelectionChanged;
            }

            var list = (SelectableIdItemList)e.NewValue;
            box.IgnoreChanges = true;
            list.SelectionChanged += box.List_SelectionChanged;
            if (box.FilterProperties != null)
            {
                list.SetSelection(box.FilterProperties.Ids);
            }

            box.UpdateTextStatus();
            box.IgnoreChanges = false;
        }

        public void List_SelectionChanged(object sender, EventArgs e)
        {
            if (!IgnoreChanges)
            {
                IgnoreChanges = true;
                FilterProperties = new IdItemFilterItemProperties { Ids = ItemsList.GetSelectedIds() };
                UpdateTextStatus();
                IgnoreChanges = false;
            }
        }

        public IdItemFilterItemProperties FilterProperties
        {
            get
            {
                return (IdItemFilterItemProperties)GetValue(FilterPropertiesProperty);
            }

            set
            {
                SetValue(FilterPropertiesProperty, value);
            }
        }

        public static readonly DependencyProperty FilterPropertiesProperty = DependencyProperty.Register(
            nameof(FilterProperties),
            typeof(IdItemFilterItemProperties),
            typeof(FilterSelectionBox),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, FilterPropertiesPropertyChangedCallback));

        private static void FilterPropertiesPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var box = sender as FilterSelectionBox;
            if (box.IgnoreChanges)
            {
                return;
            }

            box.IgnoreChanges = true;

            if (box.FilterProperties != null && box.FilterProperties.Text.IsNullOrEmpty())
            {
                box.ItemsList?.SetSelection(box.FilterProperties.Ids);
            }
            else if (box.FilterProperties == null)
            {
                box.ItemsList?.SetSelection(null);
            }

            box.UpdateTextStatus();
            box.IgnoreChanges = false;
        }

        public string FullTextText
        {
            get
            {
                if (FilterProperties == null)
                {
                    return null;
                }

                if (FilterProperties.Text.IsNullOrEmpty())
                {
                    return ItemsList?.AsString;
                }
                else
                {
                    return FilterProperties.Text;
                }
            }

            set
            {
                if (!IgnoreChanges)
                {
                    FilterProperties = new IdItemFilterItemProperties() { Text = value };
                    if (ItemsList != null)
                    {
                        IgnoreChanges = true;
                        ItemsList.SetSelection(null);
                        IgnoreChanges = false;
                    }
                }

                UpdateTextStatus();
            }
        }

        static FilterSelectionBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterSelectionBox), new FrameworkPropertyMetadata(typeof(FilterSelectionBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (TextFilterInput != null)
            {
                textInputBinding = BindingTools.SetBinding(
                    TextFilterInput,
                    TextBox.TextProperty,
                    this,
                    nameof(FullTextText),
                    delay: 200,
                    trigger: System.Windows.Data.UpdateSourceTrigger.PropertyChanged,
                    mode: BindingMode.TwoWay);
            }

            UpdateTextStatus();
        }

        public override void ClearButtonAction(RoutedEventArgs e)
        {
            FilterProperties = null;
            IgnoreChanges = true;
            ItemsList?.SetSelection(null);
            IgnoreChanges = false;
        }

        private void UpdateTextStatus()
        {
            if (TextFilterString != null)
            {
                TextFilterString.Text = FullTextText;
            }

            if (textInputBinding != null)
            {
                textInputBinding.UpdateTarget();
            }
        }
    }
}

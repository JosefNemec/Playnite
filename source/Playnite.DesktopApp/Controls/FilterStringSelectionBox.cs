using Playnite.Common;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.Controls
{
    public class FilterStringSelectionBox : FilterSelectionBoxBase
    {
        public SelectableObjectList<NamedObject<string>> ItemsList
        {
            get
            {
                return (SelectableObjectList<NamedObject<string>>)GetValue(ItemsListProperty);
            }

            set
            {
                SetValue(ItemsListProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsListProperty = DependencyProperty.Register(
            nameof(ItemsList),
            typeof(SelectableObjectList<NamedObject<string>>),
            typeof(FilterStringSelectionBox),
            new PropertyMetadata(null, ItemsListPropertyChangedCallback));

        private static void ItemsListPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as FilterStringSelectionBox;
            var oldVal = (SelectableObjectList<NamedObject<string>>)e.NewValue;
            if (oldVal != null)
            {
                oldVal.SelectionChanged -= obj.List_SelectionChanged;
            }

            var list = (SelectableObjectList<NamedObject<string>>)e.NewValue;
            obj.IgnoreChanges = true;
            list.SelectionChanged += obj.List_SelectionChanged;
            if (obj.FilterProperties != null)
            {
                list.SetSelection(obj.FilterProperties.Values?.Select(a => new NamedObject<string>(a)));
            }

            obj.IgnoreChanges = false;
            obj.UpdateTextStatus();
        }

        private void List_SelectionChanged(object sender, EventArgs e)
        {
            if (!IgnoreChanges)
            {
                IgnoreChanges = true;
                FilterProperties = new StringFilterItemProperties(ItemsList.GetSelectedItems().Select(a => a.Value).ToList());
                IgnoreChanges = false;
                UpdateTextStatus();
            }
        }

        public StringFilterItemProperties FilterProperties
        {
            get
            {
                return (StringFilterItemProperties)GetValue(FilterPropertiesProperty);
            }

            set
            {
                SetValue(FilterPropertiesProperty, value);
            }
        }

        public static readonly DependencyProperty FilterPropertiesProperty = DependencyProperty.Register(
            nameof(FilterProperties),
            typeof(StringFilterItemProperties),
            typeof(FilterStringSelectionBox),
            new PropertyMetadata(null, FilterPropertiesPropertyChangedCallback));

        private static void FilterPropertiesPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as FilterStringSelectionBox;
            if (obj.IgnoreChanges)
            {
                return;
            }

            obj.IgnoreChanges = true;
            if (obj.FilterProperties?.IsSet != true)
            {
                obj.ItemsList?.SetSelection(null);
            }
            else
            {
                obj.ItemsList?.SetSelection(obj.FilterProperties.Values?.Select(a => new NamedObject<string>(a)));
            }
            obj.IgnoreChanges = false;
            obj.UpdateTextStatus();
        }

        static FilterStringSelectionBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterStringSelectionBox), new FrameworkPropertyMetadata(typeof(FilterStringSelectionBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            UpdateTextStatus();
        }

        private void UpdateTextStatus()
        {
            if (TextFilterString != null)
            {
                TextFilterString.Text = ItemsList?.AsString;
            }
        }

        public override void ClearButtonAction(RoutedEventArgs e)
        {
            FilterProperties = null;
        }
    }
}

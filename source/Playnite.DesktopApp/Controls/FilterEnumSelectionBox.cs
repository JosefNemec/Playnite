using Playnite.Common;
using Playnite.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace Playnite.DesktopApp.Controls
{
    public class FilterEnumSelectionBox : FilterSelectionBoxBase
    {
        public List<SelectableItem<SelectionObject>> ItemsList { get; set; }

        public class SelectionObject
        {
            public string Name { get; }
            public int Value { get; }

            public SelectionObject(Enum enumValue)
            {
                Value = Convert.ToInt32(enumValue);
                Name = enumValue.GetDescription();
            }

            public override string ToString()
            {
                return Name;
            }
        }

        public Type EnumType
        {
            get
            {
                return (Type)GetValue(EnumTypeProperty);
            }

            set
            {
                SetValue(EnumTypeProperty, value);
            }
        }

        public static readonly DependencyProperty EnumTypeProperty = DependencyProperty.Register(
            nameof(EnumType),
            typeof(Type),
            typeof(FilterEnumSelectionBox),
            new PropertyMetadata(null, EnumTypePropertyChangedCallback));

        private static void EnumTypePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as FilterEnumSelectionBox;
            var list = (Type)e.NewValue;
            var items = new List<SelectableItem<SelectionObject>>();

            if (obj.ItemsList.HasItems())
            {
                foreach (var item in obj.ItemsList)
                {
                    item.PropertyChanged -= obj.NewItem_PropertyChanged;
                }
            }

            foreach (Enum en in list.GetEnumValues())
            {
                var newItem = new SelectableItem<SelectionObject>(new SelectionObject(en));
                if (obj.FilterProperties != null)
                {
                    newItem.Selected = obj.FilterProperties.Values?.Contains(newItem.Item.Value) == true;
                }

                newItem.PropertyChanged += obj.NewItem_PropertyChanged;
                items.Add(newItem);
            }

            obj.ItemsList = items;
        }

        private void NewItem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IgnoreChanges)
            {
                return;
            }

            if (e.PropertyName == nameof(SelectableItem<SelectionObject>.Selected))
            {
                var selected = ItemsList.Where(a => a.Selected == true);
                if (selected.HasItems())
                {
                    FilterProperties = new EnumFilterItemProperties(selected.Select(a => a.Item.Value).ToList());
                }
                else
                {
                    FilterProperties = null;
                }
            }
        }

        public EnumFilterItemProperties FilterProperties
        {
            get
            {
                return (EnumFilterItemProperties)GetValue(FilterPropertiesProperty);
            }

            set
            {
                SetValue(FilterPropertiesProperty, value);
            }
        }

        public static readonly DependencyProperty FilterPropertiesProperty = DependencyProperty.Register(
            nameof(FilterProperties),
            typeof(EnumFilterItemProperties),
            typeof(FilterEnumSelectionBox),
            new PropertyMetadata(null, FilterPropertiesPropertyChangedCallback));

        private static void FilterPropertiesPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as FilterEnumSelectionBox;
            if (obj.IgnoreChanges)
            {
                return;
            }

            obj.IgnoreChanges = true;
            if (obj.FilterProperties?.IsSet != true)
            {
                obj.ItemsList?.ForEach(a => a.Selected = false);
            }
            else
            {
                obj.ItemsList?.ForEach(a => a.Selected = obj.FilterProperties.Values.Contains(a.Item.Value));
            }
            obj.IgnoreChanges = false;
            obj.UpdateTextStatus();
        }

        static FilterEnumSelectionBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterEnumSelectionBox), new FrameworkPropertyMetadata(typeof(FilterEnumSelectionBox)));
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
                if (ItemsList.HasItems())
                {
                    TextFilterString.Text = string.Join(", ", ItemsList.Where(a => a.Selected == true).Select(a => a.Item.Name).ToArray());
                }
                else
                {
                    TextFilterString.Text = string.Empty;
                }
            }
        }

        public override void ClearButtonAction(RoutedEventArgs e)
        {
            FilterProperties = null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Playnite.DesktopApp.Controls
{
    /// <summary>
    /// Interaction logic for DdItemListSelectionBox.xaml
    /// </summary>
    public partial class FilterEnumSelectionBox : UserControl, INotifyPropertyChanged
    {
        public class SelectionObject
        {
            public string DisplayName { get; }
            public int Value { get; }

            public SelectionObject(Enum enumValue)
            {
                Value = Convert.ToInt32(enumValue);
                DisplayName = enumValue.GetDescription();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        internal bool IgnoreChanges { get; set; }

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
                OnPropertyChanged(nameof(SelectionString));
                var selected = ItemsList.Where(a => a.Selected == true);
                if (selected.HasItems())
                {
                    FilterProperties = new EnumFilterItemProperites(selected.Select(a => a.Item.Value).ToList());
                }
                else
                {
                    FilterProperties = null;
                }
            }
        }

        public List<SelectableItem<SelectionObject>> ItemsList { get; set; }

        public string SelectionString
        {
            get
            {
                if (ItemsList.HasItems())
                {
                    return string.Join(", ", ItemsList.Where(a => a.Selected == true).Select(a => a.Item.DisplayName).ToArray());
                }

                return string.Empty;
            }
        }

        public EnumFilterItemProperites FilterProperties
        {
            get
            {
                return (EnumFilterItemProperites)GetValue(FilterPropertiesProperty);
            }

            set
            {
                SetValue(FilterPropertiesProperty, value);
            }
        }

        public static readonly DependencyProperty FilterPropertiesProperty = DependencyProperty.Register(
            nameof(FilterProperties),
            typeof(EnumFilterItemProperites),
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
            obj.OnPropertyChanged(nameof(obj.SelectionString));
        }

        public FilterEnumSelectionBox()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            FilterProperties = null;
        }
    }
}

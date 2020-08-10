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
    public partial class FilterStringSelectionBox : UserControl
    {
        internal bool IgnoreChanges { get; set; }

        public SelectableStringList ItemsList
        {
            get
            {
                return (SelectableStringList)GetValue(ItemsListProperty);
            }

            set
            {
                SetValue(ItemsListProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsListProperty = DependencyProperty.Register(
            nameof(ItemsList),
            typeof(SelectableStringList),
            typeof(FilterStringSelectionBox),
            new PropertyMetadata(null, ItemsListPropertyChangedCallback));

        private static void ItemsListPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as FilterStringSelectionBox;
            var oldVal = (SelectableStringList)e.NewValue;
            if (oldVal != null)
            {
                oldVal.SelectionChanged -= obj.List_SelectionChanged;
            }

            var list = (SelectableStringList)e.NewValue;
            obj.IgnoreChanges = true;
            list.SelectionChanged += obj.List_SelectionChanged;
            if (obj.FilterProperties != null)
            {
                list.SetSelection(obj.FilterProperties.Values);
            }

            obj.IgnoreChanges = false;
        }

        private void List_SelectionChanged(object sender, EventArgs e)
        {
            if (!IgnoreChanges)
            {
                IgnoreChanges = true;
                FilterProperties = new StringFilterItemProperites(ItemsList.GetSelectedItems());
                IgnoreChanges = false;
            }
        }

        public StringFilterItemProperites FilterProperties
        {
            get
            {
                return (StringFilterItemProperites)GetValue(FilterPropertiesProperty);
            }

            set
            {
                SetValue(FilterPropertiesProperty, value);
            }
        }

        public static readonly DependencyProperty FilterPropertiesProperty = DependencyProperty.Register(
            nameof(FilterProperties),
            typeof(StringFilterItemProperites),
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
                obj.ItemsList?.SetSelection(obj.FilterProperties.Values);
            }
            obj.IgnoreChanges = false;
        }

        public FilterStringSelectionBox()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            FilterProperties = null;
        }
    }
}

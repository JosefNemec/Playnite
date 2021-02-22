using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    public partial class DdItemListSelectionBox : UserControl
    {
        internal bool IgnoreChanges { get; set; }

        public bool UseSearchBox
        {
            get
            {
                return (bool)GetValue(UseSearchBoxProperty);
            }

            set
            {
                SetValue(UseSearchBoxProperty, value);
            }
        }

        public static readonly DependencyProperty UseSearchBoxProperty = DependencyProperty.Register(
            nameof(UseSearchBox),
            typeof(bool),
            typeof(DdItemListSelectionBox),
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
        }

        private void List_SelectionChanged(object sender, EventArgs e)
        {
            if (!IgnoreChanges)
            {
                IgnoreChanges = true;
                BoundIds = ItemsList.GetSelectedIds();
                IgnoreChanges = false;
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

        public bool IsThreeState
        {
            get
            {
                return (bool)GetValue(IsThreeStateProperty);
            }

            set
            {
                SetValue(IsThreeStateProperty, value);
            }
        }

        public static readonly DependencyProperty IsThreeStateProperty = DependencyProperty.Register(
            nameof(IsThreeState),
            typeof(bool),
            typeof(DdItemListSelectionBox));

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
        }

        public DdItemListSelectionBox()
        {
            InitializeComponent();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            ItemsList.SetSelection(null);
            BoundIds = null;
        }


        private void PART_SearchBox_KeyUp(object sender, KeyEventArgs e)
        {
            SearchBox PART_SearchBox = sender as SearchBox;
            ItemsList.SearchItemsByString(PART_SearchBox.Text);
        }

        private void PART_SearchCheckedOnly_Click(object sender, RoutedEventArgs e)
        {
            ToggleButton PART_SearchCheckedOnly = sender as ToggleButton;
            ItemsList.SearchItemsByChecked((bool)PART_SearchCheckedOnly.IsChecked);
        }
    }
}

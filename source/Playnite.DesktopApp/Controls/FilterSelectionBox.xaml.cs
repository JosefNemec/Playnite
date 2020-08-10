using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    /// Interaction logic for FilterSelectionBox.xaml
    /// </summary>
    public partial class FilterSelectionBox : UserControl, INotifyPropertyChanged
    {
        internal bool IgnoreChanges { get; set; }

        public bool IsFullTextEnabled
        {
            get
            {
                return (bool)GetValue(IsFullTextEnabledProperty);
            }

            set
            {
                SetValue(IsFullTextEnabledProperty, value);
            }
        }

        public static readonly DependencyProperty IsFullTextEnabledProperty = DependencyProperty.Register(
            nameof(IsFullTextEnabled),
            typeof(bool),
            typeof(FilterSelectionBox),
            new PropertyMetadata(true));

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

            box.OnFullTextTextChanged();
            box.IgnoreChanges = false;
        }

        public void List_SelectionChanged(object sender, EventArgs e)
        {
            if (!IgnoreChanges)
            {
                IgnoreChanges = true;
                FilterProperties = new FilterItemProperites { Ids = ItemsList.GetSelectedIds() };
                OnFullTextTextChanged();
                IgnoreChanges = false;
            }
        }

        public FilterItemProperites FilterProperties
        {
            get
            {
                return (FilterItemProperites)GetValue(FilterPropertiesProperty);
            }

            set
            {
                SetValue(FilterPropertiesProperty, value);
            }
        }

        public static readonly DependencyProperty FilterPropertiesProperty = DependencyProperty.Register(
            nameof(FilterProperties),
            typeof(FilterItemProperites),
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

            box.OnFullTextTextChanged();
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
                    FilterProperties = new FilterItemProperites() { Text = value };
                    if (ItemsList != null)
                    {
                        IgnoreChanges = true;
                        ItemsList.SetSelection(null);
                        IgnoreChanges = false;
                    }
                }

                OnFullTextTextChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public FilterSelectionBox()
        {
            InitializeComponent();
        }

        internal void OnFullTextTextChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FullTextText)));
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            FilterProperties = null;
            IgnoreChanges = true;
            ItemsList?.SetSelection(null);
            IgnoreChanges = false;
        }
    }
}

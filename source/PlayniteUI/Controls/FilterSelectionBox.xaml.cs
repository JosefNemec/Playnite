using Playnite;
using Playnite.SDK.Models;
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

namespace PlayniteUI.Controls
{
    /// <summary>
    /// Interaction logic for FilterSelectionBox.xaml
    /// </summary>
    public partial class FilterSelectionBox : UserControl, INotifyPropertyChanged
    {
        internal bool IgnoreChanges { get; set; }

        public SelectableItemList ItemsList
        {
            get
            {
                return (SelectableItemList)GetValue(ItemsListProperty);
            }

            set
            {
                SetValue(ItemsListProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsListProperty = DependencyProperty.Register(
            nameof(ItemsList),
            typeof(SelectableItemList),
            typeof(FilterSelectionBox),
            new PropertyMetadata(null, ItemsListPropertyChangedCallback));

        private static void ItemsListPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var box = sender as FilterSelectionBox;
            if (box.IgnoreChanges)
            {
                return;
            }

            var list = (SelectableItemList)e.NewValue;
            box.IgnoreChanges = true;
            list.SelectionChanged += (s, a) =>
            {
                if (!box.IgnoreChanges)
                {
                    box.IgnoreChanges = true;
                    box.FilterProperties = new FilterItemProperites { Ids = list.GetSelectedIds() };
                    box.OnFullTextTextChanged();
                    box.IgnoreChanges = false;
                }
            };

            if (box.FilterProperties != null)
            {
                list.SetSelection(box.FilterProperties.Ids);
            }

            box.OnFullTextTextChanged();
            box.IgnoreChanges = false;
        }

        public FilterItemProperites FilterProperties
        {
            get
            {
                return (FilterItemProperites)GetValue(BoundIdsProperty);
            }

            set
            {
                SetValue(BoundIdsProperty, value);
            }
        }

        public static readonly DependencyProperty BoundIdsProperty = DependencyProperty.Register(
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
    }
}

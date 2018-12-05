using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for DdItemListSelectionBox.xaml
    /// </summary>
    public partial class DdItemListSelectionBox : UserControl
    {
        internal bool IgnoreChanges { get; set; }

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

        public static readonly DependencyProperty ItemsListProperty = DependencyProperty.Register("ItemsList", typeof(SelectableDbItemList), typeof(DdItemListSelectionBox), new PropertyMetadata(null, ItemsListPropertyChangedCallback));

        private static void ItemsListPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as DdItemListSelectionBox;
            var list = (SelectableDbItemList)e.NewValue;
            list.SelectionChanged += (s, a) =>
            {
                if (!obj.IgnoreChanges)
                {
                    obj.IgnoreChanges = true;
                    obj.BoundIds = list.GetSelectedIds();
                    obj.IgnoreChanges = false;
                }
            };
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

        public static readonly DependencyProperty BoundIdsProperty = DependencyProperty.Register("BoundIds", typeof(object), typeof(DdItemListSelectionBox), new PropertyMetadata(null, BoundIdsPropertyChangedCallback));

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

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }

            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(DdItemListSelectionBox), new PropertyMetadata(string.Empty, TextPropertyChangedCallback));

        private static void TextPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as DdItemListSelectionBox;
            obj.TextFilter.Text = (string)e.NewValue;
            obj.TextFilter.CaretIndex = obj.TextFilter.Text.Length;
        }

        public bool IsEditable
        {
            get
            {
                return (bool)GetValue(IsEditableProperty);
            }

            set
            {
                SetValue(IsEditableProperty, value);
            }
        }

        public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(DdItemListSelectionBox), new PropertyMetadata(false, IsEditablePropertyChangedCallback));

        private static void IsEditablePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            //var obj = sender as DdItemListSelectionBox;
            //obj.TextFilter.Text = (string)e.NewValue;
            //obj.TextFilter.CaretIndex = obj.TextFilter.Text.Length;
        }

        public DdItemListSelectionBox()
        {
            InitializeComponent();
        }
    }
}

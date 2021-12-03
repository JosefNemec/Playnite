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

namespace Playnite.Controls
{
    public class ExtendedListBox : ListBox
    {
        internal bool ignoreSelectedItemsListChanges = false;

        static ExtendedListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExtendedListBox), new FrameworkPropertyMetadata(typeof(ExtendedListBox)));
        }

        public ExtendedListBox()
        {
            SelectionChanged += ExtendedListBox_SelectionChanged;
        }

        private void ExtendedListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ignoreSelectedItemsListChanges = true;
            SelectedItemsList = (IList<object>)SelectedItems;
            ignoreSelectedItemsListChanges = false;
        }

        public IList<object> SelectedItemsList
        {
            get
            {
                return (IList<object>)GetValue(SelectedItemsListProperty);
            }

            set
            {
                SetValue(SelectedItemsListProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
           DependencyProperty.Register(
               nameof(SelectedItemsList),
               typeof(IList<object>),
               typeof(ExtendedListBox),
               new PropertyMetadata(null, SelectedItemsListChanged));

        public static void SelectedItemsListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var list = (ExtendedListBox)d;
            if (list.ignoreSelectedItemsListChanges || list.SelectionMode == SelectionMode.Single)
            {
                return;
            }

            list.SelectedItems.Clear();
            var newValues = e.NewValue as IList<object>;
            if (newValues.HasItems())
            {
                newValues.ForEach(a => list.SelectedItems.Add(a));
            }
        }
    }
}

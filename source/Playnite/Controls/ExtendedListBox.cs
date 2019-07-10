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
            SelectedItemsList = (IList<object>)SelectedItems;
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
               new PropertyMetadata(null));
    }
}

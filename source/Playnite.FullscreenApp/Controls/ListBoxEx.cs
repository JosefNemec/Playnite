using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Playnite.Input;

namespace Playnite.FullscreenApp.Controls
{
    public class ListBoxEx : ListBox
    {
        static ListBoxEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListBoxEx), new FrameworkPropertyMetadata(typeof(ListBoxEx)));
        }

        public ListBoxEx() : base()
        {
            SelectionChanged += ListBoxEx_SelectionChanged;
            this.GotFocus += ListBoxEx_GotFocus;
        }

        private void ListBoxEx_GotFocus(object sender, RoutedEventArgs e)
        {
            FocusSelected();
        }

        private void ListBoxEx_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FocusSelected();
        }

        private void FocusSelected()
        {
            if (IsFocused)
            {
                if (SelectedItem == null && Items.Count > 0)
                {
                    SelectedItem = Items[0];
                }

                if (SelectedItem != null)
                {
                    var selItem = ItemContainerGenerator.ContainerFromItem(SelectedItem) as FrameworkElement;
                    if (selItem == null)
                    {
                        UpdateLayout();
                        selItem = ItemContainerGenerator.ContainerFromItem(SelectedItem) as FrameworkElement;
                    }

                    if (selItem != null)
                    {
                        selItem.Focus();
                        selItem.BringIntoView();
                    }
                }
            }
        }
    }
}

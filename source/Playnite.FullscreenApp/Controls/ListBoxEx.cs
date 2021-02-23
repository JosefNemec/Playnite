using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        private FullscreenTilePanel itemsPanel;
        static ListBoxEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ListBoxEx), new FrameworkPropertyMetadata(typeof(ListBoxEx)));
        }

        public ListBoxEx() : base()
        {
            SelectionChanged += ListBoxEx_SelectionChanged;
            GotFocus += ListBoxEx_GotFocus;
            Loaded += ListBoxEx_Loaded;
            Unloaded += ListBoxEx_Unloaded;
        }

        private void ListBoxEx_Unloaded(object sender, RoutedEventArgs e)
        {
            itemsPanel.InternalChildrenGenerated -= ItemsPanel_InternalChildrenGenerated;
        }

        private void ListBoxEx_Loaded(object sender, RoutedEventArgs e)
        {
            itemsPanel = ElementTreeHelper.FindVisualChildren<FullscreenTilePanel>(this).FirstOrDefault();
            itemsPanel.InternalChildrenGenerated += ItemsPanel_InternalChildrenGenerated; ;
        }

        private void ItemsPanel_InternalChildrenGenerated(object sender, InternalChildrenGeneratedArgs e)
        {
            FocusSelected();
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
                        if (selItem == null)
                        {
                            itemsPanel.ScrollToItem(SelectedItem);
                        }
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

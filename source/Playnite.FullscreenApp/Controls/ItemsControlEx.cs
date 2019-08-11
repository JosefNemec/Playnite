using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Playnite.FullscreenApp.Controls
{
    public class ItemsControlEx : ItemsControl
    {
        static ItemsControlEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ItemsControlEx), new FrameworkPropertyMetadata(typeof(ItemsControlEx)));
        }

        public ItemsControlEx() : base()
        {
            PreviewKeyDown += ItemsControlEx_PreviewKeyDown;
        }

        private void ItemsControlEx_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                e.Handled = true;
            }
            else if (e.Key == Key.Down && Items.Count > 0)
            {
                var currentElem = (FrameworkElement)Keyboard.FocusedElement;
                var lastItem = ItemContainerGenerator.ContainerFromIndex(Items.Count - 1);
                if (lastItem != null)
                {
                    if (lastItem is ContentPresenter)
                    {
                        lastItem = VisualTreeHelper.GetChild(lastItem, 0);
                    }

                    if (lastItem == currentElem)
                    {
                        MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                        e.Handled = true;
                    }
                }
            }
            else if (e.Key == Key.Up && Items.Count > 0)
            {
                var currentElem = (FrameworkElement)Keyboard.FocusedElement;
                var firstElem = ItemContainerGenerator.ContainerFromIndex(0);
                if (firstElem != null)
                {
                    if (firstElem is ContentPresenter)
                    {
                        firstElem = VisualTreeHelper.GetChild(firstElem, 0);
                    }

                    if (firstElem == currentElem)
                    {
                        MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                        e.Handled = true;
                    }
                }
            }
        }
    }
}

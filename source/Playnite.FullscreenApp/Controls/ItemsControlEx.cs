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
        public bool HorizontalLayout
        {
            get { return (bool)GetValue(HorizontalLayoutProperty); }
            set { SetValue(HorizontalLayoutProperty, value); }
        }

        public static readonly DependencyProperty HorizontalLayoutProperty =
            DependencyProperty.Register(
                nameof(HorizontalLayout),
                typeof(bool),
                typeof(ItemsControlEx));

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
            var focusedElem = Keyboard.FocusedElement as FrameworkElement;

            if (HorizontalLayout)
            {
                if (e.Key == Key.Up)
                {
                    focusedElem?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                    e.Handled = true;
                }
                else if (e.Key == Key.Down)
                {
                    focusedElem?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                    e.Handled = true;
                }
                else if (e.Key == Key.Right && Items.Count > 0)
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
                            focusedElem?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                            e.Handled = true;
                        }
                    }
                }
                else if (e.Key == Key.Left && Items.Count > 0)
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
                            focusedElem?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                            e.Handled = true;
                        }
                    }
                }
            }
            else
            {
                if (e.Key == Key.Right)
                {
                    focusedElem?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                    e.Handled = true;
                }
                else if (e.Key == Key.Left)
                {
                    focusedElem?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
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
                            focusedElem?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
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
                            focusedElem?.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                            e.Handled = true;
                        }
                    }
                }
            }
        }
    }
}

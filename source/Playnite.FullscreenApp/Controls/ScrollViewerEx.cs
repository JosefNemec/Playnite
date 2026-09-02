using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Playnite.FullscreenApp.Controls
{
    public class ScrollViewerEx : ScrollViewer
    {
        private bool focusMovedWithin;
        public double CustomScrollAmount { get; set; } = 0;

        static ScrollViewerEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ScrollViewerEx), new FrameworkPropertyMetadata(typeof(ScrollViewerEx)));
        }

        public ScrollViewerEx() : base()
        {
            Loaded += ScrollViewerEx_Loaded;
            Unloaded += ScrollViewerEx_Unloaded;
        }

        private void ScrollViewerEx_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollChanged += ScrollViewerEx_ScrollChanged;
            PreviewKeyDown += ScrollViewerEx_PreviewKeyDown;
            GotKeyboardFocus += ScrollViewerEx_GotKeyboardFocus;
        }

        private void ScrollViewerEx_Unloaded(object sender, RoutedEventArgs e)
        {
            ScrollChanged -= ScrollViewerEx_ScrollChanged;
            PreviewKeyDown -= ScrollViewerEx_PreviewKeyDown;
            GotKeyboardFocus -= ScrollViewerEx_GotKeyboardFocus;
        }

        private void ScrollViewerEx_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            focusMovedWithin = e.NewFocus is DependencyObject newFocus && IsAncestorOf(newFocus);
        }

        private void ScrollViewerEx_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ComputedHorizontalScrollBarVisibility != Visibility.Visible)
            {
                if (e.Key == Key.Left)
                {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Left));
                    e.Handled = true;
                    return;
                }
                else if (e.Key == Key.Right)
                {
                    MoveFocus(new TraversalRequest(FocusNavigationDirection.Right));
                    e.Handled = true;
                    return;
                }
            }

            if (e.Key == Key.Up && VerticalOffset == 0)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                e.Handled = true;
                return;
            }
            else if (e.Key == Key.Down && VerticalOffset >= ScrollableHeight)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                e.Handled = true;
                return;
            }
        }

        private void ScrollViewerEx_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var focusJustMoved = focusMovedWithin;
            focusMovedWithin = false;

            if (e.VerticalChange == 0 && e.HorizontalChange == 0)
            {
                return;
            }

            if (VerticalOffset == 0)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                e.Handled = true;
            }
            else if (!focusJustMoved && VerticalOffset >= ScrollableHeight)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                e.Handled = true;
            }
            else if (CustomScrollAmount > 0)
            {
                if (Math.Abs(e.VerticalChange) >= CustomScrollAmount)
                    return;

                if (e.VerticalChange > 0)
                    ScrollToVerticalOffset(VerticalOffset + CustomScrollAmount);
                else
                    ScrollToVerticalOffset(VerticalOffset - CustomScrollAmount);

                e.Handled = true;
            }
        }
    }
}

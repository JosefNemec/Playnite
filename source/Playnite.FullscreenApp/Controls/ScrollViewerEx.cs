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
        }

        private void ScrollViewerEx_Unloaded(object sender, RoutedEventArgs e)
        {
            ScrollChanged -= ScrollViewerEx_ScrollChanged;
            PreviewKeyDown -= ScrollViewerEx_PreviewKeyDown;
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
            if (e.VerticalChange == 0 && e.HorizontalChange == 0)
            {
                return;
            }

            if (VerticalOffset == 0)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                e.Handled = true;
            }
            else if (VerticalOffset >= ScrollableHeight)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                e.Handled = true;
            }
        }
    }
}

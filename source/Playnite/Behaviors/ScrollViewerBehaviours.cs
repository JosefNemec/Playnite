using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Playnite.Behaviors
{
    public class ScrollViewerBehaviours
    {
        public static readonly DependencyProperty ScrollAmountProperty =
            DependencyProperty.RegisterAttached(
            "ScrollAmount",
            typeof(double),
            typeof(ScrollViewerBehaviours),
            new PropertyMetadata(new PropertyChangedCallback(ScrollAmountPropertyChanged)));

        public static double GetScrollAmount(DependencyObject obj)
        {
            return (double)obj.GetValue(ScrollAmountProperty);
        }

        public static void SetScrollAmount(DependencyObject obj, double value)
        {
            obj.SetValue(ScrollAmountProperty, value);
        }

        private static void ScrollAmountPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (DesignerProperties.GetIsInDesignMode(obj))
            {
                return;
            }

            if (obj is ScrollViewer scr)
            {
                scr.PreviewMouseWheel -= Test_PreviewMouseWheel;
                if ((double)args.NewValue > 0)
                {
                    scr.PreviewMouseWheel += Test_PreviewMouseWheel;
                }
            }
            else
            {
                var control = (FrameworkElement)obj;
                if (control.IsLoaded)
                {
                    var scroll = ElementTreeHelper.FindVisualChildren<ScrollViewer>(obj).FirstOrDefault();
                    if (scroll != null)
                    {
                        SetScrollAmount(scroll, (double)args.NewValue);
                    }
                }
                else
                {
                    control.Loaded += (a, b) =>
                    {
                        var ctrl = (FrameworkElement)a;
                        var scroll = ElementTreeHelper.FindVisualChildren<ScrollViewer>(control).FirstOrDefault();
                        if (scroll != null)
                        {
                            SetScrollAmount(scroll, (double)args.NewValue);
                        }
                    };
                }
            }
        }

        private static void Test_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var scroll = (ScrollViewer)sender;
            var amount = GetScrollAmount(scroll);
            if (amount > 0)
            {
                if (!e.Handled && scroll.ComputedVerticalScrollBarVisibility == Visibility.Visible)
                {
                    scroll.ScrollToVerticalOffset(scroll.VerticalOffset - e.Delta * amount);
                    e.Handled = true;
                }
            }
        }
    }
}

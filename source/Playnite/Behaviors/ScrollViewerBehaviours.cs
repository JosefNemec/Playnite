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
    public class ScrollAnimationData
    {
        private readonly ScrollViewer scroller;

        public bool IsAnimating { get; private set; }
        public FrameworkElement ScrollOwner { get; set; }
        public double TargetOffset { get; set; }
        public Storyboard Storyboard { get; private set; }
        public DoubleAnimation Animation { get; private set; }

        public ScrollAnimationData(ScrollViewer scroller, FrameworkElement owner)
        {
            this.scroller = scroller;
            ScrollOwner = owner;
            Animation = new DoubleAnimation();
            Animation.Completed += Storyboard_Completed;
        }

        private void Storyboard_Completed(object sender, EventArgs e)
        {
            IsAnimating = false;
            TargetOffset = scroller.VerticalOffset;
        }

        public void BeginAnimation(double to, TimeSpan speed)
        {
            TargetOffset = to;
            Animation.From = scroller.VerticalOffset;
            Animation.To = TargetOffset;
            Animation.Duration = new Duration(speed);
            scroller.BeginAnimation(ScrollViewerBehaviours.VerticalOffsetProperty, Animation);
            IsAnimating = true;
        }
    }

    public class ScrollViewerBehaviours
    {
        // ---------------- VerticalOffset
        // This is "internal" property only, should NOT be used in any other place.
        public static DependencyProperty ScrollDataProperty = DependencyProperty.RegisterAttached(
            "ScrollData",
            typeof(ScrollAnimationData),
            typeof(ScrollViewerBehaviours),
            new PropertyMetadata(null));

        public static void SetScrollData(FrameworkElement target, ScrollAnimationData value)
        {
            target.SetValue(ScrollDataProperty, value);
        }

        public static ScrollAnimationData GetScrollData(FrameworkElement target)
        {
            return (ScrollAnimationData)target.GetValue(ScrollDataProperty);
        }

        // ---------------- VerticalOffset
        // This is "internal" property only, should NOT be used in any other place.
        // It's done this way so vertical offset can be driven by an animation.
        public static DependencyProperty VerticalOffsetProperty = DependencyProperty.RegisterAttached(
            "VerticalOffset",
            typeof(double),
            typeof(ScrollViewerBehaviours),
            new PropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static void SetVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(VerticalOffsetProperty, value);
        }

        public static double GetVerticalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(VerticalOffsetProperty);
        }

        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (target is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        // ---------------- ScrollAmount
        public static readonly DependencyProperty SensitivityProperty = DependencyProperty.RegisterAttached(
            "Sensitivity",
            typeof(double),
            typeof(ScrollViewerBehaviours),
            new PropertyMetadata(0.0));

        public static void SetSensitivity(DependencyObject obj, double value)
        {
            obj.SetValue(SensitivityProperty, value);
        }

        public static double GetSensitivity(DependencyObject obj)
        {
            return (double)obj.GetValue(SensitivityProperty);
        }

        // ---------------- TimeDuration
        public static DependencyProperty SpeedProperty = DependencyProperty.RegisterAttached(
            "Speed",
            typeof(TimeSpan),
            typeof(ScrollViewerBehaviours),
            new PropertyMetadata(new TimeSpan(0, 0, 0, 0, 250)));

        public static void SetSpeed(FrameworkElement target, TimeSpan value)
        {
            target.SetValue(SpeedProperty, value);
        }

        public static TimeSpan GetSpeed(FrameworkElement target)
        {
            return (TimeSpan)target.GetValue(SpeedProperty);
        }

        // ---------------- SmoothScrollEnabled
        public static DependencyProperty SmoothScrollEnabledProperty = DependencyProperty.RegisterAttached(
            "SmoothScrollEnabled",
            typeof(bool),
            typeof(ScrollViewerBehaviours),
            new PropertyMetadata(false));

        public static void SetSmoothScrollEnabled(FrameworkElement target, bool value)
        {
            target.SetValue(SmoothScrollEnabledProperty, value);
        }

        public static bool GetSmoothScrollEnabled(FrameworkElement target)
        {
            return (bool)target.GetValue(SmoothScrollEnabledProperty);
        }

        // ---------------- CustomScrollEnabled
        public static DependencyProperty CustomScrollEnabledProperty = DependencyProperty.RegisterAttached(
            "CustomScrollEnabled",
            typeof(bool),
            typeof(ScrollViewerBehaviours),
            new PropertyMetadata(false, OnCustomScrollEnabledChanged));

        public static void SetCustomScrollEnabled(FrameworkElement target, bool value)
        {
            target.SetValue(CustomScrollEnabledProperty, value);
        }

        public static bool GetCustomScrollEnabled(FrameworkElement target)
        {
            return (bool)target.GetValue(CustomScrollEnabledProperty);
        }

        private static void OnCustomScrollEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (DesignerProperties.GetIsInDesignMode(obj))
            {
                return;
            }

            var control = (FrameworkElement)obj;
            if (obj is ScrollViewer scr)
            {
                SetScrollerEvents(control, scr, (bool)args.NewValue);
            }
            else
            {
                if (control.IsLoaded)
                {
                    var scroll = ElementTreeHelper.FindVisualChildren<ScrollViewer>(obj).FirstOrDefault();
                    if (scroll != null)
                    {
                        SetScrollerEvents(control, scroll, (bool)args.NewValue);
                    }
                }
                else
                {
                    void controlLoaded(object a, RoutedEventArgs e)
                    {
                        var ctrl = (FrameworkElement)a;
                        ctrl.Loaded -= controlLoaded;
                        var scroll = ElementTreeHelper.FindVisualChildren<ScrollViewer>(ctrl).FirstOrDefault();
                        if (scroll != null)
                        {
                            SetScrollerEvents(control, scroll, (bool)args.NewValue);
                        }
                    }

                    control.Loaded += controlLoaded;
                }
            }
        }

        private static void SetScrollerEvents(FrameworkElement owner, ScrollViewer scroller, bool hook)
        {
            if (hook)
            {
                SetScrollData(scroller, new ScrollAnimationData(scroller, owner));
                scroller.PreviewMouseWheel += PreviewMouseWheel;
            }
            else
            {
                SetScrollData(scroller, null);
                scroller.PreviewMouseWheel -= PreviewMouseWheel;
            }
        }

        private static void PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Handled)
            {
                return;
            }

            var mouseDelta = (double)e.Delta;
            var scrollData = GetScrollData(sender as FrameworkElement);
            var sensitivity = GetSensitivity(scrollData.ScrollOwner);
            var scroll = sender as ScrollViewer;
            if (scroll.ComputedVerticalScrollBarVisibility != Visibility.Visible)
            {
                return;
            }

            if (GetSmoothScrollEnabled(scrollData.ScrollOwner))
            {
                var targetOffset = scrollData.TargetOffset;
                if (!scrollData.IsAnimating)
                {
                    targetOffset = scroll.VerticalOffset;
                }

                var newOffset = targetOffset - (mouseDelta * sensitivity);
                if (newOffset < 0)
                {
                    newOffset = 0;
                }
                if (newOffset > scroll.ScrollableHeight)
                {
                    newOffset = scroll.ScrollableHeight;
                }

                scrollData.BeginAnimation(newOffset, GetSpeed(scrollData.ScrollOwner));
            }
            else
            {
                scroll.ScrollToVerticalOffset(scroll.VerticalOffset - e.Delta * sensitivity);
            }

            e.Handled = true;
        }
    }
}

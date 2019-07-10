using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Playnite.Behaviors
{
    public class AnimatedVisibility
    {
        #region Visibility

        private static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.RegisterAttached(
                "Visibility",
                typeof(Visibility),
                typeof(AnimatedVisibility),
                new PropertyMetadata(new PropertyChangedCallback(HandleVisibilityChanged)));

        public static Visibility GetVisibility(DependencyObject obj)
        {
            return (Visibility)obj.GetValue(VisibilityProperty);
        }

        public static void SetVisibility(DependencyObject obj, Visibility value)
        {
            obj.SetValue(VisibilityProperty, value);
        }

        private static void HandleVisibilityChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (FrameworkElement)obj;
            if (DesignerProperties.GetIsInDesignMode(control))
            {
                return;
            }

            var visibility = (Visibility)args.NewValue;
            if (visibility == Visibility.Visible)
            {
                control.Visibility = visibility;
                var anim = GetVisible(control);
                anim?.Begin();
            }
            else if (visibility == Visibility.Collapsed)
            {
                var anim = GetCollapsed(control);
                if (anim == null)
                {
                    control.Visibility = Visibility.Collapsed;
                }
                else
                {
                    anim.Begin();
                }
            }
        }

        #endregion Visibility

        #region Visible

        private static readonly DependencyProperty VisibleProperty =
            DependencyProperty.RegisterAttached(
            "Visible",
            typeof(Storyboard),
            typeof(AnimatedVisibility));

        public static Storyboard GetVisible(DependencyObject obj)
        {
            return (Storyboard)obj.GetValue(VisibleProperty);
        }

        public static void SetVisible(DependencyObject obj, Storyboard value)
        {
            obj.SetValue(VisibleProperty, value);
        }

        #endregion Visible

        #region Collapsed

        private static readonly DependencyProperty CollapsedProperty =
            DependencyProperty.RegisterAttached(
            "Collapsed",
            typeof(Storyboard),
            typeof(AnimatedVisibility),
            new PropertyMetadata(new PropertyChangedCallback(HandleCollapsedChanged)));

        public static Storyboard GetCollapsed(DependencyObject obj)
        {
            return (Storyboard)obj.GetValue(CollapsedProperty);
        }

        public static void SetCollapsed(DependencyObject obj, Storyboard value)
        {
            obj.SetValue(CollapsedProperty, value);
        }

        private static void HandleCollapsedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (FrameworkElement)obj;
            if (DesignerProperties.GetIsInDesignMode(control))
            {
                return;
            }

            void handler(object s, EventArgs e)
            {
                control.Visibility = Visibility.Collapsed;
            }

            if (args.NewValue != args.OldValue)
            {
                if (args.OldValue != null)
                {
                    var sb = (Storyboard)args.OldValue;
                    sb.Completed -= handler;
                }

                if (args.NewValue != null)
                {
                    var sb = (Storyboard)args.NewValue;
                    sb.Completed += handler;
                }
            }
        }

        #endregion Collapsed
    }
}

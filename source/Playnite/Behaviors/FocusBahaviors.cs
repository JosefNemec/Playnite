using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Playnite.Behaviors
{
    public class FocusBahaviors
    {
        private static readonly DependencyProperty OnVisibilityFocusProperty =
            DependencyProperty.RegisterAttached("OnVisibilityFocus", typeof(bool), typeof(FocusBahaviors), new PropertyMetadata(new PropertyChangedCallback(OnVisibilityFocusPropertyChanged)));

        public static bool GetOnVisibilityFocus(DependencyObject obj)
        {
            return (bool)obj.GetValue(OnVisibilityFocusProperty);
        }

        public static void SetOnVisibilityFocus(DependencyObject obj, bool value)
        {
            obj.SetValue(OnVisibilityFocusProperty, value);
        }

        private static void OnVisibilityFocusPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (FrameworkElement)obj;
            if ((bool)args.NewValue)
            {
                control.Loaded += Control_Loaded;
                control.IsVisibleChanged += Control_IsVisibleChanged;
            }
            else
            {
                control.Loaded -= Control_Loaded;
                control.IsVisibleChanged -= Control_IsVisibleChanged;
            }
        }

        private static void Control_Loaded(object sender, RoutedEventArgs e)
        {
            Control_IsVisibleChanged(sender, new DependencyPropertyChangedEventArgs());
        }

        private static IInputElement lastFocus;

        private static void Control_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (FrameworkElement)sender;

            if (control.IsVisible)
            {
                lastFocus = Keyboard.FocusedElement;
                if (control.Focusable && control.IsVisible)
                {
                    control.Focus();
                }
                else
                {
                    foreach (var child in ElementTreeHelper.FindVisualChildren<FrameworkElement>(control))
                    {
                        if (child.Focusable && child.IsVisible)
                        {
                            child.Focus();
                            return;
                        }
                    }
                }
            }
            else
            {
                if (lastFocus != null)
                {
                    lastFocus.Focus();
                }
            }
        }
    }
}

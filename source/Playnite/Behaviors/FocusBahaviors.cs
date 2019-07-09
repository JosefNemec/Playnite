using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public static readonly DependencyProperty FocusBindingProperty =
            DependencyProperty.RegisterAttached(
                "FocusBinding",
                typeof(bool),
                typeof(FocusBahaviors),
                new PropertyMetadata(new PropertyChangedCallback(FocusBindingPropertyChanged)));

        public static bool GetFocusBinding(DependencyObject obj)
        {
            return (bool)obj.GetValue(FocusBindingProperty);
        }

        public static void SetFocusBinding(DependencyObject obj, bool value)
        {
            obj.SetValue(FocusBindingProperty, value);
        }

        private static void FocusBindingPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (DesignerProperties.GetIsInDesignMode(obj))
            {
                return;
            }

            var control = (FrameworkElement)obj;
            if ((bool)args.NewValue)
            {
                if (control.Focusable)
                {
                    control.Focus();                    
                }
                else
                {
                    if (!control.IsLoaded)
                    {
                        control.Loaded += Control_Loaded;
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
            }
        }

        private static void Control_Loaded(object sender, RoutedEventArgs e)
        {
            var elem = (FrameworkElement)sender;
            elem.Loaded -= Control_Loaded;
            foreach (var child in ElementTreeHelper.FindVisualChildren<FrameworkElement>(elem))
            {
                if (child.Focusable)
                {
                    child.Focus();
                    return;
                }
            }
        }

        private static readonly DependencyProperty OnVisibilityFocusProperty =
            DependencyProperty.RegisterAttached(
            "OnVisibilityFocus",
            typeof(bool),
            typeof(FocusBahaviors),
            new PropertyMetadata(new PropertyChangedCallback(OnVisibilityFocusPropertyChanged)));

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
            if (DesignerProperties.GetIsInDesignMode(obj))
            {
                return;
            }

            var control = (FrameworkElement)obj;
            if ((bool)args.NewValue)
            {
                control.IsVisibleChanged += Control_IsVisibleChanged;                
            }
            else
            {
                control.IsVisibleChanged -= Control_IsVisibleChanged;
            }
        }

        private static void Control_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var control = (FrameworkElement)sender;
            if (control.IsVisible)
            {
                if (control.Focusable)
                {
                    control.Focus();
                }
                else
                {
                    foreach (FrameworkElement child in LogicalTreeHelper.GetChildren(control))
                    {
                        if (child.Focusable && child.IsVisible)
                        {
                            child.Focus();
                            return;
                        }
                    }
                }
            }
        }

    }
}

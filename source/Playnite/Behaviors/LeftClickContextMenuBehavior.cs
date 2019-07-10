using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Playnite.Behaviors
{
    public class LeftClickContextMenuBehavior
    {
        private static readonly DependencyProperty LeftClickContextMenuProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(LeftClickContextMenuBehavior), new PropertyMetadata(new PropertyChangedCallback(HandlePropertyChanged)));

        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(LeftClickContextMenuProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(LeftClickContextMenuProperty, value);
        }

        private static void HandlePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (FrameworkElement)obj;
            if (DesignerProperties.GetIsInDesignMode(control))
            {
                return;
            }

            if ((bool)args.NewValue)
            {
                control.PreviewMouseLeftButtonUp += Control_PreviewMouseLeftButtonUp;
            }
            else
            {
                control.PreviewMouseLeftButtonUp -= Control_PreviewMouseLeftButtonUp;
            }
        }

        private static void Control_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var control = (FrameworkElement)sender;
            if (control.ContextMenu != null)
            {
                control.ContextMenu.PlacementTarget = control;
                control.ContextMenu.IsOpen = true;
            }
        }
    }
}

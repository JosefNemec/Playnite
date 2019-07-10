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

namespace Playnite.Behaviors
{
    public class ScrollToSelectedBehavior
    {
        private static readonly DependencyProperty ScrollToSelectedProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(ScrollToSelectedBehavior), new PropertyMetadata(new PropertyChangedCallback(HandlePropertyChanged)));

        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrollToSelectedProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrollToSelectedProperty, value);
        }

        private static void HandlePropertyChanged(
          DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (Selector)obj;
            if (DesignerProperties.GetIsInDesignMode(control))
            {
                return;
            }

            if ((bool)args.NewValue)
            {
                control.SelectionChanged += Control_SelectionChanged;
            }
            else
            {
                control.SelectionChanged -= Control_SelectionChanged;
            }
        }

        private static void Control_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as Selector).SelectedItem == null)
            {
                return;
            }

            if (sender is ListView listView)
            {
                if (listView.SelectedItems?.Count == 1)
                {                    
                    listView.ScrollIntoView(listView.SelectedItem);
                    return;
                }
            }

            if (sender is ListBox listBox)
            {
                if (listBox.SelectedItems?.Count == 1)
                {
                    listBox.ScrollIntoView(listBox.SelectedItem);
                    return;
                }
            }
        }
    }
}

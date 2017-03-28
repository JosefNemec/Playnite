using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PlayniteUI
{
    public static class Extensions
    {
        public static void ScrollIntoView(this ItemsControl control, object item)
        {
            var framework = control.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
            if (framework == null)
            {
                return;
            }

            framework.BringIntoView();
        }

        public static void ScrollIntoView(this ItemsControl control)
        {
            int count = control.Items.Count;
            if (count == 0)
            {
                return;
            }

            object item = control.Items[count - 1];
            control.ScrollIntoView(item);
        }
    }
}

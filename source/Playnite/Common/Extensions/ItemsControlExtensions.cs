using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace System.Windows.Controls
{
    public static class ItemsControlExtensions
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

        public static string ToHtml(this Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}

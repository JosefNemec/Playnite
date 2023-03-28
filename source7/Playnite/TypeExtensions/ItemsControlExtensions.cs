using System.Windows.Media;

namespace System.Windows.Controls;

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
}

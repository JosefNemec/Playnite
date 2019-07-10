using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Playnite
{
    public class ElementTreeHelper
    {
        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
            {
                yield return null;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child != null && child is T)
                {
                    yield return (T)child;
                }

                foreach (T childOfChild in FindVisualChildren<T>(child))
                {
                    yield return childOfChild;
                }
            }
        }

        public static IEnumerable<DependencyObject> FindLogicalChildren(DependencyObject depObj)
        {
            if (depObj == null)
            {
                yield return null;
            }

            foreach (var child in LogicalTreeHelper.GetChildren(depObj))
            {
                if (child is DependencyObject)
                {
                    yield return child as DependencyObject;

                    foreach (var childOfChild in FindLogicalChildren(child as DependencyObject))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}

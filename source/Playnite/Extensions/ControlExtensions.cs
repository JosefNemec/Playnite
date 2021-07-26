using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace System.Windows.Controls
{
    public static class ControlExtensions
    {
        public static T GetTemplateItem<T>(this Control control, string name)
        {
            var item = control.Template.FindName(name, control);
            return item is T t ? t : default;
        }
    }
}

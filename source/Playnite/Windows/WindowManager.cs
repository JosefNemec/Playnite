using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.Windows
{
    public class WindowManager
    {
        public static Window CurrentWindow
        {
            get
            {
                var window = PlayniteApplication.CurrentNative.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                return window ?? PlayniteApplication.CurrentNative.MainWindow;
            }
        }
    }
}

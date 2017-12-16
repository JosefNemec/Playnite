using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI
{
    public class PlayniteWindows
    {
        public static Window CurrentWindow
        {
            get
            {
                var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                return window ?? Application.Current.MainWindow;
            }
        }
    }
}

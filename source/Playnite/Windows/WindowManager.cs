using Playnite.Controls;
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
                Window window = null;
                for (int i = PlayniteApplication.CurrentNative.Windows.Count - 1; i >= 0; i--)
                {
                    window = PlayniteApplication.CurrentNative.Windows[i];
                    if (window.IsActive)
                    {
                        return window;
                    }
                }

                return window ?? PlayniteApplication.CurrentNative.MainWindow;
            }
        }

        public static bool GetHasChild(Window window)
        {
            return window.OwnedWindows.Count > 0;
        }

        public static void NotifyChildOwnershipChanges()
        {
            foreach (var wnd in PlayniteApplication.CurrentNative.Windows)
            {
                if (wnd is WindowBase window)
                {
                    window.OnPropertyChanged(nameof(WindowBase.HasChildWindow));
                }
            }
        }
    }
}

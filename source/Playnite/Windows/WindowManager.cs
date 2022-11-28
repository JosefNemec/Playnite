using Playnite.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.Windows
{
    public static class WindowManager
    {
        static WindowManager()
        {
            EventManager.RegisterClassHandler(typeof(WindowBase), WindowBase.ActivatedRoutedEvent, new RoutedEventHandler(ActivatedRoutedEventHandler));
            EventManager.RegisterClassHandler(typeof(WindowBase), WindowBase.ClosedRoutedEvent, new RoutedEventHandler(WindowBaseCloseHandler));
        }

        private static void ActivatedRoutedEventHandler(object sender, RoutedEventArgs e)
        {
            LastActiveWindow = (WindowBase)sender;
        }

        private static void WindowBaseCloseHandler(object sender, RoutedEventArgs e)
        {
            if (LastActiveWindow == (WindowBase)sender)
            {
                LastActiveWindow = null;
            }
        }

        public static WindowBase LastActiveWindow { get; private set; }

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

        public static bool GetHasChild(this Window window)
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

        public static void SetEnableMouseInput(bool enable)
        {
            foreach (var wnd in PlayniteApplication.CurrentNative.Windows)
            {
                if (wnd is WindowBase window)
                {
                    window.IsHitTestVisible = enable;
                }
            }
        }
    }
}

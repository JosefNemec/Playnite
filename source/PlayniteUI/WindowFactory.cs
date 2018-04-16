using PlayniteUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI
{
    public interface IWindowFactory
    {
        bool? CreateAndOpenDialog(object dataContext);

        void Show(object dataContext);

        void RestoreWindow();

        void BringToForeground();

        void Close();

        void Close(bool? resutl);
    }

    public abstract class WindowFactory : IWindowFactory
    {
        private readonly SynchronizationContext context;
        private bool asDialog = false;

        public WindowBase Window
        {
            get;
            private set;
        }

        public abstract WindowBase CreateNewWindowInstance();

        public WindowFactory()
        {
            context = SynchronizationContext.Current;            
        }

        public bool? CreateAndOpenDialog(object dataContext)
        {
            bool? result = null;
            context.Send((a) =>
            {
                Window = CreateNewWindowInstance();
                Window.DataContext = dataContext;
                if (Window != PlayniteWindows.CurrentWindow)
                {
                    Window.Owner = PlayniteWindows.CurrentWindow;
                }

                if (Window.Owner == null)
                {
                    Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    Window.ShowInTaskbar = true;
                }

                asDialog = true;
                result = Window.ShowDialog();
            }, null);

            return result;
        }

        public void Show(object dataContext)
        {
            context.Send((a) =>
            {
                asDialog = false;
                if (Window == null)
                {
                    Window = CreateNewWindowInstance();
                }

                Window.DataContext = dataContext;
                Window.Show();
            }, null);
        }

        public void BringToForeground()
        {
            context.Send((a) =>
            {
                WindowUtils.BringToForeground(Window);
            }, null);
        }

        public void RestoreWindow()
        {
            context.Send((a) =>
            {
                WindowUtils.RestoreWindow(Window);
            }, null);
        }

        public void Close()
        {
            Close(null);
        }

        public void Close(bool? result)
        {
            context.Send((a) =>
            {
                if (asDialog)
                {
                    Window.DialogResult = result;
                }
                Window.Close();
            }, null);
        }
    }

    public static class WindowUtils
    {
        public static void RestoreWindow(WindowBase window)
        {
            window.Show();
            if (window.WindowState == WindowState.Minimized)
            {
                window.WindowState = WindowState.Normal;
            }

            BringToForeground(window);
        }

        public static void BringToForeground(WindowBase window)
        {
            window.Topmost = true;
            window.Topmost = false;
        }
    }    
}

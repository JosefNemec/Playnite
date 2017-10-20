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
    }

    public abstract class WindowFactory : IWindowFactory
    {
        private readonly SynchronizationContext context;

        public WindowBase Window
        {
            get;
            private set;
        }

        public abstract WindowBase CreateNewWindowInstance();

        public WindowFactory()
        {
            context = SynchronizationContext.Current;
            Window = CreateNewWindowInstance();
        }

        public bool? CreateAndOpenDialog(object dataContext)
        {
            bool? result = null;
            context.Post((a) =>
            {
                Window = CreateNewWindowInstance();
                Window.DataContext = dataContext;
                Window.Owner = PlayniteWindows.CurrentWindow;
                result = Window.ShowDialog();
            }, null);

            return result;
        }

        public void Show(object dataContext)
        {
            context.Post((a) =>
            {
                Window.DataContext = dataContext;
                Window.Show();
            }, null);
        }

        public void BringToForeground()
        {
            context.Post((a) =>
            {
                WindowUtils.BringToForeground(Window);
            }, null);
        }

        public void RestoreWindow()
        {
            context.Post((a) =>
            {
                WindowUtils.RestoreWindow(Window);
            }, null);
        }
    }

    public static class WindowUtils
    {
        public static void RestoreWindow(WindowBase window)
        {
            window.Show();
            window.WindowState = WindowState.Normal;
            BringToForeground(window);
        }

        public static void BringToForeground(WindowBase window)
        {
            window.Topmost = true;
            window.Topmost = false;
        }
    }    
}

using Playnite.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.Windows
{
    public interface IWindowFactory
    {
        bool IsClosed { get; }

        bool? CreateAndOpenDialog(object dataContext);

        void Show(object dataContext);

        void RestoreWindow();

        void BringToForeground();

        void Close();

        void Close(bool? resutl);

        WindowBase Window { get; }
    }

    public abstract class WindowFactory : IWindowFactory
    {
        private readonly SynchronizationContext context;
        private bool asDialog = false;
        public bool IsClosed { get; private set; } = true;

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
                Window.Closed += Window_Closed;
                Window.DataContext = dataContext;
                if (Window != WindowManager.CurrentWindow)
                {
                    Window.Owner = WindowManager.CurrentWindow;
                }

                if (Window.Owner == null)
                {
                    Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    Window.ShowInTaskbar = true;
                }

                asDialog = true;
                WindowManager.NotifyChildOwnershipChanges();
                IsClosed = false;
                result = Window.ShowDialog();
            }, null);

            return result;
        }

        public void Show(object dataContext)
        {
            context.Send((a) =>
            {
                asDialog = false;
                if (IsClosed)
                {
                    Window = CreateNewWindowInstance();
                    Window.Closed += Window_Closed;
                }

                Window.DataContext = dataContext;
                IsClosed = false;
                Window.Show();
            }, null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            WindowManager.NotifyChildOwnershipChanges();
            IsClosed = true;
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
                WindowManager.NotifyChildOwnershipChanges();
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
            if (!window.Activate())
            {
                window.Topmost = true;
                window.Topmost = false;
            }
        }
    }    
}

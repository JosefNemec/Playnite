using Playnite.Common;
using Playnite.Controls;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Playnite.Windows
{
    public interface IWindowFactory
    {
        bool IsClosed { get; }

        bool? CreateAndOpenDialog(object dataContext);

        void Show(object dataContext);

        void RestoreWindow();

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
        private static ILogger logger = LogManager.GetLogger();

        public static void RestoreWindow(WindowBase window)
        {
            try
            {
                // This is the only reliable method that also doesn't result in issues like this:
                // https://www.reddit.com/r/playnite/comments/f6d73l/bug_full_screen_ui_wont_respond_to_left_stick/
                // Adapted from https://ask.xiaolee.net/questions/1040342
                window.Show();
                if (!window.Activate())
                {
                    window.Topmost = true;
                    window.Topmost = false;
                }

                if (window.WindowState == WindowState.Minimized)
                {
                    window.WindowState = WindowState.Normal;
                }

                //Get the process ID for this window's thread
                var interopHelper = new WindowInteropHelper(window);
                var thisWindowThreadId = Interop.GetWindowThreadProcessId(interopHelper.Handle, IntPtr.Zero);

                //Get the process ID for the foreground window's thread
                var currentForegroundWindow = Interop.GetForegroundWindow();
                var currentForegroundWindowThreadId = Interop.GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);

                //Attach this window's thread to the current window's thread
                Interop.AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);

                //Set the window position
                Interop.SetWindowPos(interopHelper.Handle, new IntPtr(0), 0, 0, 0, 0, Interop.SWP_NOSIZE | Interop.SWP_NOMOVE | Interop.SWP_SHOWWINDOW);

                //Detach this window's thread from the current window's thread
                Interop.AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to restore window.");
            }
        }
    }
}

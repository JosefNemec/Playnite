using Playnite.Common;
using Playnite.Controls;
using Playnite.Native;
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
        bool WasClosed { get; }

        bool? CreateAndOpenDialog(object dataContext);

        void Show(object dataContext);

        void RestoreWindow();

        void Close();

        void Close(bool? resutl);

        WindowBase Window { get; }
    }

    public abstract class WindowFactory : IWindowFactory
    {
        private static ILogger logger = LogManager.GetLogger();
        private readonly SynchronizationContext context;
        private bool asDialog = false;
        private AutoResetEvent initFinishedEvent { get; } = new AutoResetEvent(false);
        public bool WasClosed { get; private set; } = false;

        public WindowBase Window
        {
            get;
            private set;
        }

        public abstract WindowBase CreateNewWindowInstance();

        public WindowFactory()
        {
            context = SynchronizationContext.Current ?? PlayniteApplication.Current.SyncContext;
        }

        public bool? CreateAndOpenDialog(object dataContext)
        {
            bool? result = null;
            context.Send((a) =>
            {
                Window = CreateNewWindowInstance();
                Window.Closed += Window_Closed;
                Window.DataContext = dataContext;
                logger.Debug($"Show dialog window {GetType()}: {Window.Id}");

                var currentWindow = WindowManager.CurrentWindow;
                if (currentWindow != null && Window != currentWindow)
                {
                    if (typeof(WindowBase).IsAssignableFrom(currentWindow.GetType()) && ((WindowBase)currentWindow).IsShown)
                    {
                        Window.Owner = currentWindow;
                    }
                }

                if (Window.Owner == null)
                {
                    Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    Window.ShowInTaskbar = true;
                }

                asDialog = true;
                WasClosed = false;
                initFinishedEvent.Set();
                result = Window.ShowDialog();
            }, null);

            return result;
        }

        public void Show(object dataContext)
        {
            context.Send((a) =>
            {
                asDialog = false;
                if (WasClosed)
                {
                    logger.Debug($"Opening window that was closed previously {GetType()}, old Id: {Window.Id}");
                    Window = CreateNewWindowInstance();
                    Window.Closed += Window_Closed;
                }

                if (Window == null)
                {
                    Window = CreateNewWindowInstance();
                    Window.Closed += Window_Closed;
                }

                logger.Debug($"Show window {GetType()}: {Window.Id}");
                Window.DataContext = dataContext;
                WasClosed = false;
                initFinishedEvent.Set();
                Window.Show();
            }, null);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            WasClosed = true;
            Window.Closed -= Window_Closed;
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
            // This needs to be here in case Close is called too early.
            // This can happen in async scenarios like with ProgressViewViewModel and progress dialogs if progress code is too fast to complete.
            initFinishedEvent.WaitOne();

            logger.Debug($"Closing window {GetType()}: {Window.Id}, {result}");
            context.Send(async (_) =>
            {
                // This is a workaround for WPF bug which causes deadlock in ShowDialog
                // if parent of modal window is closed before the child window itself is closed.
                // To prevent this we need to make sure that window parenting other windows is only
                // closed after all children are closed.
                // https://github.com/dotnet/wpf/issues/277
                // https://stackoverflow.com/questions/40304161/showdialog-method-hangs-without-showing-the-window-deadlock#48208699
                while (Window.GetHasChild())
                {
                    await Task.Delay(100);
                }

                if (asDialog)
                {
                    try
                    {
                        // TODO: possible race condition when Closing a window from ProgressViewViewModel
                        // which can in theory close window (and set dialog result) before a window is actually opened.
                        Window.DialogResult = result;
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, $"DialogResult fail {GetType()}, {result}");
                        throw;
                    }
                }

                Window.Close();
            }, null);
        }
    }

    public static class WindowUtils
    {
        private static ILogger logger = LogManager.GetLogger();

        public static void RestoreWindow(this Window window)
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
                var thisWindowThreadId = User32.GetWindowThreadProcessId(interopHelper.Handle, IntPtr.Zero);

                //Get the process ID for the foreground window's thread
                var currentForegroundWindow = User32.GetForegroundWindow();
                var currentForegroundWindowThreadId = User32.GetWindowThreadProcessId(currentForegroundWindow, IntPtr.Zero);

                //Attach this window's thread to the current window's thread
                User32.AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, true);

                //Set the window position
                User32.SetWindowPos(interopHelper.Handle, new IntPtr(0), 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.SHOWWINDOW);

                //Detach this window's thread from the current window's thread
                User32.AttachThreadInput(currentForegroundWindowThreadId, thisWindowThreadId, false);
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to restore window.");
            }
        }
    }
}

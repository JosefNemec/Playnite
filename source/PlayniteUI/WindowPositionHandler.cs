using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Playnite;

namespace PlayniteUI
{
    public class WindowPositionHandler
    {
        private Window Window;
        private string WindowName;
        private bool IgnoreChanges = false;
        private Settings Configuration;

        public WindowPositionHandler(Window window, string windowName, Settings settings)
        {
            Window = window;
            WindowName = windowName;
            Configuration = settings;
            window.SizeChanged += Window_SizeChanged;
            window.LocationChanged += Window_LocationChanged;
            window.StateChanged += Window_StateChanged;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (Window.IsLoaded)
            {
                SaveState();
            }
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            if (Window.IsLoaded)
            {
                SavePosition();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Window.IsLoaded)
            {
                SaveSize();
            }
        }

        private void MakeSureConfigEntryExists()
        {
            if (!Configuration.WindowPositions.ContainsKey(WindowName))
            {
                Configuration.WindowPositions[WindowName] = new Settings.WindowPosition();
            }
        }

        public void SaveState()
        {
            if (Configuration == null || IgnoreChanges)
            {
                return;
            }

            // Don't save minimized state. It would not be very user friendly if user exit Playnite while minimized
            // and it would then open minimized on next startup.
            if (Window.WindowState == WindowState.Minimized)
            {
                return;
            }

            MakeSureConfigEntryExists();
            Configuration.WindowPositions[WindowName].State = Window.WindowState;
        }

        public void SaveSize()
        {
            if (Configuration == null || IgnoreChanges)
            {
                return;
            }

            // Don't save size if windows is maximized, it would be too large when it would restore back to normal state.
            // Don't save size if windows is minimized becuase it has no size :)
            if (Window.WindowState != WindowState.Normal)
            {
                return;
            }

            MakeSureConfigEntryExists();
            Configuration.WindowPositions[WindowName].Size = new Settings.WindowPosition.Point()
            {
                X = Window.Width,
                Y = Window.Height
            };
        }

        public void SavePosition()
        {
            if (Configuration == null || IgnoreChanges)
            {
                return;
            }

            if (Window.Left < 0 || Window.Top < 0)
            {
                return;
            }

            MakeSureConfigEntryExists();
            Configuration.WindowPositions[WindowName].Position = new Settings.WindowPosition.Point()
            {
                X = Window.Left,
                Y = Window.Top
            };
        }

        public void RestoreSizeAndLocation()
        {
            if (!Configuration.WindowPositions.ContainsKey(WindowName))
            {
                return;
            }

            IgnoreChanges = true;

            try
            {
                var data = Configuration.WindowPositions[WindowName];

                if (data.Position != null)
                {
                    Window.Left = data.Position.X;
                    Window.Top = data.Position.Y;
                }

                if (data.Size != null)
                {
                    Window.Width = data.Size.X;
                    Window.Height = data.Size.Y;
                }

                Window.WindowState = data.State;
            }
            finally
            {
                IgnoreChanges = false;
            }
        }
    }
}

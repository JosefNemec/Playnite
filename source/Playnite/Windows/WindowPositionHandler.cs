using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using Playnite;
using Playnite.Settings;

namespace Playnite.Windows
{
    public class WindowPositionHandler
    {
        private Window Window;
        private string WindowName;
        private bool IgnoreChanges = false;
        private WindowPositions Configuration;

        public WindowPositionHandler(Window window, string windowName, WindowPositions settings)
        {
            Window = window;
            WindowName = windowName;
            Configuration = settings;
            window.SizeChanged += Window_SizeChanged;
            window.LocationChanged += Window_LocationChanged;
            window.StateChanged += Window_StateChanged;
            window.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Check window bounds only if this window has no configuration yet, ie. first run.
            if (!Configuration.Positions.ContainsKey(WindowName))
            {
                CheckWindowBounds();
            }
            Window.Loaded -= Window_Loaded;
        }

        private void CheckWindowBounds()
        {
            Screen screenBounds = Screen.FromRectangle(new Rectangle(
                (int) Window.Left, (int) Window.Top,
                (int) Window.Width, (int) Window.Height));
            
            if (Window.Height > screenBounds.WorkingArea.Height)
            {
                Window.Height = screenBounds.WorkingArea.Height;
            }
            if (Window.Width > screenBounds.WorkingArea.Width)
            {
                Window.Width = screenBounds.WorkingArea.Width;
            }
            if (Window.Top < screenBounds.WorkingArea.Top)
            {
                Window.Top = screenBounds.WorkingArea.Top;
            }
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
            if (!Configuration.Positions.ContainsKey(WindowName))
            {
                Configuration.Positions[WindowName] = new WindowPosition();
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
            Configuration.Positions[WindowName].State = Window.WindowState;
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
            Configuration.Positions[WindowName].Size = new WindowPosition.Point()
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
            Configuration.Positions[WindowName].Position = new WindowPosition.Point()
            {
                X = Window.Left,
                Y = Window.Top
            };
        }

        public void RestoreSizeAndLocation()
        {
            if (!Configuration.Positions.ContainsKey(WindowName))
            {
                return;
            }

            IgnoreChanges = true;

            try
            {
                var data = Configuration.Positions[WindowName];

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

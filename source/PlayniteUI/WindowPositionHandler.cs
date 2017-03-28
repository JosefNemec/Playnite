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
        private Window window;
        private string windowName;
        private bool ignoreChanges = false;

        public WindowPositionHandler(Window window, string windowName)
        {
            this.window = window;
            this.windowName = windowName;
        }

        public void SaveSize(Settings config)
        {
            if (config == null || ignoreChanges)
            {
                return;
            }

            if (!config.WindowPositions.ContainsKey(windowName))
            {
                config.WindowPositions[windowName] = new Settings.WindowPosition();
            }

            config.WindowPositions[windowName].Size = new Settings.WindowPosition.Point()
            {
                X = window.Width,
                Y = window.Height
            };
        }

        public void SavePosition(Settings config)
        {
            if (config == null || ignoreChanges)
            {
                return;
            }

            if (!config.WindowPositions.ContainsKey(windowName))
            {
                config.WindowPositions[windowName] = new Settings.WindowPosition();
            }

            config.WindowPositions[windowName].Position = new Settings.WindowPosition.Point()
            {
                X = window.Left,
                Y = window.Top
            };
        }

        public void RestoreSizeAndLocation(Settings config)
        {
            if (!config.WindowPositions.ContainsKey(windowName))
            {
                return;
            }

            ignoreChanges = true;

            try
            {
                var data = config.WindowPositions[windowName];

                if (data.Position != null)
                {
                    window.Left = data.Position.X;
                    window.Top = data.Position.Y;
                }

                if (data.Size != null)
                {
                    window.Width = data.Size.X;
                    window.Height = data.Size.Y;
                }
            }
            finally
            {
                ignoreChanges = false;
            }
        }
    }
}

using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace System.Windows
{
    public static class WindowExtensions
    {
        // TODO check DPI
        public static ComputerScreen GetScreen(this Window window)
        {
            var screens = Computer.GetAllScreensV2();
            return screens.FirstOrDefault(a => a.Bounds.Contains(new System.Drawing.Point((int)window.Left, (int)window.Top)));
        }
    }
}

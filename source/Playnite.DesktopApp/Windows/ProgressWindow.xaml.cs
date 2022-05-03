using Playnite.Controls;
using Playnite.Native;
using System;
using System.Windows.Interop;

namespace Playnite.DesktopApp.Windows
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class ProgressWindow : WindowBase
    {
        public ProgressWindow() : base()
        {
            Loaded += ProgressWindow_Loaded;
            InitializeComponent();
        }

        private void ProgressWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            // Disable window title bar menu to prevent user from force closing the dialog
            var hwnd = new WindowInteropHelper(this).Handle;
            User32.SetWindowLong(hwnd, Winuser.GWL_STYLE, User32.GetWindowLong(hwnd, Winuser.GWL_STYLE) & ~Winuser.WS_SYSMENU);
        }
    }
}

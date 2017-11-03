using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class FullscreenWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new FullscreenWindow();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class FullscreenWindow : WindowBase
    {
        public FullscreenWindow()
        {
            InitializeComponent();
        }

        private void WindowFullscreen_Activated(object sender, System.EventArgs e)
        {
            Topmost = true;
        }

        private void WindowFullscreen_Deactivated(object sender, System.EventArgs e)
        {
            Topmost = false;
        }
    }
}

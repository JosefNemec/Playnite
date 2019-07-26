using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class AboutWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new AboutWindow();
        }
    }

    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : WindowBase
    {
        public AboutWindow() : base()
        {
            InitializeComponent();
        }
    }
}

using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class EmulatorDownloadWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new EmulatorDownloadWindow();
        }
    }

    /// <summary>
    /// Interaction logic for EmulatorImportWindow.xaml
    /// </summary>
    public partial class EmulatorDownloadWindow : WindowBase
    {
        public EmulatorDownloadWindow() : base()
        {
            InitializeComponent();
        }
    }
}

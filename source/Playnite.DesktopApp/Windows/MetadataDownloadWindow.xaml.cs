using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class MetadataDownloadWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new MetadataDownloadWindow();
        }
    }

    /// <summary>
    /// Interaction logic for EmulatorImportWindow.xaml
    /// </summary>
    public partial class MetadataDownloadWindow : WindowBase
    {
        public MetadataDownloadWindow() : base()
        {
            InitializeComponent();
        }
    }
}

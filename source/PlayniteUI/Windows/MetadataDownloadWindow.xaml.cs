using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class MetadataDownloadWindowFactory : WindowFactory
    {
        public static MetadataDownloadWindowFactory Instance
        {
            get => new MetadataDownloadWindowFactory();
        }

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
        public MetadataDownloadWindow()
        {
            InitializeComponent();
        }
    }
}

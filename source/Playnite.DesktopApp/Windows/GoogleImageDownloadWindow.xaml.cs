using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class GoogleImageDownloadWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new GoogleImageDownloadWindow();
        }
    }

    /// <summary>
    /// Interaction logic for MetadataLookupWindow.xaml
    /// </summary>
    public partial class GoogleImageDownloadWindow : WindowBase
    {
        private WindowPositionHandler positionManager;

        public GoogleImageDownloadWindow() : base()
        {
            InitializeComponent();
            if (PlayniteApplication.Current.AppSettings != null)
            {
                positionManager = new WindowPositionHandler(this, "GoogleImageDownload", PlayniteApplication.Current.AppSettings.WindowPositions);
            }
        }
    }
}

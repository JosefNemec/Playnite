using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class MetadataComparisonWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new MetadataComparisonWindow();
        }
    }

    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class MetadataComparisonWindow : WindowBase
    {
        private WindowPositionHandler positionManager;

        public MetadataComparisonWindow() : base()
        {
            InitializeComponent();
            if (PlayniteApplication.Current.AppSettings != null)
            {
                positionManager = new WindowPositionHandler(this, " MetadataComparison", PlayniteApplication.Current.AppSettings.WindowPositions);
            }
        }
    }
}

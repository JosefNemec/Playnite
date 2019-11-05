using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class MainWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new MainWindow();
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase
    {
        private WindowPositionHandler positionManager;

        public MainWindow() : base()
        {
            InitializeComponent();
            if (PlayniteApplication.Current.AppSettings != null)
            { 
                positionManager = new WindowPositionHandler(this, "Main", PlayniteApplication.Current.AppSettings.WindowPositions);
            }
        }
    }
}

using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class GameEditWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new GameEditWindow();
        }
    }

    /// <summary>
    /// Interaction logic for GameEditWindow.xaml
    /// </summary>
    public partial class GameEditWindow : WindowBase
    {
        private WindowPositionHandler positionManager;

        public GameEditWindow() : base()
        {
            InitializeComponent();
            positionManager = new WindowPositionHandler(this, "EditGame", PlayniteApplication.Current.AppSettings.WindowPositions);
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            positionManager.RestoreSizeAndLocation();
        }
    }
}

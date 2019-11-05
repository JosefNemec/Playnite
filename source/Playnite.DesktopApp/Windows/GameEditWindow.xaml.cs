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
            if (PlayniteApplication.Current.AppSettings != null)
            {
                positionManager = new WindowPositionHandler(this, "EditGame", PlayniteApplication.Current.AppSettings.WindowPositions);
            }
        }
    }
}

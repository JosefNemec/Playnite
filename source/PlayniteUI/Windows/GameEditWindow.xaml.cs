using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class GameEditWindowFactory : WindowFactory
    {
        public static GameEditWindowFactory Instance
        {
            get => new GameEditWindowFactory();
        }

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

        public GameEditWindow()
        {
            InitializeComponent();
            positionManager = new WindowPositionHandler(this, "EditGame", App.AppSettings);
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            positionManager.RestoreSizeAndLocation();
        }
    }
}

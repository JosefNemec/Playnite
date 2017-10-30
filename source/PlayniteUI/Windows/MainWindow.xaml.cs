using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class MainWindowFactory : WindowFactory
    {
        public static MainWindowFactory Instance
        {
            get => new MainWindowFactory();
        }

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

        public MainWindow()
        {
            InitializeComponent();
            if (App.AppSettings != null)
            { 
                positionManager = new WindowPositionHandler(this, "Main", App.AppSettings);
            }
        }

        private void Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            positionManager?.RestoreSizeAndLocation();
        }
    }
}

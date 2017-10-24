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
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}

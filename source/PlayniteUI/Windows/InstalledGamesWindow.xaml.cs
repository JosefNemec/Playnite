using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class InstalledGamesWindowFactory : WindowFactory
    {
        public static InstalledGamesWindowFactory Instance
        {
            get => new InstalledGamesWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new InstalledGamesWindow();
        }
    }

    /// <summary>
    /// Interaction logic for InstalledGamesWindow.xaml
    /// </summary>
    public partial class InstalledGamesWindow : WindowBase
    {
        public InstalledGamesWindow()
        {
            InitializeComponent();
        }
    }
}

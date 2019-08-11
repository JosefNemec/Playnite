using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class InstalledGamesWindowFactory : WindowFactory
    {
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
        public InstalledGamesWindow() : base()
        {
            InitializeComponent();
        }
    }
}

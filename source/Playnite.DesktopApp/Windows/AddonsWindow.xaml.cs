using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class AddonsWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new AddonsWindow();
        }
    }

    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AddonsWindow : WindowBase
    {
        private WindowPositionHandler positionManager;

        public AddonsWindow() : base()
        {
            InitializeComponent();
            if (PlayniteApplication.Current.AppSettings != null)
            {
                positionManager = new WindowPositionHandler(this, "AddonsWindow_2", PlayniteApplication.Current.AppSettings.WindowPositions);
            }
        }
    }
}

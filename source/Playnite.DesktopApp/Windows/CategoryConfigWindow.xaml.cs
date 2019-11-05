using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class CategoryConfigWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new CategoryConfigWindow();
        }
    }

    /// <summary>
    /// Interaction logic for CategoryConfigWindow.xaml
    /// </summary>
    public partial class CategoryConfigWindow : WindowBase
    {
        private WindowPositionHandler positionManager;

        public CategoryConfigWindow() : base()
        {
            InitializeComponent();
            if (PlayniteApplication.Current.AppSettings != null)
            {
                positionManager = new WindowPositionHandler(this, "CategoryConfig", PlayniteApplication.Current.AppSettings.WindowPositions);
            }
        }
    }
}

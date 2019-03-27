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
        public CategoryConfigWindow()
        {
            InitializeComponent();
        }
    }
}

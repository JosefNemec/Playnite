using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class DatabaseFieldsManagerWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new DatabaseFieldsManagerWindow();
        }
    }

    public partial class DatabaseFieldsManagerWindow : WindowBase
    {
        public DatabaseFieldsManagerWindow() : base()
        {
            InitializeComponent();
        }
    }
}

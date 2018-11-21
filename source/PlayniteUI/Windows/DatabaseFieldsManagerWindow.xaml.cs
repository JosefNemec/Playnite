using PlayniteUI.Controls;

namespace PlayniteUI.Windows
{
    public class DatabaseFieldsManagerWindowFactory : WindowFactory
    {
        public static DatabaseFieldsManagerWindowFactory Instance
        {
            get => new DatabaseFieldsManagerWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new DatabaseFieldsManagerWindow();
        }
    }

    public partial class DatabaseFieldsManagerWindow : WindowBase
    {
        public DatabaseFieldsManagerWindow()
        {
            InitializeComponent();
        }
    }
}

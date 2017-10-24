using PlayniteUI.Controls;

namespace PlayniteUI.Windows
{
    public class CategoryConfigWindowFactory : WindowFactory
    {
        public static CategoryConfigWindowFactory Instance
        {
            get => new CategoryConfigWindowFactory();
        }

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

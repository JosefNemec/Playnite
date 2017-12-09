using PlayniteUI.Controls;

namespace PlayniteUI.Windows
{
    public class FirstTimeStartupWindowFactory : WindowFactory
    {
        public static FirstTimeStartupWindowFactory Instance
        {
            get => new FirstTimeStartupWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new FirstTimeStartupWindow();
        }
    }

    /// <summary>
    /// Interaction logic for FirstTimeStartupWindow.xaml
    /// </summary>
    public partial class FirstTimeStartupWindow : WindowBase
    {        
        public FirstTimeStartupWindow()
        {
            InitializeComponent();
        }
    }
}

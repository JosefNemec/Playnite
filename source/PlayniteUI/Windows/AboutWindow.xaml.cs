using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class AboutWindowFactory : WindowFactory
    {
        public static AboutWindowFactory Instance
        {
            get => new AboutWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new AboutWindow();
        }
    }

    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : WindowBase
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
    }
}

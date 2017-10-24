using PlayniteUI.Controls;

namespace PlayniteUI.Windows
{
    public class CrashHandlerWindowFactory : WindowFactory
    {
        public static CrashHandlerWindowFactory Instance
        {
            get => new CrashHandlerWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new CrashHandlerWindow();
        }
    }

    /// <summary>
    /// Interaction logic for CrashHandlerWindow.xaml
    /// </summary>
    public partial class CrashHandlerWindow : WindowBase
    {
        public CrashHandlerWindow()
        {
            InitializeComponent();
        }
    }
}

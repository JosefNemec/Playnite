using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class SettingsWindowFactory : WindowFactory
    {
        public static SettingsWindowFactory Instance
        {
            get => new SettingsWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new SettingsWindow();
        }
    }

    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class SettingsWindow : WindowBase
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }
    }
}

using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class PlatformsWindowFactory : WindowFactory
    {
        public static PlatformsWindowFactory Instance
        {
            get => new PlatformsWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new PlatformsWindow();
        }
    }

    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class PlatformsWindow : WindowBase
    {
        public PlatformsWindow()
        {
            InitializeComponent();
        }
    }
}
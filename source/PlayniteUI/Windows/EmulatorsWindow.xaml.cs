using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class EmulatorsWindowFactory : WindowFactory
    {
        public static EmulatorsWindowFactory Instance
        {
            get => new EmulatorsWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new EmulatorsWindow();
        }
    }

    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class EmulatorsWindow : WindowBase
    {
        public EmulatorsWindow()
        {
            InitializeComponent();
        }
    }
}
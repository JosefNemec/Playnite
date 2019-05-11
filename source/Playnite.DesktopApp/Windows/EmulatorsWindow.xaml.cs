using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class EmulatorsWindowFactory : WindowFactory
    {
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
        public EmulatorsWindow() : base()
        {
            InitializeComponent();
        }
    }
}
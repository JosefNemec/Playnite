using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class EmulatorImportWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new EmulatorImportWindow();
        }
    }

    /// <summary>
    /// Interaction logic for EmulatorImportWindow.xaml
    /// </summary>
    public partial class EmulatorImportWindow : WindowBase
    {
        public EmulatorImportWindow() : base()
        {
            InitializeComponent();
        }
    }
}

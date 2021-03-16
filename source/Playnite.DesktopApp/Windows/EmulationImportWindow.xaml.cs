using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class EmulationImportWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new EmulationImportWindow();
        }
    }

    /// <summary>
    /// Interaction logic for EmulatorImportWindow.xaml
    /// </summary>
    public partial class EmulationImportWindow : WindowBase
    {
        public EmulationImportWindow() : base()
        {
            InitializeComponent();
        }
    }
}

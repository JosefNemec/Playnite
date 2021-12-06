using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class EmulatedGameImportWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new EmulatedGameImportWindow();
        }
    }

    /// <summary>
    /// Interaction logic for EmulatorImportWindow.xaml
    /// </summary>
    public partial class EmulatedGameImportWindow : WindowBase
    {
        public EmulatedGameImportWindow() : base("EmulatedGameImportWindow_V2")
        {
            InitializeComponent();
        }
    }
}

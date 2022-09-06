using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class ActionSelectionWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new ActionSelectionWindow();
        }
    }

    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class ActionSelectionWindow : WindowBase
    {
        public ActionSelectionWindow() : base()
        {
            InitializeComponent();
        }
    }
}

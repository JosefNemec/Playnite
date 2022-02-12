using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class SearchWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new SearchWindow();
        }
    }

    /// <summary>
    /// Interaction logic for SearchWindow.xaml
    /// </summary>
    public partial class SearchWindow : WindowBase
    {
        public SearchWindow() : base()
        {
            // TODO safe position
            InitializeComponent();
        }
    }
}

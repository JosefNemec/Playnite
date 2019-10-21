using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class ItemSelectionWithSearchWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new ItemSelectionWithSearchWindow();
        }
    }

    /// <summary>
    /// Interaction logic for MetadataLookupWindow.xaml
    /// </summary>
    public partial class ItemSelectionWithSearchWindow : WindowBase
    {
        public ItemSelectionWithSearchWindow() : base()
        {
            InitializeComponent();
        }
    }
}

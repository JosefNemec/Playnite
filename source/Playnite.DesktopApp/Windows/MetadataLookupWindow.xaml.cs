using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class MetadataLookupWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new MetadataLookupWindow();
        }
    }

    /// <summary>
    /// Interaction logic for MetadataLookupWindow.xaml
    /// </summary>
    public partial class MetadataLookupWindow : WindowBase
    {
        public MetadataLookupWindow()
        {
            InitializeComponent();
        }
    }
}

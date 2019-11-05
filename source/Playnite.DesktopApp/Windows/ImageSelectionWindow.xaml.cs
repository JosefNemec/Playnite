using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class ImageSelectionWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new ImageSelectionWindow();
        }
    }

    /// <summary>
    /// Interaction logic for MetadataLookupWindow.xaml
    /// </summary>
    public partial class ImageSelectionWindow : WindowBase
    {
        public ImageSelectionWindow() : base()
        {
            InitializeComponent();
        }
    }
}

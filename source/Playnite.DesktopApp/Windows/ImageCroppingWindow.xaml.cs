using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class ImageCroppingWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new ImageCroppingWindow();
        }
    }

    /// <summary>
    /// Interaction logic for ImageCroppingWindow.xaml
    /// </summary>
    public partial class ImageCroppingWindow : WindowBase
    {
        public ImageCroppingWindow() : base()
        {
            InitializeComponent();
        }
    }
}

using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class MetadataLookupWindowFactory : WindowFactory
    {
        public static MetadataLookupWindowFactory Instance
        {
            get => new MetadataLookupWindowFactory();
        }

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

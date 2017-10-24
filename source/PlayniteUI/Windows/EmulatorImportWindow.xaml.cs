using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class EmulatorImportWindowFactory : WindowFactory
    {
        public static EmulatorImportWindowFactory Instance
        {
            get => new EmulatorImportWindowFactory();
        }

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
        public EmulatorImportWindow()
        {
            InitializeComponent();
        }
    }
}

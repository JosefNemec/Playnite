using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class UpdateWindowFactory : WindowFactory
    {
        public static UpdateWindowFactory Instance
        {
            get => new UpdateWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new UpdateWindow();
        }
    }

    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : WindowBase
    {
        public UpdateWindow()
        {
            InitializeComponent();
        }
    }
}

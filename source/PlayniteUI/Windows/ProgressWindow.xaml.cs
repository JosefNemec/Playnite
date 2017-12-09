using PlayniteUI.Controls;
using System.Windows;

namespace PlayniteUI
{
    public class ProgressWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new ProgressWindow();
        }
    }

    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class ProgressWindow : WindowBase
    {
        public ProgressWindow()
        {
            InitializeComponent();
        }
    }
}

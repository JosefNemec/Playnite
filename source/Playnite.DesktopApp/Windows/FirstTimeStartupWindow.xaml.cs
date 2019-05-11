using Playnite.Controls;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    public class FirstTimeStartupWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new FirstTimeStartupWindow();
        }
    }

    /// <summary>
    /// Interaction logic for FirstTimeStartupWindow.xaml
    /// </summary>
    public partial class FirstTimeStartupWindow : WindowBase
    {        
        public FirstTimeStartupWindow() : base()
        {
            InitializeComponent();
        }

        private void ButtonFinish_IsVisibleChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (ButtonFinish.IsVisible)
            {
                ButtonFinish.Focus();
            }
        }

        private void ButtonBack_IsEnabledChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            if (!ButtonBack.IsEnabled)
            {
                ButtonNext.Focus();
            }
        }
    }
}

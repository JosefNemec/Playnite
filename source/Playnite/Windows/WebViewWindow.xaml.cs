using Playnite.Controls;
using System.Windows;

namespace Playnite.Windows
{
    /// <summary>
    /// Interaction logic for WebViewWindow.xaml
    /// </summary>
    public partial class WebViewWindow :  WindowBase
    {
        public WebViewWindow() : base()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Browser.Focus();
        }
    }
}

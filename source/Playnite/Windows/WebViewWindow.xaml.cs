using System.Windows;

namespace Playnite.Windows
{
    /// <summary>
    /// Interaction logic for WebViewWindow.xaml
    /// </summary>
    public partial class WebViewWindow : Window
    {
        public WebViewWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Browser.Focus();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PlayniteUI.Windows
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

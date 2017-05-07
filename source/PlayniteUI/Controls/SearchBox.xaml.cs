using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlayniteUI.Controls
{
    /// <summary>
    /// Interaction logic for SearchBox.xaml
    /// </summary>
    public partial class SearchBox : UserControl
    {
        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }

            set
            {
                if (Text != value)
                {
                    SetValue(TextProperty, value);
                }
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SearchBox));

        public SearchBox()
        {
            InitializeComponent();
        }

        private void ClearImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Text = string.Empty;
        }

        private void TextFilter_TextChanged(object sender, TextChangedEventArgs e)
        {

            if (!string.IsNullOrEmpty(TextFilter.Text))
            {
                ImageClear.Visibility = Visibility.Visible;
            }
            else
            {
                ImageClear.Visibility = Visibility.Hidden;
            }
        }
    }
}

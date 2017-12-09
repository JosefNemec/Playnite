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
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(SearchBox), new PropertyMetadata(string.Empty, TextPropertyChangedCallback));

        public bool ShowImage
        {
            get
            {
                return (bool)GetValue(ShowImageProperty);
            }

            set
            {
                SetValue(ShowImageProperty, value);
            }
        }
        
        public static readonly DependencyProperty ShowImageProperty = DependencyProperty.Register("ShowImage", typeof(bool), typeof(SearchBox), new PropertyMetadata(true, ShowImagePropertyChangedCallback));

        public SearchBox()
        {
            InitializeComponent();
        }

        private void ClearImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TextFilter.Text = string.Empty;
        }
        
        private static void TextPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as SearchBox;
            obj.TextFilter.Text = (string)e.NewValue;
            obj.TextFilter.CaretIndex = obj.TextFilter.Text.Length;
        }

        private static void ShowImagePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as SearchBox;
            obj.ShowImage = (bool)e.NewValue;
        }
    }
}

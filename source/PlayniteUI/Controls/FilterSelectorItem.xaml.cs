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
using Playnite;

namespace PlayniteUI.Controls
{
    /// <summary>
    /// Interaction logic for FilterSelectorItem.xaml
    /// </summary>
    public partial class FilterSelectorItem : UserControl
    {
        public string CountText
        {
            get
            {
                return (string)GetValue(CountTextProperty);
            }

            set
            {
                SetValue(CountTextProperty, value);
            }
        }

        public static readonly DependencyProperty CountTextProperty = DependencyProperty.Register(
            "CountText",
            typeof(string),
            typeof(FilterSelectorItem),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

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

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text",
            typeof(string),
            typeof(FilterSelectorItem),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public bool IsChecked
        {
            get
            {
                return (bool)GetValue(IsCheckedProperty);
            }

            set
            {
                SetValue(IsCheckedProperty, value);
            }
        }

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
            "IsChecked",
            typeof(bool),
            typeof(FilterSelectorItem),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public FilterSelectorItem()
        {
            InitializeComponent();
        }
        
        private void mainControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            IsChecked = !IsChecked;
        }
    }
}

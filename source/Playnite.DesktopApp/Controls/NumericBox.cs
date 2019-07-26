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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Playnite.DesktopApp.Controls
{
    public class NumericBox : TextBox
    {        
        public long MinLongValue
        {
            get
            {
                return (long)GetValue(MinLongValueProperty);
            }

            set
            {
                SetValue(MinLongValueProperty, value);
            }
        }

        public long MaxLongValue
        {
            get
            {
                return (long)GetValue(MaxLongValueProperty);
            }

            set
            {
                SetValue(MaxLongValueProperty, value);
            }
        }

        private long lastLongValue;
        public long LongValue
        {
            get
            {
                return (long)GetValue(LongValueProperty);
            }

            set
            {
                lastLongValue = value;
                SetValue(LongValueProperty, value);
            }
        }

        public static readonly DependencyProperty LongValueProperty =
            DependencyProperty.Register(nameof(LongValue), typeof(long), typeof(NumericBox),
                new FrameworkPropertyMetadata((long)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, LongValuePropertyChanged, CoerceLongValue, false, UpdateSourceTrigger.PropertyChanged));

        public static readonly DependencyProperty MinLongValueProperty =
            DependencyProperty.Register(nameof(MinLongValue), typeof(long), typeof(NumericBox),
                new PropertyMetadata((long)0, MinLongValuePropertyChanged));


        public static readonly DependencyProperty MaxLongValueProperty =
            DependencyProperty.Register(nameof(MaxLongValue), typeof(long), typeof(NumericBox),
                new PropertyMetadata(long.MaxValue, MaxLongValuePropertyChanged));

        static NumericBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumericBox), new FrameworkPropertyMetadata(typeof(NumericBox)));
        }

        public NumericBox()
        {
            Text = LongValue.ToString();
            LostFocus += NumericBox_LostFocus;
            Loaded += NumericBox_Loaded;
            TextChanged += NumericBox_TextChanged;
        }

        private void NumericBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumericBox_LostFocus(sender, e);
        }

        private void NumericBox_Loaded(object sender, RoutedEventArgs e)
        {
            lastLongValue = LongValue;
        }

        private void NumericBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
            {
                Text = "0";
            }

            if (!long.TryParse(Text, out var result))
            {
                e.Handled = true;
                LongValue = lastLongValue;
            }
            else
            {
                if (result >= MinLongValue && result <= MaxLongValue)
                {
                    LongValue = result;
                }
                else
                {
                    e.Handled = true;
                    LongValue = lastLongValue;
                }
            }
        }

        private static object CoerceLongValue(DependencyObject element, object baseValue)
        {
            var box = (NumericBox)element;
            var value = (long)baseValue;            
            box.Text = value.ToString();
            return value;
        }

        private static void LongValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {                      
        }

        private static void MinLongValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void MaxLongValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}

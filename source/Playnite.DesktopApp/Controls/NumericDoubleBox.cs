using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
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
    public class DoubleNumericBox : TextBox
    {
        public double MinValue
        {
            get
            {
                return (double)GetValue(MinValueProperty);
            }

            set
            {
                SetValue(MinValueProperty, value);
            }
        }

        public double MaxValue
        {
            get
            {
                return (double)GetValue(MaxValueProperty);
            }

            set
            {
                SetValue(MaxValueProperty, value);
            }
        }

        private double lastValue;
        public double Value
        {
            get
            {
                return (double)GetValue(ValueProperty);
            }

            set
            {
                lastValue = value;
                SetValue(ValueProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(DoubleNumericBox),
                new FrameworkPropertyMetadata(
                    (double)0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    DoubleValuePropertyChanged,
                    CoerceDoubleValue,
                    false,
                    UpdateSourceTrigger.PropertyChanged));

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(nameof(MinValue), typeof(double), typeof(DoubleNumericBox),
                new PropertyMetadata((double)0, MinDoubleValuePropertyChanged));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(double), typeof(DoubleNumericBox),
                new PropertyMetadata(double.MaxValue, MaxDoubleValuePropertyChanged));

        static DoubleNumericBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DoubleNumericBox), new FrameworkPropertyMetadata(typeof(DoubleNumericBox)));
        }

        public DoubleNumericBox()
        {
            Text = Value.ToString();
            LostFocus += NumericDoubleBox_LostFocus;
            Loaded += NumericDoubleBox_Loaded;
            TextChanged += NumericDoubleBox_TextChanged;
            PreviewKeyDown += NumericDoubleBox_PreviewKeyDown;
        }

        private void NumericDoubleBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.IsNumericKey() || e.Key == Key.OemPeriod || e.Key == Key.OemComma || e.Key == Key.Delete ||
                e.Key == Key.Back || e.Key == Key.Left || e.Key == Key.Right)
            {
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void NumericDoubleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            NumericDoubleBox_LostFocus(sender, e);
        }

        private void NumericDoubleBox_Loaded(object sender, RoutedEventArgs e)
        {
            lastValue = Value;
        }

        private void NumericDoubleBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
            {
                Text = "0";
            }


            if (!TryParseForCurrentCulture(Text, out var result))
            {
                e.Handled = true;
                Value = lastValue;
            }
            else
            {
                if (result >= MinValue && result <= MaxValue)
                {
                    Value = result;
                }
                else
                {
                    e.Handled = true;
                    Value = lastValue;
                    Text = lastValue.ToString();
                }
            }
        }

        private static object CoerceDoubleValue(DependencyObject element, object baseValue)
        {
            var box = (DoubleNumericBox)element;
            var current = (double)baseValue;
            if (current < box.MinValue)
            {
                current = box.MinValue;
            }

            if (current > box.MaxValue)
            {
                current = box.MaxValue;
            }

            return current;
        }

        private static void DoubleValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = (DoubleNumericBox)sender;
            if (obj.Text.IsNullOrEmpty())
            {
                obj.Text = e.NewValue.ToString();
            }
            else
            {
                if (TryParseForCurrentCulture(obj.Text.Replace(".", ","), out var result) && result == (double)e.NewValue)
                {
                }
                else
                {
                    obj.Text = e.NewValue.ToString();
                }
            }
        }

        private static void MinDoubleValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void MaxDoubleValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private static bool TryParseForCurrentCulture(string text, out double result)
        {
            var numberFormat = CultureInfo.CurrentCulture.NumberFormat;
            var parsedText = text.Replace(",", numberFormat.NumberDecimalSeparator)
                .Replace(".", numberFormat.NumberDecimalSeparator);

            return double.TryParse(parsedText, out result);
        }
    }
}

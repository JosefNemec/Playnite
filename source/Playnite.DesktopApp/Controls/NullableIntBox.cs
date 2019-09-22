using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Playnite.DesktopApp.Controls
{
    public class NullableIntBox : TextBox
    {        
        public int MinValue
        {
            get
            {
                return (int)GetValue(MinValueProperty);
            }

            set
            {
                SetValue(MinValueProperty, value);
            }
        }

        public int MaxValue
        {
            get
            {
                return (int)GetValue(MaxValueProperty);
            }

            set
            {
                SetValue(MaxValueProperty, value);
            }
        }

        private int? lastValue;
        public int? Value
        {
            get
            {
                return (int?)GetValue(ValueProperty);
            }

            set
            {
                lastValue = value;
                SetValue(ValueProperty, value);
            }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int?), typeof(NullableIntBox),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ValuePropertyChanged, CoerceValue, false, UpdateSourceTrigger.PropertyChanged));

        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register(nameof(MinValue), typeof(int?), typeof(NullableIntBox),
                new PropertyMetadata(0, MinValuePropertyChanged));

        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register(nameof(MaxValue), typeof(int?), typeof(NullableIntBox),
                new PropertyMetadata(int.MaxValue, MaxValuePropertyChanged));

        static NullableIntBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NullableIntBox), new FrameworkPropertyMetadata(typeof(NullableIntBox)));
        }

        public NullableIntBox()
        {
            Text = Value.ToString();
            LostFocus += NumericBox_LostFocus;
            Loaded += NumericBox_Loaded;
        }

        private void NumericBox_Loaded(object sender, RoutedEventArgs e)
        {
            lastValue = Value;
        }

        private void NumericBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Text))
            {
                Value = null;
            }
            else
            {
                if (!int.TryParse(Text, out var result))
                {
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
                        Value = lastValue;
                    }
                }
            }
        }

        private static object CoerceValue(DependencyObject element, object baseValue)
        {
            var box = (NullableIntBox)element;
            var value = (int?)baseValue;
            if (value == null)
            {
                box.Text = string.Empty;
            }
            else
            {
                box.Text = value.ToString();
            }

            return value;
        }

        private static void ValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {                      
        }

        private static void MinValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }

        private static void MaxValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}

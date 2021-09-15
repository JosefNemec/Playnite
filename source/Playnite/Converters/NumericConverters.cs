using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class NullableIntToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }
            else if (value is int num)
            {
                return num.ToString();
            }

            throw new NotSupportedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = (string)value;
            if (str.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                return int.Parse(str);
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class NullableIntFieldValidation : ValidationRule
    {
        private string invalidInput => $"Not an integer value in {MinValue} to {MaxValue} range!";

        public int MinValue { get; set; }
        public int MaxValue { get; set; }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (value == null)
            {
                return new ValidationResult(true, null);
            }
            else
            {
                var str = (string)value;
                if (str.IsNullOrEmpty())
                {
                    return new ValidationResult(true, null);
                }

                if (int.TryParse(str, out var intVal) && intVal >= MinValue && intVal <= MaxValue)
                {
                    return new ValidationResult(true, null);
                }

                return new ValidationResult(false, invalidInput);
            }
        }
    }
}

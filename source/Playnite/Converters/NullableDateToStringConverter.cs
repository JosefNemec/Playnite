using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class NullableDateToStringConverter : MarkupExtension, IValueConverter
    {
        public static readonly NullableDateToStringConverter Instance = new NullableDateToStringConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is DateTime date)
            {
                if (parameter is DateFormattingOptions options)
                {
                    return date.ToDisplayString(options);
                }

                return date.ToString(Common.Constants.DateUiFormat);
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (string.IsNullOrEmpty(value as string))
            {
                return null;
            }

            var sucess = DateTime.TryParseExact(value as string, Common.Constants.DateUiFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var newDate);
            if (sucess)
            {
                return newDate;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class ReleaseDateToStringConverter : MarkupExtension, IValueConverter
    {
        public static readonly ReleaseDateToStringConverter Instance = new ReleaseDateToStringConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ReleaseDate date)
            {
                if (parameter is ReleaseDateFormattingOptions options)
                {
                    return date.ToDisplayString(options);
                }
                else if (date.Month != null && date.Day != null)
                {
                    return date.Date.ToString(Common.Constants.DateUiFormat);
                }
                else
                {
                    return date.Serialize();
                }
            }
            else if (value == null)
            {
                return string.Empty;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value as string;
            if (str.IsNullOrEmpty())
            {
                return null;
            }

            return ReleaseDate.Deserialize(str);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class EditingReleaseDateToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ReleaseDate date)
            {
                return date.Serialize();
            }
            else if (value == null)
            {
                return null;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value as string;
            if (str.IsNullOrEmpty())
            {
                return null;
            }

            return ReleaseDate.Deserialize(str);
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class ReleaseDateFieldValidation : ValidationRule
    {
        private const string InvalidInput = "Release date must be in year-month-day format!";

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            if (!string.IsNullOrEmpty((string)value))
            {
                if (!ReleaseDate.TryDeserialize((string)value, out var _))
                {
                    return new ValidationResult(false, InvalidInput);
                }
            }

            return new ValidationResult(true, null);
        }
    }

    public class DateTimeFormatToStringValidation : ValidationRule
    {
        private const string InvalidFormatInput = "Format does not contain a valid custom format pattern!";
        private const string InvalidArgumentRangeInput = "The date and time is outside the range of dates supported!";
        private static DateTime TestDate = DateTime.Now;

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            var str = (string)value;
            try
            {
                TestDate.ToString(str);
                return new ValidationResult(true, null);
            }
            catch (FormatException)
            {
                return new ValidationResult(false, InvalidFormatInput);
            }
            catch (ArgumentOutOfRangeException)
            {
                return new ValidationResult(false, InvalidArgumentRangeInput);
            }
        }
    }
}

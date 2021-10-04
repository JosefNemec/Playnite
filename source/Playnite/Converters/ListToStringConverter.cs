using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class NiceListToStringConverter : MarkupExtension, IValueConverter
    {        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return string.Join(", ", (IEnumerable<object>)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var stringVal = (string)value;

            if (string.IsNullOrEmpty(stringVal))
            {
                return null;
            }
            else
            {
                var converted = stringVal.Split(new char[] { ',' }).Select(a => a.Trim());
                if (targetType == typeof(ComparableList<string>))
                {
                    return new ComparableList<string>(converted);
                }
                else
                {
                    return converted.ToList();
                }
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }

    public class ListToStringConverter : MarkupExtension, IValueConverter
    {
        private const string defaultSeperator = ",";
    
        public static string MakeString(IEnumerable<string> source)
        {
            return string.Join(",", source);
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }
            string sep = defaultSeperator;
            if (parameter is string customSep)
            {
                sep = customSep;
            }
            
            return string.Join(sep, (IEnumerable<object>)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var stringVal = (string)value;

            if (string.IsNullOrEmpty(stringVal))
            {
                return null;
            }
            else
            {
                string sep = defaultSeperator;
                
                if (parameter is string customSep)
                {
                    sep = customSep;
                }
                
                var converted = stringVal.Split(new [] { sep }, StringSplitOptions.RemoveEmptyEntries);
                if (targetType == typeof(ComparableList<string>))
                {
                    return new ComparableList<string>(converted);
                }
                else
                {
                    return converted.ToList();
                }
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

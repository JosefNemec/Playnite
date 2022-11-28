using Playnite.SDK;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class GenericTypeConverter : MarkupExtension, IValueConverter
    {
        public IValueConverter CustomConverter { get; set; }
        public string StringFormat { get; set; }
        public bool TestAsFilePath { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                object actualValue = value;
                if (!StringFormat.IsNullOrEmpty() && value is string)
                {
                    actualValue = string.Format(StringFormat, value);
                }

                if (CustomConverter != null)
                {
                    if (TestAsFilePath && actualValue is string filePath)
                    {
                        if (File.Exists(filePath))
                        {
                            return CustomConverter.Convert(filePath, targetType, parameter, culture);
                        }
                        else
                        {
                            return DependencyProperty.UnsetValue;
                        }
                    }
                    else
                    {
                        return CustomConverter.Convert(actualValue, targetType, parameter, culture);
                    }
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(targetType);
                    if (TestAsFilePath && actualValue is string filePath)
                    {
                        if (File.Exists(filePath))
                        {
                            return converter.ConvertFrom(filePath);
                        }
                        else
                        {
                            return DependencyProperty.UnsetValue;
                        }
                    }
                    else
                    {
                        return converter.ConvertFrom(actualValue);
                    }
                }
            }
            catch
            {
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

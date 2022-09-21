using Playnite.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Extensions.Markup
{
    public class PluginConverter : MarkupExtension
    {
        public string Converter { get; set; }
        public string Plugin { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return new PluginConverterProvider(Plugin, Converter);
        }
    }

    // This intermediary class is needed because markup extensions are resolved when theme's xaml is loaded,
    // which happens before plugins are loaded.
    public class PluginConverterProvider : IValueConverter
    {
        private bool converterRequested = false;
        private IValueConverter actualConverter;

        private readonly string converterName;
        private readonly string pluginSource;

        public PluginConverterProvider(string plugin, string converter)
        {
            pluginSource = plugin;
            converterName = converter;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetConverter()?.Convert(value, targetType, parameter, culture) ?? DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return GetConverter()?.ConvertBack(value, targetType, parameter, culture) ?? DependencyProperty.UnsetValue;
        }

        private IValueConverter GetConverter()
        {
            if (converterRequested)
            {
                return actualConverter;
            }

            var pSource = PlayniteApplication.Current?.Extensions?.ConvertersSupportList.FirstOrDefault(a => a.SourceName == pluginSource);
            actualConverter = null;
            if (pSource != null)
            {
                actualConverter = pSource.Converters?.FirstOrDefault(a => a.GetType().Name == converterName);
            }

            converterRequested = true;
            return actualConverter;
        }
    }
}

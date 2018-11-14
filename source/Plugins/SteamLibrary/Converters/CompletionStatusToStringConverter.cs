using Playnite;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace SteamLibrary
{
    public class BackgroundSourceToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (BackgroundSource)value;
            switch (source)
            {
                case BackgroundSource.Image:
                    return DefaultResourceProvider.FindString("LOCSteamBackgroundSourceImage");
                case BackgroundSource.StoreScreenshot:
                    return DefaultResourceProvider.FindString("LOCSteamBackgroundSourceScreenshot");
                case BackgroundSource.StoreBackground:
                    return DefaultResourceProvider.FindString("LOCSteamBackgroundSourceStore");
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

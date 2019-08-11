using Playnite;
using Playnite.SDK;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class AfterGameLaunchOptionToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (AfterLaunchOptions)value;
            switch (source)
            {
                case AfterLaunchOptions.None:
                    return ResourceProvider.GetString("LOCDoNothing");
                case AfterLaunchOptions.Minimize:
                    return ResourceProvider.GetString("LOCMinimize");
                case AfterLaunchOptions.Close:
                    return ResourceProvider.GetString("LOCClose");
                default:
                    return string.Empty;
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

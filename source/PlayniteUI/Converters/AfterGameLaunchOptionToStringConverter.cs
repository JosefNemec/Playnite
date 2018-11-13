using Playnite;
using Playnite.SDK;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlayniteUI
{
    public class AfterGameLaunchOptionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (AfterLaunchOptions)value;
            switch (source)
            {
                case AfterLaunchOptions.None:
                    return DefaultResourceProvider.FindString("LOCDoNothing");
                case AfterLaunchOptions.Minimize:
                    return DefaultResourceProvider.FindString("LOCMinimize");
                case AfterLaunchOptions.Close:
                    return DefaultResourceProvider.FindString("LOCClose");
                default:
                    return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

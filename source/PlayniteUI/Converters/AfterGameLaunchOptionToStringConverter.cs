using Playnite;
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
            var resources = new ResourceProvider();
            switch (source)
            {
                case AfterLaunchOptions.None:
                    return resources.FindString("LOCDoNothing");
                case AfterLaunchOptions.Minimize:
                    return resources.FindString("LOCMinimize");
                case AfterLaunchOptions.Close:
                    return resources.FindString("LOCClose");
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

using Playnite;
using Playnite.MetaProviders;
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
                    return resources.FindString("DoNothing");
                case AfterLaunchOptions.Minimize:
                    return resources.FindString("Minimize");
                case AfterLaunchOptions.Close:
                    return resources.FindString("Close");
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

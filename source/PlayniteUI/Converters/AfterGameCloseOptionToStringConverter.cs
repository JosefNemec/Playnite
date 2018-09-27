using Playnite;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlayniteUI
{
    public class AfterGameCloseOptionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (AfterGameCloseOptions)value;
            var resources = new ResourceProvider();
            switch (source)
            {
                case AfterGameCloseOptions.None:
                    return resources.FindString("LOCDoNothing");
                case AfterGameCloseOptions.Restore:
                    return resources.FindString("LOCRestoreWindow");
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

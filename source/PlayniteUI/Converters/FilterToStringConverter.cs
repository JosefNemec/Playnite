using Playnite;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace PlayniteUI
{
    public class FilterToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            // TODO: this is just for fullscreen mode right now where only three filter options are available.
            // This should be extended in future.
            var settings = (FilterSettings)value;
            if (settings.IsInstalled)
            {
                return DefaultResourceProvider.FindString("LOCGameIsInstalledTitle");
            }
            else if (settings.IsUnInstalled)
            {
                return DefaultResourceProvider.FindString("LOCGameIsUnInstalledTitle");
            }
            else
            {
                return DefaultResourceProvider.FindString("LOCAllGames");
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

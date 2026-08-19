using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;
using Playnite.Database;
using Playnite.SDK.Models;

namespace Playnite.Converters
{
    public class LibraryObjectNameMatchToBoolConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var match = false;
            if (parameter is not string compareName)
            {
                match = false;
            }
            else
            {
                if (value is DatabaseObject obj)
                    match = GameFieldComparer.StringEquals(obj.Name, compareName);

                else if (value is IEnumerable<DatabaseObject> enumerable)
                    match = enumerable.Any(a => GameFieldComparer.StringEquals(a.Name, compareName));
            }

            if (targetType == typeof(Visibility))
                return match ? Visibility.Visible : Visibility.Collapsed;

            return match;
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
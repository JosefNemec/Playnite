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
using System.Windows.Media;

namespace Playnite.Converters
{
    public class StrechToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (Stretch)value;
            switch (source)
            {
                case Stretch.None:
                    return ResourceProvider.GetString("LOCStrechNone");
                case Stretch.Fill:
                    return ResourceProvider.GetString("LOCStrechFill");
                case Stretch.Uniform:
                    return ResourceProvider.GetString("LOCStrechUniform");
                case Stretch.UniformToFill:
                    return ResourceProvider.GetString("LOCStrechUniformToFill");
            }

            return "<UknownStrechMode>";
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

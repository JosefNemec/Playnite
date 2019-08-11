using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Playnite.Database;
using NLog;
using System.IO;
using Playnite;
using System.Windows.Markup;
using Playnite.Common.Web;
using Playnite.Settings;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using Playnite.Common;
using System.Drawing.Imaging;

namespace Playnite.Converters
{
    public class ImageStringToImageConverter : MarkupExtension, IValueConverter
    {
        public bool Cached { get; set; }

        public ImageStringToImageConverter()
        {
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }

            var image = ImageSourceManager.GetImage((string)value, Cached);
            return image ?? DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

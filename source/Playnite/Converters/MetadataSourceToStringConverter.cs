using Playnite;
using Playnite.Metadata;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Converters
{
    public class MetadataSourceToStringConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (MetadataSource)value;
            switch (source)
            {
                case MetadataSource.Store:
                    return ResourceProvider.GetString("LOCMetaSourceStore");
                case MetadataSource.IGDB:
                    return ResourceProvider.GetString("LOCMetaSourceIGDB");
                case MetadataSource.IGDBOverStore:
                    return ResourceProvider.GetString("LOCMetaSourceIGDBOverStore");
                case MetadataSource.StoreOverIGDB:
                    return ResourceProvider.GetString("LOCMetaSourceStoreOverIGDB");
                default:
                    return string.Empty;
            }
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

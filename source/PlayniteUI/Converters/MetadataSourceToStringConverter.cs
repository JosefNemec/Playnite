using Playnite;
using Playnite.Metadata;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlayniteUI
{
    public class MetadataSourceToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (MetadataSource)value;
            switch (source)
            {
                case MetadataSource.Store:
                    return DefaultResourceProvider.FindString("LOCMetaSourceStore");
                case MetadataSource.IGDB:
                    return DefaultResourceProvider.FindString("LOCMetaSourceIGDB");
                case MetadataSource.IGDBOverStore:
                    return DefaultResourceProvider.FindString("LOCMetaSourceIGDBOverStore");
                case MetadataSource.StoreOverIGDB:
                    return DefaultResourceProvider.FindString("LOCMetaSourceStoreOverIGDB");
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

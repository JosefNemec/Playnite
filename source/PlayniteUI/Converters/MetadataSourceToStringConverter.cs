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
    public class MetadataSourceToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var source = (MetadataSource)value;
            var resources = new ResourceProvider();
            switch (source)
            {
                case MetadataSource.Store:
                    return resources.FindString("LOCMetaSourceStore");
                case MetadataSource.IGDB:
                    return resources.FindString("LOCMetaSourceIGDB");
                case MetadataSource.IGDBOverStore:
                    return resources.FindString("LOCMetaSourceIGDBOverStore");
                case MetadataSource.StoreOverIGDB:
                    return resources.FindString("LOCMetaSourceStoreOverIGDB");
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

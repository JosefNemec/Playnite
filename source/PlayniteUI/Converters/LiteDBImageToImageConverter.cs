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

namespace PlayniteUI
{
    public class LiteDBImageToImageConverter : IValueConverter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var imageId = (string)value;
            if (string.IsNullOrEmpty(imageId))
            {
                return DependencyProperty.UnsetValue;
            }
            
            using (var imageData = GameDatabase.Instance.GetFileStream(imageId))
            {
                if (imageData == null)
                {
                    logger.Warn("Image not found in database: " + imageId);
                    return DependencyProperty.UnsetValue;
                }

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = imageData;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}

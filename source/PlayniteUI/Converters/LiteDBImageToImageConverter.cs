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

namespace PlayniteUI
{
    public class LiteDBImageToImageConverter : IValueConverter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public static bool IsCacheEnabled
        {
            get; set;
        } = false;

        public static Dictionary<string, BitmapImage> Cache
        {
            get; set;
        } = new Dictionary<string, BitmapImage>();

        public static void ClearCache()
        {
            Cache.Clear();
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return DependencyProperty.UnsetValue;
            }            

            var imageId = (string)value;
            if (string.IsNullOrEmpty(imageId))
            {
                return DependencyProperty.UnsetValue;
            }

            if (imageId.StartsWith("resources:"))
            {
                return imageId.Replace("resources:", "");
            }

            if (IsCacheEnabled && Cache.ContainsKey(imageId))
            {
                return Cache[imageId];
            }
            else
            {
                if (File.Exists(imageId))
                {
                    return imageId;
                }

                var imageData = GameDatabase.Instance.GetFileImage(imageId);
                if (imageData == null)
                {
                    logger.Warn("Image not found in database: " + imageId);
                    return DependencyProperty.UnsetValue;
                }
                else
                {
                    if (IsCacheEnabled)
                    {
                        Cache.Add(imageId, imageData);
                    }

                    return imageData;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}

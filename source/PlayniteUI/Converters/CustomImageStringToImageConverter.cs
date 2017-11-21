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

namespace PlayniteUI
{
    public class CustomImageStringToImageConverter : IValueConverter
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
                
        public static GameDatabase Database
        {
            get; set;
        }

        public static Dictionary<string, BitmapImage> Cache
        {
            get; set;
        } = new Dictionary<string, BitmapImage>();

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

            if (imageId.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                var cachedFile = Web.GetCachedWebFile(imageId);
                if (string.IsNullOrEmpty(cachedFile))
                {
                    logger.Warn("Web file not found: " + imageId);
                    return DependencyProperty.UnsetValue;
                }

                try
                {
                    return BitmapExtensions.BitmapFromFile(cachedFile);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, $"Failed to create bitmap from {cachedFile} file.");
                    return DependencyProperty.UnsetValue;
                }
            }

            if (File.Exists(imageId))
            {
                return BitmapExtensions.BitmapFromFile(imageId);
            }

            try
            {
                if (Database == null)
                {
                    logger.Error("Cannot load database image, database not found.");
                    return DependencyProperty.UnsetValue;
                }

                var imageData = Database.GetFileImage(imageId);
                if (imageData == null)
                {
                    logger.Warn("Image not found in database: " + imageId);
                    return DependencyProperty.UnsetValue;
                }
                else
                {
                    return imageData;
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to load image from database.");
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}

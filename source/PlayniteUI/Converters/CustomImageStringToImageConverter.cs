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

namespace PlayniteUI
{
    public class CustomImageStringToImageConverter : MarkupExtension, IValueConverter
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
                try
                {
                    var cachedFile = Web.GetCachedWebFile(imageId);
                    if (string.IsNullOrEmpty(cachedFile))
                    {
                        logger.Warn("Web file not found: " + imageId);
                        return DependencyProperty.UnsetValue;
                    }

                    return BitmapExtensions.BitmapFromFile(cachedFile);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, $"Failed to create bitmap from {imageId} file.");
                    return DependencyProperty.UnsetValue;
                }
            }

            if (File.Exists(imageId))
            {
                try
                {
                    return BitmapExtensions.BitmapFromFile(imageId);
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to create bitmap from " + imageId);
                    return DependencyProperty.UnsetValue;
                }
            }

            try
            {
                if (Database == null)
                {
                    logger.Error("Cannot load database image, database not found.");
                    return DependencyProperty.UnsetValue;
                }

                try
                {
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
                    logger.Error(exc, $"Failed to get bitmap from {imageId} database file.");
                    return DependencyProperty.UnsetValue;
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

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}

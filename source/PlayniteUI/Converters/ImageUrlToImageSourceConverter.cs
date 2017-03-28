using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.IO;
using Playnite;
using System.Net;

namespace PlayniteUI
{
    public class ImageUrlToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var url = (string)value;
            if (string.IsNullOrEmpty(url))
            {
                return DependencyProperty.UnsetValue;
            }

            var extension = Path.GetExtension(url);
            var md5 = url.MD5();
            var cacheFile = Path.Combine(Paths.ImagesCachePath, md5 + extension);

            if (!File.Exists(cacheFile))
            {
                FileSystem.CreateFolder(Paths.ImagesCachePath);

                try
                {
                    Web.DownloadFile(url, cacheFile);
                }
                catch (WebException e)
                {
                    if (e.Response == null)
                    {
                        throw;
                    }

                    var response = (HttpWebResponse)e.Response;
                    if (response.StatusCode != HttpStatusCode.NotFound)
                    {
                        throw;
                    }
                    else
                    {
                        return DependencyProperty.UnsetValue;
                    }
                }
            }

            return cacheFile;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}

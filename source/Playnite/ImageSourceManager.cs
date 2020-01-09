using Playnite.Common;
using Playnite.Database;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Playnite
{
    public class ImageSourceManager
    {
        private static ILogger logger = LogManager.GetLogger();
        private static GameDatabase database;
        public static MemoryCache Cache = new MemoryCache(Units.MegaBytesToBytes(100));
        private const string btmpPropsFld = "bitmappros";

        public static void SetDatabase(GameDatabase db)
        {
            if (database != null)
            {
                database.DatabaseFileChanged -= Database_DatabaseFileChanged;
            }

            database = db;
            database.DatabaseFileChanged += Database_DatabaseFileChanged;
        }

        private static void Database_DatabaseFileChanged(object sender, DatabaseFileEventArgs args)
        {
            if (args.EventType == FileEvent.Removed)
            {
                Cache.TryRemove(args.FileId, out var file);
            }
        }

        public static string GetImagePath(string source)
        {
            if (source.IsNullOrEmpty())
            {
                return null;
            }

            if (source.StartsWith("resources:") || source.StartsWith("pack://"))
            {
                try
                {
                    var imagePath = source;
                    if (source.StartsWith("resources:"))
                    {
                        imagePath = source.Replace("resources:", "pack://application:,,,");
                    }

                    return imagePath;
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to create bitmap from resources " + source);
                    return null;
                }
            }

            if (StringExtensions.IsHttpUrl(source))
            {
                try
                {
                    var cachedFile = HttpFileCache.GetWebFile(source);
                    if (string.IsNullOrEmpty(cachedFile))
                    {
                        logger.Warn("Web file not found: " + source);
                        return null;
                    }

                    return cachedFile;
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, $"Failed to create bitmap from {source} file.");
                    return null;
                }
            }

            if (File.Exists(source))
            {
                return source;
            }

            if (database == null)
            {
                logger.Error("Cannot load database image, database not found.");
                return null;
            }

            try
            {
                return database.GetFullFilePath(source);

            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, $"Failed to get bitmap from {source} database file.");
                return null;
            }
        }

        public static BitmapImage GetResourceImage(string resourceKey, bool cached, BitmapLoadProperties loadProperties = null)
        {
            if (cached && Cache.TryGet(resourceKey, out var image))
            {
                BitmapLoadProperties existingMetadata = null;
                if (image.Metadata.TryGetValue(btmpPropsFld, out object metaValue))
                {
                    existingMetadata = (BitmapLoadProperties)metaValue;
                }

                if (existingMetadata?.MaxDecodePixelWidth == loadProperties?.MaxDecodePixelWidth)
                {
                    return image.CacheObject as BitmapImage;
                }
                else
                {
                    Cache.TryRemove(resourceKey);
                }
            }

            var resource = ResourceProvider.GetResource(resourceKey) as BitmapImage;
            if (loadProperties?.MaxDecodePixelWidth > 0 && resource?.PixelWidth > loadProperties?.MaxDecodePixelWidth)
            {
                resource = resource.GetClone(loadProperties);
            }

            if (cached)
            {
                Cache.TryAdd(resourceKey, resource, resource.GetSizeInMemory(),
                    new Dictionary<string, object>
                    {
                        { btmpPropsFld, loadProperties }
                    });
            }

            return resource;
        }

        public static BitmapImage GetImage(string source, bool cached, BitmapLoadProperties loadProperties = null)
        {
            if (DesignerTools.IsInDesignMode)
            {
                cached = false;
            }

            if (source.IsNullOrEmpty())
            {
                return null;
            }

            if (cached && Cache.TryGet(source, out var image))
            {
                BitmapLoadProperties existingMetadata = null;
                if (image.Metadata.TryGetValue(btmpPropsFld, out object metaValue))
                {
                    existingMetadata = (BitmapLoadProperties)metaValue;
                }

                if (existingMetadata == loadProperties)
                {
                    return image.CacheObject as BitmapImage;
                }
                else
                {
                    Cache.TryRemove(source);
                }
            }

            if (source.StartsWith("resources:") || source.StartsWith("pack://"))
            {
                try
                {
                    var imagePath = source;
                    if (source.StartsWith("resources:"))
                    {
                        imagePath = source.Replace("resources:", "pack://application:,,,");
                    }

                    var streamInfo = Application.GetResourceStream(new Uri(imagePath));
                    using (var stream = streamInfo.Stream)
                    {
                        var imageData = BitmapExtensions.BitmapFromStream(stream, loadProperties);
                        if (imageData != null)
                        {
                            if (cached)
                            {
                                Cache.TryAdd(source, imageData, imageData.GetSizeInMemory(),
                                    new Dictionary<string, object>
                                    {
                                    { btmpPropsFld, loadProperties }
                                    });
                            }

                            return imageData;
                        }
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to create bitmap from resources " + source);
                    return null;
                }
            }

            if (StringExtensions.IsHttpUrl(source))
            {
                try
                {
                    var cachedFile = HttpFileCache.GetWebFile(source);
                    if (string.IsNullOrEmpty(cachedFile))
                    {
                        logger.Warn("Web file not found: " + source);
                        return null;
                    }

                    return BitmapExtensions.BitmapFromFile(cachedFile, loadProperties);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, $"Failed to create bitmap from {source} file.");
                    return null;
                }
            }

            if (File.Exists(source))
            {
                try
                {
                    var imageData = BitmapExtensions.BitmapFromFile(source, loadProperties);
                    if (imageData != null)
                    {
                        if (cached)
                        {
                            Cache.TryAdd(source, imageData, imageData.GetSizeInMemory(),
                                new Dictionary<string, object>
                                {
                                    { btmpPropsFld, loadProperties }
                                });
                        }

                        return imageData;
                    }
                }
                catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(e, "Failed to create bitmap from " + source);
                    return null;
                }
            }

            try
            {
                if (database == null)
                {
                    logger.Error("Cannot load database image, database not found.");
                    return null;
                }

                try
                {
                    var imageData = database.GetFileAsImage(source, loadProperties);
                    if (imageData == null)
                    {
                        logger.Warn("Image not found in database: " + source);
                        return null;
                    }
                    else
                    {
                        if (cached)
                        {
                            Cache.TryAdd(source, imageData, imageData.GetSizeInMemory(),
                                new Dictionary<string, object>
                                {
                                    { btmpPropsFld, loadProperties }
                                });
                        }

                        return imageData;
                    }

                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    logger.Error(exc, $"Failed to get bitmap from {source} database file.");
                    return null;
                }
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(exc, "Failed to load image from database.");
                return null;
            }
        }
    }
}

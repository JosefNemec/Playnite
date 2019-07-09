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

namespace Playnite
{
    public class ImageSourceManager
    {
        private static ILogger logger = LogManager.GetLogger();
        private static GameDatabase database;
        internal static MemoryCache Cache = new MemoryCache(Units.MegaBytesToBytes(100));

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

        public static object GetImage(string source, bool cached)
        {
            if (DesignerTools.IsInDesignMode)
            {
                cached = false;
            }

            if (source.IsNullOrEmpty())
            {
                return null;
            }

            if (source.StartsWith("resources:"))
            {
                if (cached && Cache.TryGet(source, out var image))
                {
                    return image;
                }
                else
                {
                    try
                    {
                        var imagePath = source.Replace("resources:", "pack://application:,,,");
                        var imageData = BitmapExtensions.BitmapFromFile(imagePath);
                        if (imageData != null)
                        {
                            if (cached)
                            {
                                Cache.TryAdd(source, imageData, imageData.GetSizeInMemory());
                            }
                            return imageData;
                        }
                    }
                    catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
                    {
                        logger.Error(e, "Failed to create bitmap from resources " + source);
                        return null;
                    }
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

                    return BitmapExtensions.BitmapFromFile(cachedFile);
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
                    var imageData = BitmapExtensions.BitmapFromFile(source);
                    if (imageData != null)
                    {
                        if (cached)
                        {
                            Cache.TryAdd(source, imageData, imageData.GetSizeInMemory());
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
                    if (cached && Cache.TryGet(source, out var image))
                    {
                        return image;
                    }

                    var imageData = database.GetFileAsImage(source);
                    if (imageData == null)
                    {
                        logger.Warn("Image not found in database: " + source);
                        return null;
                    }
                    else
                    {
                        if (cached)
                        {
                            Cache.TryAdd(source, imageData, imageData.GetSizeInMemory());
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

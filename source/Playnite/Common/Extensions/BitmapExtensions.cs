using PhotoSauce.MagicScaler;
using Playnite;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImageMagick;
using TGASharpLib;

namespace System.Drawing.Imaging
{
    public class BitmapLoadProperties: IEquatable<BitmapLoadProperties>
    {
        public ImageLoadScaling Scaling { get; set; } = ImageLoadScaling.BitmapDotNet;
        public DpiScale? DpiScale { get; set; }
        public int MaxDecodePixelWidth { get; set; } = 0;
        public int MaxDecodePixelHeight { get; set; } = 0;
        public int DpiAwareMaxDecodePixelWidth =>
            DpiScale == null ? MaxDecodePixelWidth : (int)Math.Round(MaxDecodePixelWidth * DpiScale.Value.DpiScaleX);
        public int DpiAwareMaxDecodePixelHeight =>
            DpiScale == null ? MaxDecodePixelHeight : (int)Math.Round(MaxDecodePixelHeight * DpiScale.Value.DpiScaleY);
        public string Source { get; set; }

        public BitmapLoadProperties(int decodePixelWidth, int decodePixelHeight)
        {
            MaxDecodePixelWidth = decodePixelWidth;
            MaxDecodePixelHeight = decodePixelHeight;
        }

        public BitmapLoadProperties(int decodePixelWidth, int decodePixelHeight, DpiScale? dpiScale) : this(decodePixelWidth, decodePixelHeight)
        {
            DpiScale = dpiScale;
        }

        public BitmapLoadProperties(int decodePixelWidth, int decodePixelHeight, DpiScale? dpiScale, ImageLoadScaling scaling) : this(decodePixelWidth, decodePixelHeight, dpiScale)
        {
            Scaling = scaling;
        }

        public bool Equals(BitmapLoadProperties other)
        {
            if (other is null)
            {
                return false;
            }

            if (DpiScale?.Equals(other.DpiScale) == false)
            {
                return false;
            }

            if (MaxDecodePixelWidth != other.MaxDecodePixelWidth)
            {
                return false;
            }

            if (MaxDecodePixelHeight != other.MaxDecodePixelHeight)
            {
                return false;
            }

            if (!string.Equals(Source, other.Source, StringComparison.Ordinal))
            {
                return false;
            }

            if (Scaling != other.Scaling)
            {
                return false;
            }

            return true;
        }

        public override bool Equals(object obj) => Equals(obj as BitmapLoadProperties);

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(BitmapLoadProperties obj1, BitmapLoadProperties obj2)
        {
            if (obj1 is null && obj2 is null)
            {
                return true;
            }
            else
            {
                return obj1?.Equals(obj2) == true;
            }
        }

        public static bool operator !=(BitmapLoadProperties obj1, BitmapLoadProperties obj2)
        {
            return obj1?.Equals(obj2) == false;
        }

        public override string ToString()
        {
            return $"{MaxDecodePixelWidth}x{MaxDecodePixelHeight};{DpiScale?.DpiScaleX}x{DpiScale?.DpiScaleY};{Source};{Scaling}";
        }
    }

    public static partial class BitmapExtensions
    {
        private static ILogger logger = LogManager.GetLogger();
        private static readonly byte[] webpSig = new byte[] { 0x57, 0x45, 0x42, 0x50 };
        // Apparently both ftypavif and ftypmif1 can appear in avif encoded images
        private static readonly byte[] avif1 = new byte[] { 0x66, 0x74, 0x79, 0x70, 0x61, 0x76, 0x69, 0x66 };
        private static readonly byte[] avif2 = new byte[] { 0x66, 0x74, 0x79, 0x70, 0x6d, 0x69, 0x66, 0x31 };

        public static BitmapSource CreateSourceFromURI(Uri imageUri)
        {
            return CreateSourceFromURI(imageUri.LocalPath);
        }

        public static BitmapSource CreateSourceFromURI(string imageUri)
        {
            return BitmapFromFile(imageUri);
        }

        public static byte[] ToPngArray(this BitmapImage image)
        {
            return ToPngArray((BitmapSource)image);
        }

        public static byte[] ToPngArray(this BitmapSource image)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return stream.ToArray();
            }
        }

        public static BitmapSource GetClone(this BitmapSource image, BitmapLoadProperties loadProperties = null)
        {
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                return BitmapExtensions.BitmapFromStream(stream, loadProperties);
            }
        }

        public static Windows.Controls.Image ToImage(this BitmapSource bitmap, BitmapScalingMode scaling = BitmapScalingMode.Fant)
        {
            var image = new Windows.Controls.Image()
            {
                Source = bitmap
            };

            RenderOptions.SetBitmapScalingMode(image, scaling);
            return image;
        }

        public static BitmapSource BitmapFromFile(string imagePath, BitmapLoadProperties loadProperties = null)
        {
            try
            {
                using (var fStream = FileSystem.OpenReadFileStreamSafe(imagePath))
                {
                    return BitmapFromStream(fStream, loadProperties);
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to create bitmap from file {imagePath}.");
                return null;
            }
        }

        public static bool IsFormatForImageMagickDecode(Stream stream)
        {
            if (stream.Length < 12)
                return false;

            // WEBP
            stream.Seek(8, SeekOrigin.Begin);
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            if (buffer.SequenceEqual(webpSig))
                return true;

            // AVIF
            stream.Seek(4, SeekOrigin.Begin);
            buffer = new byte[8];
            stream.Read(buffer, 0, 8);
            if (buffer.SequenceEqual(avif1))
                return true;

            if (buffer.SequenceEqual(avif2))
                return true;

            return false;
        }

        public static BitmapSource HtmlComponentImageLoader(Stream stream)
        {
            if (IsFormatForImageMagickDecode(stream))
                return BitmapFromStreamImageMagick(stream);

            // This is exactly how HTML renderer loads them by default, for compatbility reasons.
            // With an exception of IgnoreColorProfile, which speeds up image decode and doesn't really matter for our use case.
            stream.Seek(0, SeekOrigin.Begin);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = stream;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        public static BitmapSource BitmapFromStreamImageMagick(Stream stream, BitmapLoadProperties loadProperties = null)
        {
            // It doesn't look look we can decode images from ImageMagick at specific size for speed and memory gains.
            // So loadProperties are ignored for now.
            // https://github.com/dlemstra/Magick.NET/discussions/1824#discussioncomment-12810888
            //var info = new MagickImageInfo(stream);

            stream.Seek(0, SeekOrigin.Begin);
            using (var mImage = new MagickImage(stream))
            {
                // 1. Create a memory stream to bridge Magick.NET to WPF
                using (var ms = new System.IO.MemoryStream())
                {
                    // 2. Write to BMP format (fastest for WPF native decoding)
                    mImage.Write(ms, MagickFormat.Bmp);
                    ms.Seek(0, SeekOrigin.Begin);

                    // 3. Create the WPF BitmapSource
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();

                    // 4. Freeze makes the image cross-thread compatible and prevents memory leaks
                    bitmap.Freeze();
                    return bitmap;
                }
            }
        }

        // TODO: Modify scaling to scale by both axies.
        // This will currently work properly only if load properties force only width or height, not both.
        public static BitmapSource BitmapFromStream(Stream stream, BitmapLoadProperties loadProperties = null)
        {
            // Have to call .Freeze() on sources created here otherwise images decoded from non-UI
            // thread won't load on UI thread.
            try
            {
                // For webp and avif primarily, since Windows' decoder doesn't work properly with some files.
                // Primary example being images in Steam store's game descriptions.
                if (IsFormatForImageMagickDecode(stream))
                    return BitmapFromStreamImageMagick(stream, loadProperties);

                stream.Seek(0, SeekOrigin.Begin);
                var properties = Images.GetImageProperties(stream);
                var aspect = new AspectRatio(properties.Width, properties.Height);
                stream.Seek(0, SeekOrigin.Begin);
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                MemoryStream tempStream = null;
                var shouldRescale = false;

                if ((loadProperties?.MaxDecodePixelWidth > 0 && properties?.Width > loadProperties?.MaxDecodePixelWidth) ||
                    (loadProperties?.MaxDecodePixelHeight > 0 && properties?.Height > loadProperties?.MaxDecodePixelHeight))
                {
                    shouldRescale = true;
                }

                if (loadProperties?.Scaling == ImageLoadScaling.None)
                {
                    shouldRescale = false;
                }

                if (shouldRescale)
                {
                    if (loadProperties?.Scaling == ImageLoadScaling.BitmapDotNet)
                    {
                        if (loadProperties?.MaxDecodePixelWidth > 0 && properties?.Width > loadProperties?.MaxDecodePixelWidth)
                        {
                            bitmap.DecodePixelWidth = loadProperties.DpiAwareMaxDecodePixelWidth;
                        }

                        if (loadProperties?.MaxDecodePixelHeight > 0 && properties?.Height > loadProperties?.MaxDecodePixelHeight)
                        {
                            bitmap.DecodePixelHeight = loadProperties.DpiAwareMaxDecodePixelHeight;
                        }

                        if (bitmap.DecodePixelHeight != 0 && bitmap.DecodePixelWidth == 0)
                        {
                            bitmap.DecodePixelWidth = (int)Math.Round(aspect.GetWidth(bitmap.DecodePixelHeight));
                        }
                        else if (bitmap.DecodePixelWidth != 0 && bitmap.DecodePixelHeight == 0)
                        {
                            bitmap.DecodePixelHeight = (int)Math.Round(aspect.GetHeight(bitmap.DecodePixelWidth));
                        }
                    }
                    else if (loadProperties?.Scaling == ImageLoadScaling.Custom)
                    {
                        var settings = new ProcessImageSettings
                        {                            
                            Sharpen = false
                        };

                        // Use the built-in helper to set the format to BMP
                        settings.TrySetEncoderFormat(ImageMimeTypes.Bmp);

                        if (loadProperties.MaxDecodePixelWidth > 0 && properties?.Width > loadProperties?.MaxDecodePixelWidth)
                        {
                            settings.Width = loadProperties.DpiAwareMaxDecodePixelWidth;
                        }

                        if (loadProperties.MaxDecodePixelHeight > 0 && properties?.Height > loadProperties?.MaxDecodePixelHeight)
                        {
                            settings.Height = loadProperties.DpiAwareMaxDecodePixelHeight;
                        }

                        tempStream = new MemoryStream();
                        // MagicImage can't run on UI thread.
                        if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
                        {
                            Task.Run(() => MagicImageProcessor.ProcessImage(stream, tempStream, settings)).Wait();
                        }
                        else
                        {
                            MagicImageProcessor.ProcessImage(stream, tempStream, settings);
                        }

                        tempStream.Seek(0, SeekOrigin.Begin);
                    }
                }

                bitmap.StreamSource = tempStream ?? stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
                bitmap.EndInit();
                bitmap.Freeze();
                tempStream?.Dispose();
                return bitmap;
            }
            catch (Exception e)
            {
                logger.Error(e, "Failed to create bitmap from stream.");
                return null;
            }
        }

        public static long GetSizeInMemory(this BitmapSource image)
        {
            try
            {
                return Convert.ToInt64(image.PixelHeight * image.PixelWidth * 4);
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to get image size from bitmap.");
                return 0;
            }
        }

        public static BitmapImage TgaToBitmap(TGA tga)
        {
            try
            {
                using (var tgaBitmap = tga.ToBitmap())
                {
                    using (var memory = new MemoryStream())
                    {
                        tgaBitmap.Save(memory, ImageFormat.Png);
                        memory.Position = 0;
                        var bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = memory;
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();
                        bitmapImage.Freeze();
                        return bitmapImage;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to create bitmap from TGA image.");
                return null;
            }
        }

        public static Bitmap ToBitmap(this BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                var enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }

        public static BitmapImage TgaToBitmap(string tgaPath)
        {
            return TgaToBitmap(new TGA(tgaPath));
        }

        public static BitmapImage TgaToBitmap(byte[] tgaContent)
        {
            return TgaToBitmap(new TGA(tgaContent));
        }
    }
}

using PhotoSauce.MagicScaler;
using Playnite;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TGASharpLib;

namespace System.Drawing.Imaging;

public enum ImageLoadScaling
{
    None,
    BitmapDotNet,
    Custom
}

// TODO: check what we really need from these methods and delete rest

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
    public string? Source { get; set; }

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

    public bool Equals(BitmapLoadProperties? other)
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

    public override bool Equals(object? obj) => Equals(obj as BitmapLoadProperties);

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
    private static readonly ILogger logger = LogManager.GetLogger();

    public static BitmapSource? CreateSourceFromURI(Uri imageUri)
    {
        return CreateSourceFromURI(imageUri.LocalPath);
    }

    public static BitmapSource? CreateSourceFromURI(string imageUri)
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
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return stream.ToArray();
    }

    public static BitmapImage? GetClone(this BitmapImage image, BitmapLoadProperties? loadProperties = null)
    {
        var encoder = new BmpBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        return BitmapFromStream(stream, loadProperties);
    }

    public static Windows.Controls.Image ToImage(this BitmapImage bitmap, BitmapScalingMode scaling = BitmapScalingMode.Fant)
    {
        var image = new Windows.Controls.Image()
        {
            Source = bitmap
        };

        RenderOptions.SetBitmapScalingMode(image, scaling);
        return image;
    }

    public static BitmapImage? BitmapFromFile(string imagePath, BitmapLoadProperties? loadProperties = null)
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

    // TODO: Modify scaling to scale by both axies.
    // This will currently work properly only if load properties force only width or height, not both.
    public static BitmapImage? BitmapFromStream(Stream stream, BitmapLoadProperties? loadProperties = null)
    {
        try
        {
            stream.Seek(0, SeekOrigin.Begin);
            var properties = Images.GetImageProperties(stream);
            var aspect = new AspectRatio(properties.Width, properties.Height);
            stream.Seek(0, SeekOrigin.Begin);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            MemoryStream? tempStream = null;
            var shouldRescale = false;

            if ((loadProperties?.MaxDecodePixelWidth > 0 && properties.Width > loadProperties?.MaxDecodePixelWidth) ||
                (loadProperties?.MaxDecodePixelHeight > 0 && properties.Height > loadProperties?.MaxDecodePixelHeight))
            {
                shouldRescale = true;
            }

            if (loadProperties?.Scaling == ImageLoadScaling.None)
            {
                shouldRescale = false;
            }

            if (shouldRescale && loadProperties is not null)
            {
                if (loadProperties.Scaling == ImageLoadScaling.BitmapDotNet)
                {
                    if (loadProperties.MaxDecodePixelWidth > 0 && properties.Width > loadProperties.MaxDecodePixelWidth)
                    {
                        bitmap.DecodePixelWidth = loadProperties.DpiAwareMaxDecodePixelWidth;
                    }

                    if (loadProperties.MaxDecodePixelHeight > 0 && properties.Height > loadProperties.MaxDecodePixelHeight)
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
                else if (loadProperties.Scaling == ImageLoadScaling.Custom)
                {
                    var settings = new ProcessImageSettings
                    {
                        Sharpen = false
                    };

                    settings.TrySetEncoderFormat(ImageMimeTypes.Bmp);

                    if (loadProperties.MaxDecodePixelWidth > 0 && properties.Width > loadProperties.MaxDecodePixelWidth)
                    {
                        settings.Width = loadProperties.DpiAwareMaxDecodePixelWidth;
                    }

                    if (loadProperties.MaxDecodePixelHeight > 0 && properties.Height > loadProperties.MaxDecodePixelHeight)
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

    public static long GetSizeInMemory(this BitmapImage image)
    {
        try
        {
            return Convert.ToInt64(image.PixelHeight * image.PixelWidth * 4);
        }
        catch (Exception e) when (!AppConfig.ThrowAllErrors)
        {
            logger.Error(e, "Failed to get image size from bitmap.");
            return 0;
        }
    }

    public static BitmapImage? TgaToBitmap(TGA tga)
    {
        try
        {
            using var tgaBitmap = tga.ToBitmap();
            using var memory = new MemoryStream();
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
        catch (Exception e)
        {
            logger.Error(e, $"Failed to create bitmap from TGA image.");
            return null;
        }
    }

    public static Bitmap ToBitmap(this BitmapImage bitmapImage)
    {
        using var outStream = new MemoryStream();
        var enc = new BmpBitmapEncoder();
        enc.Frames.Add(BitmapFrame.Create(bitmapImage));
        enc.Save(outStream);
        return new Bitmap(outStream);
    }

    public static BitmapImage? TgaToBitmap(string tgaPath)
    {
        return TgaToBitmap(new TGA(tgaPath));
    }

    public static BitmapImage? TgaToBitmap(byte[] tgaContent)
    {
        return TgaToBitmap(new TGA(tgaContent));
    }

    /// <summary>
    /// Converts a PNG image to a icon (ico) with all the sizes windows likes
    /// </summary>
    /// <param name="inputBitmap">The input bitmap</param>
    /// <param name="output">The output stream</param>
    /// <returns>Wether or not the icon was succesfully generated</returns>
    public static bool ConvertToIcon(Bitmap inputBitmap, Stream output)
    {
        if (inputBitmap == null)
            return false;

        int[] sizes = new int[] { 256, 48, 32, 16 };

        // Generate bitmaps for all the sizes and toss them in streams
        List<MemoryStream> imageStreams = new List<MemoryStream>();
        foreach (int size in sizes)
        {
            Bitmap newBitmap = ResizeImage(inputBitmap, size, size);
            if (newBitmap == null)
                return false;
            MemoryStream memoryStream = new MemoryStream();
            newBitmap.Save(memoryStream, ImageFormat.Png);
            imageStreams.Add(memoryStream);
        }

        BinaryWriter iconWriter = new BinaryWriter(output);
        if (output == null || iconWriter == null)
            return false;

        int offset = 0;

        // 0-1 reserved, 0
        iconWriter.Write((byte)0);
        iconWriter.Write((byte)0);

        // 2-3 image type, 1 = icon, 2 = cursor
        iconWriter.Write((short)1);

        // 4-5 number of images
        iconWriter.Write((short)sizes.Length);

        offset += 6 + (16 * sizes.Length);

        for (int i = 0; i < sizes.Length; i++)
        {
            // image entry 1
            // 0 image width
            iconWriter.Write((byte)sizes[i]);
            // 1 image height
            iconWriter.Write((byte)sizes[i]);

            // 2 number of colors
            iconWriter.Write((byte)0);

            // 3 reserved
            iconWriter.Write((byte)0);

            // 4-5 color planes
            iconWriter.Write((short)0);

            // 6-7 bits per pixel
            iconWriter.Write((short)32);

            // 8-11 size of image data
            iconWriter.Write((int)imageStreams[i].Length);

            // 12-15 offset of image data
            iconWriter.Write((int)offset);

            offset += (int)imageStreams[i].Length;
        }

        for (int i = 0; i < sizes.Length; i++)
        {
            // write image data
            // png data must contain the whole png data file
            iconWriter.Write(imageStreams[i].ToArray());
            imageStreams[i].Close();
        }

        iconWriter.Flush();

        return true;
    }

    /// <summary>
    /// Converts a PNG image to a icon (ico)
    /// </summary>
    /// <param name="input">The input stream</param>
    /// <param name="output">The output stream</param
    /// <returns>Wether or not the icon was succesfully generated</returns>
    public static bool ConvertToIcon(Stream input, Stream output)
    {
        var inputBitmap = (Bitmap)Bitmap.FromStream(input);
        return ConvertToIcon(inputBitmap, output);
    }

    /// <summary>
    /// Converts a PNG image to a icon (ico)
    /// </summary>
    /// <param name="inputPath">The input path</param>
    /// <param name="outputPath">The output path</param>
    /// <returns>Wether or not the icon was succesfully generated</returns>
    public static bool ConvertToIcon(string inputPath, string outputPath)
    {
        using var inputStream = new FileStream(inputPath, FileMode.Open);
        using var outputStream = new FileStream(outputPath, FileMode.Create);
        return ConvertToIcon(inputStream, outputStream);
    }

    /// <summary>
    /// Converts an image to a icon (ico)
    /// </summary>
    /// <param name="inputImage">The input image</param>
    /// <param name="outputPath">The output path</param>
    /// <returns>Wether or not the icon was succesfully generated</returns>
    public static bool ConvertToIcon(Image inputImage, string outputPath)
    {
        using var outputStream = new FileStream(outputPath, FileMode.Create);
        return ConvertToIcon(new Bitmap(inputImage), outputStream);
    }

    /// <summary>
    /// Resize the image to the specified width and height.
    /// Found on stackoverflow: https://stackoverflow.com/questions/1922040/resize-an-image-c-sharp
    /// </summary>
    /// <param name="image">The image to resize.</param>
    /// <param name="width">The width to resize to.</param>
    /// <param name="height">The height to resize to.</param>
    /// <returns>The resized image.</returns>
    public static Bitmap ResizeImage(Image image, int width, int height)
    {
        var destRect = new Rectangle(0, 0, width, height);
        var destImage = new Bitmap(width, height);

        destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

        using var graphics = Graphics.FromImage(destImage);
        graphics.CompositingMode = CompositingMode.SourceCopy;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        using var wrapMode = new ImageAttributes();
        wrapMode.SetWrapMode(WrapMode.TileFlipXY);
        graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);

        return destImage;
    }
}
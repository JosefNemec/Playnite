using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing.Imaging
{
    /// <summary>
    /// Adapted from this gist: https://gist.github.com/darkfall/1656050
    /// Provides helper methods for imaging
    /// </summary>
    public static partial class BitmapExtensions
    {
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
            Bitmap inputBitmap = (Bitmap)Bitmap.FromStream(input);
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
            using (FileStream inputStream = new FileStream(inputPath, FileMode.Open))
            using (FileStream outputStream = new FileStream(outputPath, FileMode.OpenOrCreate))
            {
                return ConvertToIcon(inputStream, outputStream);
            }
        }

        /// <summary>
        /// Converts an image to a icon (ico)
        /// </summary>
        /// <param name="inputImage">The input image</param>
        /// <param name="outputPath">The output path</param>
        /// <returns>Wether or not the icon was succesfully generated</returns>
        public static bool ConvertToIcon(Image inputImage, string outputPath)
        {
            using (FileStream outputStream = new FileStream(outputPath, FileMode.OpenOrCreate))
            {
                return ConvertToIcon(new Bitmap(inputImage), outputStream);
            }
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

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}

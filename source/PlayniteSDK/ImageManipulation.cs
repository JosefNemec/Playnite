using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Playnite.SDK
{
    /// <summary>
    /// Utility functions for Image Manipulation
    /// </summary>
    public static class ImageManipulation
    {
        /// <summary>
        /// Resize an image.
        /// </summary>
        /// <param name="image">Image to resize</param>
        /// <param name="maxWidth">Max width the image should have</param>
        /// <param name="maxHeight">Max height the image should have</param>
        /// <returns></returns>
        public static byte[] ResizeImage(Image image, int maxWidth, int maxHeight)
        {
            if(image == null)
                throw new ArgumentNullException(nameof(image));

            var desiredWidth = image.Width > maxWidth ? maxWidth : image.Width;
            var desiredHeight = image.Height > maxHeight ? maxHeight : image.Height;

            using (var resizedImage = image.GetThumbnailImage(desiredWidth, desiredHeight, null, IntPtr.Zero))
            using (var stream = new MemoryStream())
            {
                resizedImage.Save(stream, ImageFormat.Png);
                stream.Position = 0;
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Downscale an Image percentage based
        /// </summary>
        /// <param name="image">Image to downscale</param>
        /// <param name="factor">Downscale percentage, must be between 0 and 1</param>
        /// <returns></returns>
        public static byte[] DownscaleImage(Image image, double factor = 0.5)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            if(factor >= 1 || factor <= double.Epsilon)
                throw new ArgumentException($"Factor can only be between 0 and 1! {factor}", nameof(factor));

            return ResizeImage(image, (int)(image.Width * factor), (int)(image.Height * factor));
        }
    }
}

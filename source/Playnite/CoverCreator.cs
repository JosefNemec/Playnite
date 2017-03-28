using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class CoverCreator
    {
        public static SizeF ScaleRect(SizeF dest, SizeF src, bool keepWidth, bool keepHeight)
        {
            SizeF destRect = new SizeF();

            float sourceAspect = src.Width / src.Height;
            float destAspect = dest.Width / dest.Height;

            if (sourceAspect > destAspect)
            {
                // wider than high keep the width and scale the height
                destRect.Width = dest.Width;
                destRect.Height = dest.Width / sourceAspect;

                if (keepHeight)
                {
                    float resizePerc = dest.Height / destRect.Height;
                    destRect.Width = dest.Width * resizePerc;
                    destRect.Height = dest.Height;
                }
            }
            else
            {
                // higher than wide – keep the height and scale the width
                destRect.Height = dest.Height;
                destRect.Width = dest.Height * sourceAspect;

                if (keepWidth)
                {
                    float resizePerc = dest.Width / destRect.Width;
                    destRect.Width = dest.Width;
                    destRect.Height = dest.Height * resizePerc;
                }

            }

            return destRect;
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }

        public static void CreateCover(Stream backgroundImage, Stream coverImage, Stream stream)
        {
            var background = Image.FromStream(backgroundImage);
            var cover = Image.FromStream(coverImage);

            using (var canvas = Graphics.FromImage(background))
            {
                var scaled = ScaleRect(new SizeF(231, 50), new SizeF(cover.Width, cover.Height), true, false);

                canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                canvas.DrawImage(cover, 0, 0, 231, scaled.Height);
                canvas.Save();
            }
            
            var jpgEncoder = GetEncoder(ImageFormat.Jpeg);
            var myEncoder = System.Drawing.Imaging.Encoder.Quality;
            var myEncoderParameters = new EncoderParameters(1);
            var myEncoderParameter = new EncoderParameter(myEncoder, 100L);
            myEncoderParameters.Param[0] = myEncoderParameter;
            background.Save(stream, jpgEncoder, myEncoderParameters);         
        }

    }
}

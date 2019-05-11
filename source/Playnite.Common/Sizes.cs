using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class AspectRatioTypeConverter : TypeConverter
    {
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value is string ratio)
            {
                var regex = Regex.Match(ratio, @"(\d+):(\d+)");
                if (regex.Success)
                {
                    return new AspectRatio(
                        Convert.ToInt32(regex.Groups[1].Value),
                        Convert.ToInt32(regex.Groups[2].Value));
                }
            }

            throw new NotSupportedException($"Cannot convert {value} to AspectRatio.");
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string);
        }
    }

    [TypeConverter(typeof(AspectRatioTypeConverter))]
    public class AspectRatio : IEquatable<AspectRatio>
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public AspectRatio()
        {
        }

        public AspectRatio(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public override bool Equals(object obj)
        {
            if (obj is AspectRatio other)
            {
                return other.Height == Height && other.Width == Width;
            }
            else
            {
                return false;
            }
        }

        public bool Equals(AspectRatio other)
        {
            return other != null &&
                   Width == other.Width &&
                   Height == other.Height;
        }

        public override int GetHashCode()
        {
            var hashCode = 859600377;
            hashCode = hashCode * -1521134295 + Width.GetHashCode();
            hashCode = hashCode * -1521134295 + Height.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return $"{Width}:{Height}";
        }

        public double GetWidth(double height)
        {
            return ((double)Width / Height) * height;
        }

        public double GetHeight(double width)
        {
            return ((double)Height / Width) * width;
        }
    }

    public class Sizes
    {
        public static AspectRatio GetAspectRatio(Rectangle rect)
        {
            return GetAspectRatio(rect.Width, rect.Height);
        }

        public static AspectRatio GetAspectRatio(int width, int height)
        {
            var gcd = GetGreatestCommonDivisor(width, height);
            return new AspectRatio(width / gcd, height / gcd);
        }

        static int GetGreatestCommonDivisor(int a, int b)
        {
            return b == 0 ? a : GetGreatestCommonDivisor(b, a % b);
        }
    }
}

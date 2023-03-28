using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Playnite;

public class AspectRatioTypeConverter : TypeConverter
{
    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
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
            else
            {
                // For cases where this is called from designer element preview via binding expression
                return new AspectRatio();
            }
        }

        throw new NotSupportedException($"Cannot convert {value} to AspectRatio.");
    }

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
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

    public override bool Equals(object? obj) => Equals(obj as AspectRatio);

    public bool Equals(AspectRatio? other)
    {
        if (other is null)
        {
            return false;
        }

        return Width == other.Width && Height == other.Height;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(AspectRatio obj1, AspectRatio obj2)
    {
        return obj1?.Equals(obj2) == true;
    }

    public static bool operator !=(AspectRatio obj1, AspectRatio obj2)
    {
        return obj1?.Equals(obj2) == false;
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

    public static double GetMegapixelsFromRes(int width, int height)
    {
        return Math.Round((double)(width * height) / 1000000, 3);
    }

    public static double GetMegapixelsFromRes(ImageProperties props)
    {
        return GetMegapixelsFromRes(props.Width, props.Height);
    }
}

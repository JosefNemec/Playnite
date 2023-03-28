using System.Windows.Media;

namespace System.Windows.Controls;

public static class ColorExtensions
{
    public static string ToHtml(this Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}

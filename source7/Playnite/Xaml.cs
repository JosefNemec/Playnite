using System.IO;
using System.Windows.Markup;

namespace Playnite;

public class Xaml
{
    public static object FromFile(string path)
    {
        using var stream = new StreamReader(path);
        return XamlReader.Load(stream.BaseStream);
    }

    public static T FromFile<T>(string path)
    {
        using var stream = new StreamReader(path);
        return (T)XamlReader.Load(stream.BaseStream);
    }

    public static T FromString<T>(string xaml)
    {
        return (T)XamlReader.Parse(xaml);
    }
}

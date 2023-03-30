using System.IO;
using System.Reflection;
using Vanara.PInvoke;

namespace Playnite;

public class Resources
{
    public static string? ReadFileFromResource(string resource)
    {
        using var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resource);
        if (stream is null)
        {
            return null;
        }

        var tr = new StreamReader(stream);
        return tr.ReadToEnd();
    }

    public static string GetIndirectResourceString(string fullName, string packageName, string resource)
    {
        var resUri = new Uri(resource);
        var resourceString = string.Empty;
        if (resource.StartsWith("ms-resource://", StringComparison.Ordinal))
        {
            resourceString = $"@{{{fullName}? {resource}}}";
        }
        else if (resource.Contains('/', StringComparison.Ordinal))
        {
            resourceString = $"@{{{fullName}? ms-resource://{packageName}/{resource.Replace("ms-resource:", "", StringComparison.Ordinal).Trim('/')}}}";
        }
        else
        {
            resourceString = $"@{{{fullName}? ms-resource://{packageName}/resources/{resUri.Segments.Last()}}}";
        }

        var sb = new StringBuilder(8192);
        var result = ShlwApi.SHLoadIndirectString(resourceString, sb, (uint)sb.Capacity);
        if (result == 0)
        {
            return sb.ToString();
        }

        resourceString = $"@{{{fullName}? ms-resource://{packageName}/{resUri.Segments.Last()}}}";
        result = ShlwApi.SHLoadIndirectString(resourceString, sb, (uint)sb.Capacity);
        if (result == 0)
        {
            return sb.ToString();
        }

        return string.Empty;
    }
}

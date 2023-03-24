using System.Text.RegularExpressions;

namespace Playnite;

public class IniSection
{
    public string Name { get; }
    public List<IniItem> Items { get; } = new List<IniItem>();

    public string? this[string itemName]
    {
        get
        {
            return Items.FirstOrDefault(a => a.Name == itemName)?.Value;
        }
    }

    public IniSection(string name)
    {
        if (string.IsNullOrEmpty(name)) new ArgumentNullException(nameof(name));
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }
}

public class IniItem
{
    public string Name { get; }
    public string Value { get; }

    public IniItem(string name, string value)
    {
        if (string.IsNullOrEmpty(name)) new ArgumentNullException(nameof(name));
        Name = name;
        Value = value;
    }

    public override string ToString()
    {
        return Name;
    }
}

public class IniData
{
    public List<IniSection> Sections { get; } = new List<IniSection>();

    public IniSection? this[string sectionName]
    {
        get
        {
            return Sections.FirstOrDefault(a => a.Name == sectionName);
        }
    }
}

public class IniFile
{
    public static IniData Parse(string[] iniString)
    {
        ArgumentNullException.ThrowIfNull(iniString);
        var data = new IniData();
        if (iniString.Length == 0)
        {
            return data;
        }

        IniSection? curSection = null;
        foreach (var line in iniString)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            // Comment
            if (line.TrimStart().StartsWith(";", StringComparison.Ordinal))
            {
                continue;
            }

            // Section
            var sectionMatch = Regex.Match(line.Trim(), @"^\[(.+)\]$");
            if (sectionMatch.Success)
            {
                curSection = new IniSection(sectionMatch.Groups[1].Value);
                data.Sections.Add(curSection);
                continue;
            }

            // Section item
            var valueMatch = Regex.Match(line.Trim(), @"^(.+)=(.*)$");
            if (valueMatch.Success)
            {
                curSection?.Items.Add(new IniItem(valueMatch.Groups[1].Value, valueMatch.Groups[2].Value));
            }
        }

        return data;
    }
}

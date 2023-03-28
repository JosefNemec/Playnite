namespace Playnite;

public class ItemsSource
{
    public class EnumItem
    {
        public string Name { get; set; }
        public Enum Value { get; set; }

        public EnumItem(string name, Enum value)
        {
            Name = name;
            Value = value;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public static IEnumerable<EnumItem> GetEnumSources(Type enumType)
    {
        foreach (Enum type in Enum.GetValues(enumType))
        {
            yield return new EnumItem(type.GetDescription(), type);
        }
    }
}

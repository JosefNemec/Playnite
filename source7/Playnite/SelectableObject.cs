using CommunityToolkit.Mvvm.ComponentModel;

namespace Playnite;

public partial class SelectableObject<TItem> : ObservableObject
{
    public string Name { get; }
    public TItem? Item { get; }

    [ObservableProperty] private bool? selected = false;

    public SelectableObject(TItem? item)
    {
        Item = item;
        Name = item?.ToString() ?? "NamedObject: no name";
    }

    public SelectableObject(TItem? item, string name)
    {
        Item = item;
        Name = name;
    }

    public SelectableObject(TItem? item, bool selected)
    {
        this.selected = selected;
        Item = item;
        Name = item?.ToString() ?? "NamedObject: no name";
    }

    public SelectableObject(TItem? item, string name, bool selected)
    {
        this.selected = selected;
        Item = item;
        Name = name;
    }

    public override string ToString()
    {
        return Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is SelectableObject<TItem> named)
        {
            return Equals(named);
        }

        return false;
    }

    public bool Equals(SelectableObject<TItem> p)
    {
        if (p is null)
        {
            return false;
        }

        return Item?.Equals(p.Item) == true;
    }

    public override int GetHashCode()
    {
        return Item?.GetHashCode() ?? 0;
    }

    public static bool operator ==(SelectableObject<TItem> l, SelectableObject<TItem> r)
    {
        if ((l is null && r is not null) || (l is not null && r is null))
        {
            return false;
        }

        if (l is null && r is null)
        {
            return true;
        }

        return l!.Item?.Equals(r!.Item) == true;
    }

    public static bool operator !=(SelectableObject<TItem> l, SelectableObject<TItem> r)
    {
        return !(l == r);
    }
}
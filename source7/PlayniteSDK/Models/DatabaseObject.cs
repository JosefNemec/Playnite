using CommunityToolkit.Mvvm.ComponentModel;

namespace Playnite;

/// <summary>
/// Represents base database object item.
/// </summary>
public partial class DatabaseObject : ObservableObject
{
    /// <summary>
    /// Gets or sets identifier of database object.
    /// </summary>
    public Guid Id { get; set; }

    [ObservableProperty] private string? name;

    /// <summary>
    /// Creates new instance of <see cref="DatabaseObject"/>.
    /// </summary>
    public DatabaseObject()
    {
        Id = Guid.NewGuid();
    }

    /// <inheritdoc/>
    public override string? ToString()
    {
        return Name ?? string.Empty;
    }

    /// <summary>
    /// Copies differential properties to target object intance.
    /// </summary>
    /// <param name="target">Target object instance to receive new data.</param>
    public virtual void CopyDiffTo(object target)
    {
        ArgumentNullException.ThrowIfNull(target);

        if (ReferenceEquals(this, target))
        {
            throw new Exception("Cannot copy data to itself.");
        }

        if (target is DatabaseObject dbo)
        {
            if (!string.Equals(Name, dbo.Name, StringComparison.Ordinal))
            {
                dbo.Name = Name;
            }
        }
        else
        {
            throw new Exception($"Target object has to be of type {GetType().Name}");
        }
    }
}

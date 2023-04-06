namespace Playnite;

/// <summary>
/// Represents arguments for Playnite URI execution event.
/// </summary>
public class PlayniteUriEventArgs
{
    /// <summary>
    /// Gets or sets url arguments.
    /// </summary>
    public string[]? Arguments { get; set; }
}

/// <summary>
/// Describes API for handling playnite:// URI.
/// </summary>
public interface IUriHandlerAPI
{
    /// <summary>
    /// Registers new URI source.
    /// </summary>
    /// <param name="source">Source name.</param>
    /// <param name="handler">Method to be executed.</param>
    void RegisterSource(string source, Action<PlayniteUriEventArgs> handler);

    /// <summary>
    /// Removes registered source.
    /// </summary>
    /// <param name="source">Source name.</param>
    void RemoveSource(string source);
}

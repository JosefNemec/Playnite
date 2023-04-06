using System.Web;

namespace Playnite;

public class UriCommands
{
    public const string StartGame = "start";
    public const string CreateDiag = "creatediag";
    public const string ShowGame = "showgame";
    public const string InstallAddon = "installaddon";
    public const string Search = "search";
}

public class PlayniteUriHandler : IUriHandlerAPI
{
    private static readonly ILogger logger = LogManager.GetLogger();
    private static readonly char[] uriSplitter = new char[] { '/' };

    internal readonly Dictionary<string, Action<PlayniteUriEventArgs>> Handlers = new (StringComparer.OrdinalIgnoreCase);

    public void RegisterSource(string source, Action<PlayniteUriEventArgs> handler)
    {
        ArgumentException.ThrowIfNullOrEmpty(source);

        if (source.Equals("playnite", StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception($"\"playnite\" source is reserved source.");
        }

        if (Handlers.TryGetValue(source, out _))
        {
            throw new Exception($"Handler {source} is already registered.");
        }

        Handlers.Add(source, handler);
    }

    public void RemoveSource(string source)
    {
        if (Handlers.TryGetValue(source, out var handler))
        {
            Handlers.Remove(source);
        }
    }

    public static (string source, string[]? arguments) ParseUri(string uri)
    {
        var split = uri.Split(uriSplitter, StringSplitOptions.RemoveEmptyEntries);
        if (split.Length < 2)
        {
            throw new Exception("playnite:// uri missing parameters.");
        }

        var source = split[1];
        string[]? arguments = null;
        if (split.Length > 2)
        {
            arguments = split.Skip(2).Select(a => HttpUtility.UrlDecode(a)).ToArray();
        }

        return (source, arguments);
    }

    internal void ProcessUri(string uri)
    {
        logger.Info($"Processing Playnite URI {uri}");
        try
        {
            var (source, arguments) = ParseUri(uri);
            if (Handlers.TryGetValue(source, out var handler))
            {
                handler.Invoke(new PlayniteUriEventArgs { Arguments = arguments });
            }
            else
            {
                logger.Error($"URI handler {source} is not registered.");
                return;
            }
        }
        catch (Exception e) when (!AppConfig.ThrowAllErrors)
        {
            logger.Error(e, "URI handler failed.");
        }
    }
}

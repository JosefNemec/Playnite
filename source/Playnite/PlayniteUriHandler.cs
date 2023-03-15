using Flurl;
using Playnite.SDK;
using Playnite.SDK.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Playnite
{
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

        internal readonly Dictionary<string, Action<PlayniteUriEventArgs>> Handlers =
            new Dictionary<string, Action<PlayniteUriEventArgs>>();

        public void RegisterSource(string source, Action<PlayniteUriEventArgs> handler)
        {
            if (source.IsNullOrEmpty())
            {
                throw new ArgumentNullException("Source cannot be null");
            }

            if (source.Equals("playnite", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception($"\"playnite\" source is reserved source.");
            }

            if (Handlers.Keys.FirstOrDefault(a => a.Equals(source, StringComparison.OrdinalIgnoreCase)) != null)
            {
                throw new Exception($"Handler {source} is already registered.");
            }

            Handlers.Add(source, handler);
        }

        public void RemoveSource(string source)
        {
            var handler = Handlers.FirstOrDefault(a => a.Key.Equals(source, StringComparison.OrdinalIgnoreCase));
            if (handler.Value != null)
            {
                Handlers.Remove(handler.Key);
            }
        }

        public static (string source, string[] arguments) ParseUri(string uri)
        {
            var split = uri.Split(uriSplitter, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
            {
                throw new Exception("playnite:// uri missing parameters.");
            }

            var source = split[1];
            var arguments = new string[0];
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
                var handler = Handlers.FirstOrDefault(a => a.Key.Equals(source, StringComparison.OrdinalIgnoreCase));
                if (handler.Value == null)
                {
                    logger.Error($"URI handler {source} is not registered.");
                    return;
                }

                handler.Value.Invoke(new PlayniteUriEventArgs { Arguments = arguments });
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "URI handler failed.");
            }
        }
    }
}

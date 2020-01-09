using Flurl;
using Playnite.SDK;
using Playnite.SDK.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class PlayniteUriHandler : IUriHandlerAPI
    {
        private static readonly ILogger logger = LogManager.GetLogger();

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

        internal void ProcessUri(string uri)
        {
            logger.Info($"Processing Playnite URI {uri}");
            var url = new Uri(uri);
            var source = url.Host;
            var args = url.LocalPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            var handler = Handlers.FirstOrDefault(a => a.Key.Equals(source, StringComparison.OrdinalIgnoreCase));
            if (handler.Value == null)
            {
                logger.Error($"URI handler {source} is not registered.");
                return;
            }

            try
            {
                handler.Value.Invoke(new PlayniteUriEventArgs { Arguments = args });
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "URI handler failed.");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents default resource provider.
    /// </summary>
    public class ResourceProvider : IResourceProvider
    {
        /// <summary>
        /// Creates new instance of <see cref="ResourceProvider"/>.
        /// </summary>
        public ResourceProvider()
        {
        }

        string IResourceProvider.GetString(string key)
        {
            return GetString(key);
        }

        object IResourceProvider.GetResource(string key)
        {
            return GetResource(key);
        }

        /// <summary>
        /// Gets string resource.
        /// </summary>
        /// <param name="key">String resource key.</param>
        /// <returns>String resource.</returns>
        public static string GetString(string key)
        {
            var resource = Application.Current?.TryFindResource(key);
            return resource == null ? $"<!{key}!>" : resource as string;
        }

        /// <summary>
        /// Gets application resource.
        /// </summary>
        /// <param name="key">Resource key.</param>
        /// <returns>Application resource.</returns>
        public static object GetResource(string key)
        {
            return Application.Current?.TryFindResource(key);
        }
    }
}

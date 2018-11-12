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
    public class DefaultResourceProvider : IResourceProvider
    {
        /// <summary>
        /// Creates new instance of <see cref="DefaultResourceProvider"/>.
        /// </summary>
        public DefaultResourceProvider()
        {
        }

        string IResourceProvider.FindString(string key)
        {
            return FindString(key);
        }

        /// <summary>
        /// Returns string resource.
        /// </summary>
        /// <param name="key">Name of the string resource.</param>
        /// <returns>String resource.</returns>
        public static string FindString(string key)
        {
            var resource = Application.Current.TryFindResource(key);
            return resource == null ? $"<!{key}!>" : resource as string;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes application resource provider.
    /// </summary>
    public interface IResourceProvider
    {
        /// <summary>
        /// Gets string resource.
        /// </summary>
        /// <param name="key">Resource key.</param>
        /// <returns>String resource.</returns>
        string GetString(string key);

        /// <summary>
        /// Gets application resource.
        /// </summary>
        /// <param name="key">Resource key.</param>
        /// <returns>Application resource.</returns>
        object GetResource(string key);
    }

    /// <summary>
    /// Represents default resource provider.
    /// </summary>
    public class ResourceProvider : IResourceProvider
    {
        private static IResourceProvider staticProvider;

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
            if (staticProvider != null)
            {
                return staticProvider.GetString(key);
            }
            else
            {
                var resource = Application.Current?.TryFindResource(key);
                return resource == null ? $"<!{key}!>" : resource as string;
            }
        }

        /// <summary>
        /// Gets application resource.
        /// </summary>
        /// <param name="key">Resource key.</param>
        /// <returns>Application resource.</returns>
        public static object GetResource(string key)
        {
            if (staticProvider != null)
            {
                return staticProvider.GetResource(key);
            }
            else
            {
                return Application.Current?.TryFindResource(key);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetResource<T>(string key)
        {
            return (T)Application.Current?.TryFindResource(key);
        }

        internal static void SetGlobalProvider(IResourceProvider provider)
        {
            staticProvider = provider;
        }
    }
}

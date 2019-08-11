using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}

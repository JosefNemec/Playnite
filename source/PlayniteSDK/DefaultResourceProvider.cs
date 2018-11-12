using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.SDK
{
    public class DefaultResourceProvider : IResourceProvider
    {
        public DefaultResourceProvider()
        {
        }

        string IResourceProvider.FindString(string key)
        {
            return FindString(key);
        }

        public static string FindString(string key)
        {
            var resource = Application.Current.TryFindResource(key);
            return resource == null ? $"<!{key}!>" : resource as string;
        }
    }
}

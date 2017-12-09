using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI
{
    public interface IResourceProvider
    {
        string FindString(string key);
    }

    public class ResourceProvider : IResourceProvider
    {
        public string FindString(string key)
        {
            var resource = Application.Current.TryFindResource(key);
            return resource == null ? $"<!{key}!>" : resource as string;
        }
    }
}

using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI
{
    public class ResourceProvider : IResourceProvider
    {
        private static ResourceProvider instance;
        public static ResourceProvider Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ResourceProvider();
                }

                return instance;
            }
        }

        public string FindString(string key)
        {
            var resource = Application.Current.TryFindResource(key);
            return resource == null ? $"<!{key}!>" : resource as string;
        }
    }
}

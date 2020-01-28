using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.Tests
{
    public class TestResourceProvider : IResourceProvider
    {
        private readonly ResourceDictionary engStringResource;
        public TestResourceProvider()
        {
            var engSource = Path.Combine(PlaynitePaths.LocalizationsPath, PlaynitePaths.EngLocSourceFileName);
            engStringResource = Xaml.FromFile<ResourceDictionary>(engSource);
        }

        public object GetResource(string key)
        {
            return null;
        }

        public string GetString(string key)
        {
            return engStringResource[key]?.ToString() ?? key;
        }
    }
}

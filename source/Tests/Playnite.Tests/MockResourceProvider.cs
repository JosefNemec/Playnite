using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    public class MockResourceProvider : IResourceProvider
    {
        public object GetResource(string key)
        {
            return null;
        }

        public string GetString(string key)
        {
            return string.Empty;
        }
    }
}

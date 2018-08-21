using Playnite.SDK;
using PlayniteUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUITests
{
    public class MockResourceProvider : IResourceProvider
    {
        public string FindString(string key)
        {
            return string.Empty;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.IO;
using Playnite;
using Playnite.Tests;
using System.Web;
using XboxLibrary.Models;
using Playnite.Common;
using System.Net.Http;
using Playnite.SDK.Models;
using XboxLibrary.Services;

namespace XboxLibrary.Tests
{
    [TestFixture]
    public class HumbleLibraryTests
    {
        public static XboxLibrary CreateLibrary()
        {
            return new XboxLibrary(PlayniteTests.GetTestingApi().Object);
        }
    }
}

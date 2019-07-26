using NUnit.Framework;
using Playnite;
using Playnite.Controllers;
using Playnite.Database;
using Playnite.DesktopApp.ViewModels;
using Playnite.Plugins;
using Playnite.SDK.Models;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.Tests.ViewModels
{
    [TestFixture]
    public class MainViewModelTests
    {
        [Test]
        public void InitializeCommandsTest()
        {
            var database = new GameDatabase();
            var controllers = new GameControllerFactory();
            var model = new DesktopAppViewModel();
            var props = typeof(DesktopAppViewModel).GetProperties();
            foreach (var prop in props)
            {
                if (prop.Name.EndsWith("Command"))
                {
                    var cmd = prop.GetValue(model, null);
                    if (cmd == null)
                    {
                        Assert.Fail($"{prop.Name} is not defined.");
                    }
                }
            }
        }
    }
}

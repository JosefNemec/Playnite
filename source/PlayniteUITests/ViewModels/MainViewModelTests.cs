using NUnit.Framework;
using Playnite;
using Playnite.Controllers;
using Playnite.Database;
using Playnite.Plugins;
using Playnite.SDK.Models;
using Playnite.Settings;
using PlayniteUI;
using PlayniteUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUITests.ViewModels
{
    [TestFixture]
    public class MainViewModelTests
    {
        [Test]
        public void InitializeCommandsTest()
        {
            var database = new GameDatabase();
            var controllers = new GameControllerFactory();
            var model = new MainViewModel(
                database,
                null,
                null,
                null,
                new PlayniteSettings(),
                new GamesEditor(
                    new GameDatabase(),
                    controllers,
                    new PlayniteSettings(),
                    null,
                    null),
                null,
                new ExtensionFactory(database, controllers));

            var props = typeof(MainViewModel).GetProperties();
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

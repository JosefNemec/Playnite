using NUnit.Framework;
using Playnite.Common;
using Playnite.Database;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Database
{
    [TestFixture]
    public class EmulatorsCollectionTests
    {
        [Test]
        public void UsageRemovalTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();

                var emulator = new Emulator("test")
                {
                    Profiles = new System.Collections.ObjectModel.ObservableCollection<EmulatorProfile>()
                    {
                        new EmulatorProfile() { Name = "test profile" }
                    }
                };

                db.Emulators.Add(emulator);
                var game = new Game("test")
                {
                    PlayAction = new GameAction()
                    {
                        Type = GameActionType.Emulator,
                        EmulatorId = emulator.Id,
                        EmulatorProfileId = emulator.Profiles[0].Id
                    }
                };

                db.Games.Add(game);
                db.Emulators.Remove(emulator);
                Assert.AreEqual(Guid.Empty, game.PlayAction.EmulatorId);
                Assert.AreEqual(Guid.Empty, game.PlayAction.EmulatorProfileId);
            }
        }
    }
}

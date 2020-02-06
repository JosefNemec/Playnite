using NUnit.Framework;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Models
{
    [TestFixture]
    public class GameActionTests
    {
        [Test]
        public void EqualityTest()
        {
            var obj1 = new GameAction()
            {
                AdditionalArguments = "addargs",
                Arguments = "args",
                EmulatorId = new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
                EmulatorProfileId = new Guid(2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2),
                IsHandledByPlugin = false,
                Name = "name",
                OverrideDefaultArgs = false,
                Path = "path",
                SuppressNotifications = false,
                Type = GameActionType.Emulator,
                WorkingDir = "workdir"
            };

            var obj2 = new GameAction()
            {
                AdditionalArguments = "addargs",
                Arguments = "args",
                EmulatorId = new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
                EmulatorProfileId = new Guid(2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2),
                IsHandledByPlugin = false,
                Name = "name",
                OverrideDefaultArgs = false,
                Path = "path",
                SuppressNotifications = false,
                Type = GameActionType.Emulator,
                WorkingDir = "workdir"
            };

            var obj3 = new GameAction()
            {
                AdditionalArguments = "addargs2",
                Arguments = "args2",
                EmulatorId = new Guid(2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2),
                EmulatorProfileId = new Guid(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1),
                IsHandledByPlugin = true,
                Name = "name2",
                OverrideDefaultArgs = true,
                Path = "path2",
                SuppressNotifications = true,
                Type = GameActionType.File,
                WorkingDir = "workdir2"
            };

            Assert.IsTrue(obj1.Equals(obj2));
            Assert.IsFalse(obj1.Equals(obj3));

            Assert.IsTrue(
                new List<GameAction> { new GameAction() { Name = "1" }, new GameAction() { Name = "2" } }.IsListEqualExact(
                new List<GameAction> { new GameAction() { Name = "1" }, new GameAction() { Name = "2" } }));

            Assert.IsFalse(
                new List<GameAction> { new GameAction() { Name = "1" }, new GameAction() { Name = "2" } }.IsListEqualExact(
                new List<GameAction> { new GameAction() { Name = "2" }, new GameAction() { Name = "1" } }));
        }
    }
}

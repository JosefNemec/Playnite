using NUnit.Framework;
using Playnite.Common.System;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.Database
{
    [TestFixture]
    public class ItemCollectionTests
    {
        [Test]
        public void AddTest()
        {
            var newGame = new Game("TestGame");
            using (var temp = TempDirectory.Create())
            {                
                var col = new ItemCollection<Game>(temp.TempPath);
                col.Add(newGame);                
                var files = Directory.GetFiles(temp.TempPath);
                Assert.AreEqual(1, col.Count);
                Assert.AreEqual(1, files.Count());
                Assert.AreEqual(newGame.Id.ToString() + ".json", Path.GetFileName(files[0]));
            }
        }

        [Test]
        public void EventsInvokeCountNonBufferedTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var itemUpdates = 0;
                var colUpdates = 0;
                var col = new ItemCollection<DatabaseObject>(temp.TempPath);
                col.ItemUpdated += (e, args) => itemUpdates++;
                col.ItemCollectionChanged += (e, args) => colUpdates++;

                var item = new DatabaseObject();
                col.Add(item);
                col.Update(item);
                col.Remove(item);
                Assert.AreEqual(1, itemUpdates);
                Assert.AreEqual(2, colUpdates);
            }
        }

        [Test]
        public void EventsInvokeCountBufferedTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var itemUpdates = 0;
                var colUpdates = 0;
                var col = new ItemCollection<DatabaseObject>(temp.TempPath);
                col.ItemUpdated += (e, args) => itemUpdates++;
                col.ItemCollectionChanged += (e, args) => colUpdates++;

                var item = new DatabaseObject();
                col.BeginBufferUpdate();
                col.Add(item);
                col.Update(item);
                col.Remove(item);
                Assert.AreEqual(0, itemUpdates);
                Assert.AreEqual(0, colUpdates);
                col.EndBufferUpdate();                
                Assert.AreEqual(1, itemUpdates);
                Assert.AreEqual(1, colUpdates);
            }            
        }

        [Test]
        public void EventsArgsNonBufferedTest()
        {
            using (var temp = TempDirectory.Create())
            {
                ItemCollectionChangedEventArgs<DatabaseObject> itemColArgs = null;
                ItemUpdatedEventArgs<DatabaseObject> itemUpdateArgs = null;
                var col = new ItemCollection<DatabaseObject>(temp.TempPath);
                col.ItemUpdated += (e, args) => itemUpdateArgs = args;
                col.ItemCollectionChanged += (e, args) => itemColArgs = args;
                var item = new DatabaseObject();
                
                col.Add(item);
                Assert.AreEqual(1, itemColArgs.AddedItems.Count);
                Assert.AreEqual(item, itemColArgs.AddedItems[0]);
                Assert.AreEqual(0, itemColArgs.RemovedItems.Count);

                col.Update(item);
                Assert.AreEqual(1, itemUpdateArgs.UpdatedItems.Count);
                Assert.AreEqual(item, itemUpdateArgs.UpdatedItems[0].NewData);
                Assert.AreNotEqual(item, itemUpdateArgs.UpdatedItems[0].OldData);

                col.Remove(item);
                Assert.AreEqual(0, itemColArgs.AddedItems.Count);
                Assert.AreEqual(1, itemColArgs.RemovedItems.Count);
                Assert.AreEqual(item, itemColArgs.RemovedItems[0]);
            }
        }

        [Test]
        public void EventsArgsBufferedTest()
        {
            using (var temp = TempDirectory.Create())
            {
                ItemCollectionChangedEventArgs<DatabaseObject> itemColArgs = null;
                ItemUpdatedEventArgs<DatabaseObject> itemUpdateArgs = null;
                var col = new ItemCollection<DatabaseObject>(temp.TempPath);
                col.ItemUpdated += (e, args) => itemUpdateArgs = args;
                col.ItemCollectionChanged += (e, args) => itemColArgs = args;
                var item = new DatabaseObject();

                col.BeginBufferUpdate();
                col.Add(item);
                col.Update(item);
                col.Update(item);
                col.Remove(item);
                Assert.IsNull(itemColArgs);
                Assert.IsNull(itemUpdateArgs);

                col.EndBufferUpdate();
                Assert.AreEqual(1, itemColArgs.AddedItems.Count);
                Assert.AreEqual(1, itemColArgs.RemovedItems.Count);
                Assert.AreEqual(2, itemUpdateArgs.UpdatedItems.Count);
            }
        }
    }
}

using NUnit.Framework;
using Playnite.Common;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.Database
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
                var item = new DatabaseObject() { Name = "Original" };
                
                col.Add(item);
                Assert.AreEqual(1, itemColArgs.AddedItems.Count);
                Assert.AreEqual(item, itemColArgs.AddedItems[0]);
                Assert.AreEqual(0, itemColArgs.RemovedItems.Count);

                item.Name = "New";
                col.Update(item);
                Assert.AreEqual(1, itemUpdateArgs.UpdatedItems.Count);
                Assert.AreEqual("Original", itemUpdateArgs.UpdatedItems[0].OldData.Name);
                Assert.AreEqual("New", itemUpdateArgs.UpdatedItems[0].NewData.Name);

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
                col.Remove(item);
                Assert.IsNull(itemColArgs);
                Assert.IsNull(itemUpdateArgs);

                col.EndBufferUpdate();
                Assert.AreEqual(1, itemColArgs.AddedItems.Count);
                Assert.AreEqual(1, itemColArgs.RemovedItems.Count);
                Assert.AreEqual(1, itemUpdateArgs.UpdatedItems.Count);
            }
        }

        [Test]
        public void NestedBufferTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var colChanges = 0;
                var colUpdates = 0;
                var col = new ItemCollection<DatabaseObject>(temp.TempPath);
                col.ItemUpdated += (e, args) => colUpdates++;
                col.ItemCollectionChanged += (e, args) => colChanges++;
                var item = new DatabaseObject();

                col.BeginBufferUpdate();
                col.BeginBufferUpdate();
                col.BeginBufferUpdate();
                col.Add(item);
                col.Update(item);
                col.EndBufferUpdate();
                Assert.AreEqual(0, colChanges);
                Assert.AreEqual(0, colChanges);
                col.EndBufferUpdate();
                Assert.AreEqual(0, colChanges);
                Assert.AreEqual(0, colChanges);
                col.EndBufferUpdate();
                Assert.AreEqual(1, colUpdates);
                Assert.AreEqual(1, colChanges);

                col.BeginBufferUpdate();
                col.Update(item);
                col.EndBufferUpdate();
                Assert.AreEqual(2, colUpdates);
            }
        }

        [Test]
        public void BufferConsolidationTest()
        {
            using (var temp = TempDirectory.Create())
            {
                ItemCollectionChangedEventArgs<DatabaseObject> colChanges = null;
                ItemUpdatedEventArgs<DatabaseObject> colUpdates = null;
                var col = new ItemCollection<DatabaseObject>(temp.TempPath);
                col.ItemUpdated += (e, args) => colUpdates = args;
                col.ItemCollectionChanged += (e, args) => colChanges = args;

                var item = new DatabaseObject() { Name = "Original" };
                col.Add(item);

                col.BeginBufferUpdate();
                item.Name = "Change1";
                col.Update(item);
                item.Name = "Change2";
                col.Update(item);
                col.EndBufferUpdate();
                Assert.AreEqual(1, colUpdates.UpdatedItems.Count);
                Assert.AreEqual("Original", colUpdates.UpdatedItems[0].OldData.Name);
                Assert.AreEqual("Change2", colUpdates.UpdatedItems[0].NewData.Name);
            }
        }
    }
}

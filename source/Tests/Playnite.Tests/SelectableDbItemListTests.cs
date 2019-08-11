using NUnit.Framework;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    [TestFixture]
    public class SelectableDbItemListTests
    {
        [Test]
        public void PropertiesTest()
        {
            var items = new List<DatabaseObject>()
            {
                new DatabaseObject() { Name = "Test1" },
                new DatabaseObject() { Name = "Test2" }
            };

            List<Guid> refIds = null;
            var dbList = new SelectableDbItemList(items);
            dbList.SelectionChanged += (s, e) =>
            {
                refIds = dbList.GetSelectedIds().ToList();
            };

            var selectionItems = dbList.ToList();
            selectionItems[0].Selected = true;
            selectionItems[1].Selected = true;
            CollectionAssert.IsNotEmpty(refIds);
            Assert.AreEqual("Test1, Test2", dbList.AsString);
            
            selectionItems[1].Selected = false;
            Assert.AreEqual("Test1", dbList.AsString);
            Assert.AreEqual(1, refIds.Count);

            selectionItems[0].Selected = false;
            CollectionAssert.IsEmpty(refIds);
            Assert.IsEmpty(dbList.AsString);
        }

        [Test]
        public void EventsTest()
        {
            var items = new List<DatabaseObject>()
            {
                new DatabaseObject() { Name = "Test1" },
                new DatabaseObject() { Name = "Test2" },
                new DatabaseObject() { Name = "Test3" }
            };

            var properies = 0;
            var selections = 0;
            var dbList = new SelectableDbItemList(items);
            dbList.SelectionChanged += (s, e) =>
            {
                properies++;
            };

            dbList.PropertyChanged += (s, e) =>
            {
                selections++;
            };

            dbList.SetSelection(new List<Guid> { items[0].Id, items[1].Id });
            Assert.AreEqual(1, properies);
            Assert.AreEqual(1, selections);

            dbList.First().Selected = false;
            Assert.AreEqual(2, properies);
            Assert.AreEqual(2, selections);
        }
    }
}

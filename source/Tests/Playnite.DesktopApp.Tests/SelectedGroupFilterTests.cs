using NUnit.Framework;
using Playnite;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace Playnite.DesktopApp.Tests
{
    [TestFixture]
    public class SelectedGroupFilterTests
    {
        [Test]
        public void ShowsOnlySelectedGroup()
        {
            var selected = Guid.NewGuid();
            var other = Guid.NewGuid();
            var settings = new FilterSettings
            {
                ShowSelectedGroupsOnly = true,
                Category = new IdItemFilterItemProperties(selected)
            };

            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(settings, GroupableField.Category, selected));
            Assert.IsFalse(SelectedGroupFilter.IsEntryVisible(settings, GroupableField.Category, other));
        }

        [Test]
        public void ShowsAllGroupsWhenNoIdsSelected()
        {
            var settings = new FilterSettings
            {
                ShowSelectedGroupsOnly = true,
                Category = new IdItemFilterItemProperties(new List<Guid>())
            };

            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(settings, GroupableField.Category, Guid.NewGuid()));
        }

        [Test]
        public void FailsOpenWhenProjectionDoesNotApply()
        {
            var settings = new FilterSettings
            {
                ShowSelectedGroupsOnly = true,
                Category = new IdItemFilterItemProperties("action")
            };

            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(settings, GroupableField.Category, Guid.NewGuid()));
            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(settings, GroupableField.Source, Guid.NewGuid()));
            Assert.IsTrue(SelectedGroupFilter.IsEntryVisible(settings, GroupableField.Category, null));
        }
    }
}

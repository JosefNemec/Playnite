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
    public class FiltersDatabaseTests
    {
        [Test]
        public void SortedFilterPresetsTest()
        {
            using (var temp = TempDirectory.Create())
            using (var db = new TestGameDatabase(temp.TempPath))
            {
                db.OpenDatabase();
                var addedFilterPresets = new List<FilterPreset>();
                for (int i = 0; i < 10; i++)
                {
                    var filterPreset = new FilterPreset() { Name = $"Preset {i}" };
                    db.FilterPresets.Add(filterPreset);
                    addedFilterPresets.Add(filterPreset);
                }

                // Set default sorting order based on added order
                var newFilterPresetSettings = new FilterPresetsSettings
                {
                    SortingOrder = addedFilterPresets.Select(x => x.Id).ToList()
                };

                db.SetFilterPresetsSettings(newFilterPresetSettings);
                var sortedFilterPresets = db.GetSortedFilterPresets();

                // Verify that order is the same as when it was set
                Assert.IsTrue(sortedFilterPresets.Select(x => x.Id).SequenceEqual(addedFilterPresets.Select(x => x.Id)));

                // Set last added Filter Preset to first in order
                newFilterPresetSettings.SortingOrder.Remove(addedFilterPresets.Last().Id);
                newFilterPresetSettings.SortingOrder.Insert(0, addedFilterPresets.Last().Id);
                db.SetFilterPresetsSettings(newFilterPresetSettings);
                sortedFilterPresets = db.GetSortedFilterPresets();
                Assert.AreEqual(addedFilterPresets.Last(), sortedFilterPresets.First());

                // Add a new filter preset and verify that it shows as last item when obtaining sorted Filter Presets
                var newFilterPreset = new FilterPreset() { Name = "New Filter Preset" };
                db.FilterPresets.Add(newFilterPreset);
                sortedFilterPresets = db.GetSortedFilterPresets();
                Assert.AreEqual(newFilterPreset, sortedFilterPresets.Last());
            }
        }
    }
}

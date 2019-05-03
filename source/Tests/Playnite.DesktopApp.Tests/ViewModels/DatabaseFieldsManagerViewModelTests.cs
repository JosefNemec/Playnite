using Moq;
using NUnit.Framework;
using Playnite.Common;
using Playnite.Database;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK.Models;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.Tests.ViewModels
{
    [TestFixture]
    public class DatabaseFieldsManagerViewModelTests
    {
        [Test]
        public void StandardListUpdateTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                var cats = new List<Category>
                {
                    new Category("Test1"),
                    new Category("Test2"),
                    new Category("Test3")
                };

                db.Categories.Add(cats);
                var vm = new DatabaseFieldsManagerViewModel(db, new Mock<IWindowFactory>().Object, null, null);
                vm.EditingCategories[0].Name = "Test4";
                vm.EditingCategories.Add(new Category("Test5"));
                vm.EditingCategories.Add(new Category("Test6"));
                vm.EditingCategories.RemoveAt(2);
                vm.SaveChanges();
                                
                Assert.AreEqual(4, db.Categories.Count);
                Assert.AreEqual("Test4", db.Categories[cats[0].Id].Name);
                Assert.IsNotNull(db.Categories.First(a => a.Name == "Test2"));
                Assert.IsNotNull(db.Categories.First(a => a.Name == "Test5"));
                Assert.IsNotNull(db.Categories.First(a => a.Name == "Test6"));
            }
        }

        [Test]
        public void PlatformsUpdateTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                db.Platforms.Remove(db.Platforms);

                var plats = new List<Platform>
                {
                    new Platform("Test1"),
                    new Platform("Test2"),
                    new Platform("Test3")
                };

                db.Platforms.Add(plats);
                var vm = new DatabaseFieldsManagerViewModel(db, new Mock<IWindowFactory>().Object, null, null);
                vm.EditingPlatforms[0].Name = "Test4";
                vm.EditingPlatforms.Add(new Platform("Test5"));
                vm.EditingPlatforms.Add(new Platform("Test6"));
                vm.EditingPlatforms.RemoveAt(2);
                vm.SaveChanges();

                Assert.AreEqual(4, db.Platforms.Count);
                Assert.AreEqual("Test4", db.Platforms[plats[0].Id].Name);
                Assert.IsNotNull(db.Platforms.First(a => a.Name == "Test2"));
                Assert.IsNotNull(db.Platforms.First(a => a.Name == "Test5"));
                Assert.IsNotNull(db.Platforms.First(a => a.Name == "Test6"));

                // TODO add file test
            }
        }
    }
}

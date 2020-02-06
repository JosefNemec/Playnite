using Moq;
using NUnit.Framework;
using Playnite.Common;
using Playnite.Database;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Tests;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

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

        [Test]
        public void RemoveUnsusedTest()
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

                var features = new List<GameFeature>
                {
                    new GameFeature("Feature1"),
                    new GameFeature("Feature2"),
                    new GameFeature("Feature3"),
                    new GameFeature("Feature4")
                };

                var companies = new List<Company>
                {
                    new Company("Comp1"),
                    new Company("Comp2"),
                    new Company("Comp3"),
                    new Company("Comp4"),
                    new Company("Comp5")
                };

                db.Features.Add(features);
                db.Platforms.Add(plats);
                db.Companies.Add(companies);
                db.Games.Add(new Game("game1")
                {
                    PlatformId = plats[1].Id,
                    FeatureIds = new List<Guid> { features[0].Id, features[3].Id },
                    PublisherIds = new List<Guid> { companies[3].Id }
                });

                db.Games.Add(new Game("game2")
                {
                    DeveloperIds = new List<Guid> { companies[0].Id },
                    PublisherIds = new List<Guid> { companies[1].Id }
                });

                db.Games.Add(new Game("game3"));

                var detectedUnused = false;
                var dialogs = new Mock<IDialogsFactory>();
                dialogs.Setup(a => a.ShowMessage(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<MessageBoxButton>(), It.IsAny<MessageBoxImage>())).
                    Callback<string, string, MessageBoxButton, MessageBoxImage>((a, b, c, d) =>
                {
                    detectedUnused = a.Contains("2");
                }).Returns(MessageBoxResult.Yes);

                var vm = new DatabaseFieldsManagerViewModel(db, new Mock<IWindowFactory>().Object, dialogs.Object, new TestResourceProvider());
                vm.RemoveUnusedPlatformsCommand.Execute(null);
                Assert.IsTrue(detectedUnused);

                detectedUnused = false;
                vm.RemoveUnusedFeaturesCommand.Execute(null);
                Assert.IsTrue(detectedUnused);

                detectedUnused = false;
                vm.RemoveUnusedCompaniesCommand.Execute(null);
                Assert.IsTrue(detectedUnused);

                vm.RemoveUnusedSourcesCommand.Execute(null);
                vm.SaveChanges();

                Assert.AreEqual(1, db.Platforms.Count);
                Assert.IsNotNull(db.Platforms[plats[1].Id]);

                Assert.AreEqual(2, db.Features.Count);
                Assert.IsNotNull(db.Features[features[0].Id]);
                Assert.IsNotNull(db.Features[features[3].Id]);

                Assert.AreEqual(3, db.Companies.Count);
                Assert.IsNotNull(db.Companies[companies[0].Id]);
                Assert.IsNotNull(db.Companies[companies[1].Id]);
                Assert.IsNotNull(db.Companies[companies[3].Id]);
            }
        }
    }
}

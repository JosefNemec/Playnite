using NUnit.Framework;
using Playnite;
using Playnite.Common;
using Playnite.Database;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK.Models;
using Playnite.Tests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.Tests.ViewModels
{
    [TestFixture]
    public class GameEditViewModelTests
    {
        [Test]
        public void ImageReplaceTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();
                var game = new Game()
                {
                    GameId = "testid",
                    Name = "Test Game"
                };

                var origIcon = db.AddFile(PlayniteTests.GenerateFakeFile(), game.Id);
                var origImage = db.AddFile(PlayniteTests.GenerateFakeFile(), game.Id);
                var origBackground = db.AddFile(PlayniteTests.GenerateFakeFile(), game.Id);
                game.Icon = origIcon;
                game.CoverImage = origImage;
                game.BackgroundImage = origBackground;
                db.Games.Add(game);

                var newIcon = PlayniteTests.GenerateFakeFile();
                var newImage = PlayniteTests.GenerateFakeFile();
                var newBackground = PlayniteTests.GenerateFakeFile();
                File.WriteAllBytes(Path.Combine(temp.TempPath, newIcon.FileName), newIcon.Content);
                File.WriteAllBytes(Path.Combine(temp.TempPath, newImage.FileName), newImage.Content);
                File.WriteAllBytes(Path.Combine(temp.TempPath, newBackground.FileName), newBackground.Content);

                // Images are replaced
                var model = new GameEditViewModel(game, db, new MockWindowFactory(), new MockDialogsFactory(), new TestResourceProvider(), null, null, new PlayniteSettings());
                model.EditingGame.Icon = Path.Combine(temp.TempPath, newIcon.FileName);
                model.EditingGame.CoverImage = Path.Combine(temp.TempPath, newImage.FileName);
                model.EditingGame.BackgroundImage = Path.Combine(temp.TempPath, newBackground.FileName);
                model.ConfirmDialog();

                Assert.AreNotEqual(game.Icon, origIcon);
                Assert.AreNotEqual(game.CoverImage, origImage);
                Assert.AreNotEqual(game.BackgroundImage, origBackground);

                var dbFiles = Directory.GetFiles(db.GetFileStoragePath(game.Id));
                Assert.AreEqual(3, dbFiles.Count());      
                CollectionAssert.AreEqual(newIcon.Content, File.ReadAllBytes(db.GetFullFilePath(game.Icon)));
                CollectionAssert.AreEqual(newImage.Content, File.ReadAllBytes(db.GetFullFilePath(game.CoverImage)));
                CollectionAssert.AreEqual(newBackground.Content, File.ReadAllBytes(db.GetFullFilePath(game.BackgroundImage)));
            }
        }

        [Test]
        public void ImageReplaceMultiTest()
        {
            using (var temp = TempDirectory.Create())
            {
                var db = new GameDatabase(temp.TempPath);
                db.OpenDatabase();

                var game = new Game()
                {
                    GameId = "testid",
                    Name = "Test Game"
                };

                var origIcon = db.AddFile(PlayniteTests.GenerateFakeFile(), game.Id);
                var origImage = db.AddFile(PlayniteTests.GenerateFakeFile(), game.Id);
                var origBackground = db.AddFile(PlayniteTests.GenerateFakeFile(), game.Id);
                game.Icon = origIcon;
                game.CoverImage = origImage;
                game.BackgroundImage = origBackground;
                db.Games.Add(game);

                game = new Game()
                {
                    GameId = "testid2",
                    Name = "Test Game 2"
                };

                origIcon = db.AddFile(PlayniteTests.GenerateFakeFile(), game.Id);
                origImage = db.AddFile(PlayniteTests.GenerateFakeFile(), game.Id);
                origBackground = db.AddFile(PlayniteTests.GenerateFakeFile(), game.Id);
                game.Icon = origIcon;
                game.CoverImage = origImage;
                game.BackgroundImage = origBackground;
                db.Games.Add(game);

                var newIcon = PlayniteTests.GenerateFakeFile();
                var newImage = PlayniteTests.GenerateFakeFile();
                var newBackground = PlayniteTests.GenerateFakeFile();
                File.WriteAllBytes(Path.Combine(temp.TempPath, newIcon.FileName), newIcon.Content);
                File.WriteAllBytes(Path.Combine(temp.TempPath, newImage.FileName), newImage.Content);
                File.WriteAllBytes(Path.Combine(temp.TempPath, newBackground.FileName), newBackground.Content);

                // Replaces all images for all games
                var games = db.Games.ToList();
                var model = new GameEditViewModel(games, db, new MockWindowFactory(), new MockDialogsFactory(), new TestResourceProvider(), null, null, new PlayniteSettings());
                model.EditingGame.Icon = Path.Combine(temp.TempPath, newIcon.FileName);
                model.EditingGame.CoverImage = Path.Combine(temp.TempPath, newImage.FileName);
                model.EditingGame.BackgroundImage = Path.Combine(temp.TempPath, newBackground.FileName);
                model.ConfirmDialog();

                Assert.AreEqual(3, Directory.GetFiles(db.GetFileStoragePath(games[0].Id)).Count());
                Assert.AreEqual(3, Directory.GetFiles(db.GetFileStoragePath(games[1].Id)).Count());

                CollectionAssert.AreEqual(newIcon.Content, File.ReadAllBytes(db.GetFullFilePath(games[0].Icon)));
                CollectionAssert.AreEqual(newImage.Content, File.ReadAllBytes(db.GetFullFilePath(games[0].CoverImage)));
                CollectionAssert.AreEqual(newBackground.Content, File.ReadAllBytes(db.GetFullFilePath(games[0].BackgroundImage)));

                CollectionAssert.AreEqual(newIcon.Content, File.ReadAllBytes(db.GetFullFilePath(games[1].Icon)));
                CollectionAssert.AreEqual(newImage.Content, File.ReadAllBytes(db.GetFullFilePath(games[1].CoverImage)));
                CollectionAssert.AreEqual(newBackground.Content, File.ReadAllBytes(db.GetFullFilePath(games[1].BackgroundImage)));
            }
        }
    }
}

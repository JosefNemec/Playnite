using NUnit.Framework;
using Playnite;
using Playnite.Database;
using Playnite.SDK.Models;
using PlayniteUI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUITests.ViewModels
{
    [TestFixture]
    public class GameEditViewModelTests
    {
        [Test]
        public void ImageReplaceTest()
        {
            var path = Path.Combine(PlayniteUITests.TempPath, "imagereplace.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                var origIcon = PlayniteUITests.CreateFakeFile();
                var origImage = PlayniteUITests.CreateFakeFile();
                var origBackground = PlayniteUITests.CreateFakeFile();

                db.AddFile(origIcon.FileName, origIcon.FileName, origIcon.Content);
                db.AddFile(origImage.FileName, origImage.FileName, origImage.Content);
                db.AddFile(origBackground.FileName, origBackground.FileName, origBackground.Content);

                var game = new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game",
                    Image = origImage.FileName,
                    Icon = origIcon.FileName,
                    BackgroundImage = origBackground.FileName
                };

                db.AddGame(game);

                var newIcon = PlayniteUITests.CreateFakeFile();
                var newImage = PlayniteUITests.CreateFakeFile();
                var newBackground = PlayniteUITests.CreateFakeFile();
                
                // Images are replaced
                var model = new GameEditViewModel(game, db, new MockWindowFactory(), new MockDialogsFactory(), new MockResourceProvider());
                model.EditingGame.Icon = newIcon.FileId;
                model.EditingGame.Image = newImage.FileId;
                model.EditingGame.BackgroundImage = newBackground.FileId;
                model.ConfirmDialog();

                Assert.AreNotEqual(game.Icon, origIcon.FileName);
                Assert.AreNotEqual(game.Image, origImage.FileName);
                Assert.AreNotEqual(game.BackgroundImage, origBackground.FileName);
                Assert.AreNotEqual(game.Icon, newIcon.FileId);
                Assert.AreNotEqual(game.Image, newImage.FileId);
                Assert.AreNotEqual(game.BackgroundImage, newBackground.FileId);

                var dbFiles = db.Database.FileStorage.FindAll().ToList();
                Assert.AreEqual(3, dbFiles.Count());

                using (var str = db.GetFileStream(game.Icon))
                {
                    CollectionAssert.AreEqual(newIcon.Content, str.ToArray());
                }

                using (var str = db.GetFileStream(game.Image))
                {
                    CollectionAssert.AreEqual(newImage.Content, str.ToArray());
                }

                using (var str = db.GetFileStream(game.BackgroundImage))
                {
                    CollectionAssert.AreEqual(newBackground.Content, str.ToArray());
                }

                // Duplicates are kept and not replaced
                var currentIcon = game.Icon;
                var currentImage = game.Image;
                var currentBack = game.BackgroundImage;

                model = new GameEditViewModel(game, db, new MockWindowFactory(), new MockDialogsFactory(), new MockResourceProvider());
                model.EditingGame.Icon = newIcon.FileId;
                model.EditingGame.Image = newImage.FileId;
                model.EditingGame.BackgroundImage = newBackground.FileId;
                model.ConfirmDialog();

                dbFiles = db.Database.FileStorage.FindAll().ToList();
                Assert.AreEqual(3, dbFiles.Count());
                Assert.AreEqual(game.Icon, currentIcon);
                Assert.AreEqual(game.Image, currentImage);
                Assert.AreEqual(game.BackgroundImage, currentBack);
            }
        }

        [Test]
        public void ImageReplaceMultiTest()
        {
            var path = Path.Combine(PlayniteUITests.TempPath, "imagereplacemulti.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase(null);
            using (db.OpenDatabase(path))
            {
                var origIcon = PlayniteUITests.CreateFakeFile();
                var origImage = PlayniteUITests.CreateFakeFile();
                var origBackground = PlayniteUITests.CreateFakeFile();
                db.AddFile(origIcon.FileName, origIcon.FileName, origIcon.Content);
                db.AddFile(origImage.FileName, origImage.FileName, origImage.Content);
                db.AddFile(origBackground.FileName, origBackground.FileName, origBackground.Content);
                db.AddGame(new Game()
                {
                    ProviderId = "testid",
                    Name = "Test Game",
                    Image = origImage.FileName,
                    Icon = origIcon.FileName,
                    BackgroundImage = origBackground.FileName
                });

                origIcon = PlayniteUITests.CreateFakeFile();
                origImage = PlayniteUITests.CreateFakeFile();
                origBackground = PlayniteUITests.CreateFakeFile();
                db.AddFile(origIcon.FileName, origIcon.FileName, origIcon.Content);
                db.AddFile(origImage.FileName, origImage.FileName, origImage.Content);
                db.AddFile(origBackground.FileName, origBackground.FileName, origBackground.Content);
                db.AddGame(new Game()
                {
                    ProviderId = "testid2",
                    Name = "Test Game 2",
                    Image = origImage.FileName,
                    Icon = origIcon.FileName,
                    BackgroundImage = origBackground.FileName
                });

                Assert.AreEqual(6, db.Database.FileStorage.FindAll().ToList().Count());

                var newIcon = PlayniteUITests.CreateFakeFile();
                var newImage = PlayniteUITests.CreateFakeFile();
                var newBackground = PlayniteUITests.CreateFakeFile();

                // Replaces all images for all games
                var games = db.GamesCollection.FindAll().ToList();
                var model = new GameEditViewModel(games, db, new MockWindowFactory(), new MockDialogsFactory(), new MockResourceProvider());
                model.EditingGame.Icon = newIcon.FileId;
                model.EditingGame.Image = newImage.FileId;
                model.EditingGame.BackgroundImage = newBackground.FileId;
                model.ConfirmDialog();

                var dbFiles = db.Database.FileStorage.FindAll().ToList();
                Assert.AreEqual(3, dbFiles.Count());

                games = db.GamesCollection.FindAll().ToList();
                foreach (var game in games)
                {
                    StringAssert.StartsWith("images/custom/", game.Icon);
                    StringAssert.StartsWith("images/custom/", game.Image);
                    StringAssert.StartsWith("images/custom/", game.BackgroundImage);
                }

                Assert.AreEqual(games[0].Icon, games[1].Icon);
                Assert.AreEqual(games[0].Image, games[1].Image);
                Assert.AreEqual(games[0].BackgroundImage, games[1].BackgroundImage);

                // Replaces only non-duplicate images
                newIcon = PlayniteUITests.CreateFakeFile();
                newImage = PlayniteUITests.CreateFakeFile();
                newBackground = PlayniteUITests.CreateFakeFile();
                db.AddFile(newIcon.FileName, newIcon.FileName, newIcon.Content);
                db.AddFile(newImage.FileName, newImage.FileName, newImage.Content);
                db.AddFile(newBackground.FileName, newBackground.FileName, newBackground.Content);
                games[0].Icon = newIcon.FileName;
                games[0].Image = newImage.FileName;
                games[0].BackgroundImage = newBackground.FileName;
                db.UpdateGameInDatabase(games[0]);

                Assert.AreEqual(6, db.Database.FileStorage.FindAll().ToList().Count());

                games = db.GamesCollection.FindAll().ToList();
                model = new GameEditViewModel(games, db, new MockWindowFactory(), new MockDialogsFactory(), new MockResourceProvider());
                model.EditingGame.Icon = newIcon.FileId;
                model.EditingGame.Image = newImage.FileId;
                model.EditingGame.BackgroundImage = newBackground.FileId;
                model.ConfirmDialog();

                Assert.AreEqual(3, db.Database.FileStorage.FindAll().ToList().Count());

                games = db.GamesCollection.FindAll().ToList();
                foreach (var game in games)
                {
                    Assert.AreEqual(newIcon.FileName, game.Icon);
                    Assert.AreEqual(newImage.FileName, game.Image);
                    Assert.AreEqual(newBackground.FileName, game.BackgroundImage);
                }
            }
        }
    }
}

using LiteDB;
using NUnit.Framework;
using Playnite;
using Playnite.API;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests.API
{
    [TestFixture]
    public class DatabaseAPITests
    {
        [Test]
        public void GamesTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "dbapigames.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                var dbApi = new DatabaseAPI(db);
                Assert.AreEqual(0, dbApi.GetGames().Count);

                db.AddGames(new List<Game>()
                {
                    new Game()
                    {
                        Provider = Provider.Custom,
                        Name = "Test Name 1"
                    },
                    new Game()
                    {
                        Provider = Provider.Custom,
                        Name = "Test Name 2"
                    }
                });

                Assert.AreEqual(2, dbApi.GetGames().Count);

                dbApi.AddGame(new Game("API Game"));
                Assert.AreEqual(3, dbApi.GetGames().Count);

                dbApi.RemoveGame(dbApi.GetGames()[0].Id);
                var apiGames = dbApi.GetGames();
                Assert.AreEqual(2, apiGames.Count);
                Assert.AreEqual("API Game", apiGames[1].Name);
                Assert.AreEqual("Test Name 2", dbApi.GetGame(apiGames[0].Id).Name);

                apiGames[0].Name = "Changed Name";
                dbApi.UpdateGame(apiGames[0]);
                Assert.AreEqual("Changed Name", dbApi.GetGame(apiGames[0].Id).Name);
            }
        }

        [Test]
        public void FilesTest()
        {
            var path = Path.Combine(Playnite.PlayniteTests.TempPath, "dbapifiles.db");
            FileSystem.DeleteFile(path);

            var db = new GameDatabase();
            using (db.OpenDatabase(path))
            {
                var dbApi = new DatabaseAPI(db);
                Assert.AreEqual(0, dbApi.GetFiles().Count);

                db.AddFile("testid1", "testname1.png", new byte[] { 0, 1, 2 });
                db.AddFile("testid2", "testname2.png", new byte[] { 2, 1, 0 });

                var dbFiles = dbApi.GetFiles();
                Assert.AreEqual(2, dbFiles.Count);
                Assert.AreEqual("testid1", dbFiles[0].Id);
                Assert.AreEqual("testname1.png", dbFiles[0].Filename);
                Assert.AreEqual(3, dbFiles[0].Length);
                Assert.IsNotNull(dbFiles[0].Metadata["checksum"]);

                dbApi.RemoveFile(dbFiles[0].Id);
                Assert.AreEqual(1, dbApi.GetFiles().Count);

                var filePath = Path.Combine(Playnite.PlayniteTests.TempPath, "testname3.png");
                File.WriteAllBytes(filePath, new byte[] { 2, 1, 0 });

                var duplId = dbApi.AddFileNoDuplicates("testid3", filePath);
                Assert.AreEqual("testid2", duplId);
                Assert.AreEqual(1, dbApi.GetFiles().Count);

                dbApi.AddFile("testid3", filePath);
                Assert.AreEqual(2, dbApi.GetFiles().Count);

                filePath = Path.Combine(Playnite.PlayniteTests.TempPath, "testFile2.png");
                FileSystem.DeleteFile(filePath);
                dbApi.SaveFile("testid2", filePath);
                Assert.IsTrue(File.Exists(filePath));
                Assert.AreEqual(3, File.ReadAllBytes(filePath).Count());
            }
        }
    }
}

using LiteDB;
using Moq;
using NUnit.Framework;
using Playnite;
using Playnite.Common;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.Tests.Database
{
    [TestFixture]
    public class GameDatabaseFileTests
    {
        [Test]
        public void AddFileTest()
        {
            using (var db = new GameDbTestWrapper())
            {
                var testId = Guid.NewGuid();
                var filesDir = Path.Combine(db.DbDirectory, "files", testId.ToString());

                void testImage(string path, string resFilter, string addedExt, bool addAsImage)
                {
                    var newPath = db.DB.AddFile(path, testId, addAsImage, CancellationToken.None);
                    Assert.IsNotNull(newPath);
                    StringAssert.EndsWith(addedExt, newPath);
                    Assert.AreEqual(1, Directory.GetFiles(filesDir, resFilter).Count());
                    FileSystem.DeleteDirectory(filesDir);
                }

                void testImageMeta(MetadataFile file, string resFilter)
                {
                    Assert.IsNotNull(db.DB.AddFile(file, testId, true, CancellationToken.None));
                    Assert.AreEqual(1, Directory.GetFiles(filesDir, resFilter).Count());
                    FileSystem.DeleteDirectory(filesDir);
                }

                // Standard image local file
                testImage(Path.Combine(PlayniteTests.ResourcesPath, "Images", "applogo.png"), "*.png", ".png", true);

                // Standard image htpp file
                testImage(@"https://playnite.link/applogo.png", "*.png", ".png", true);

                // Standard image content file
                var contentImage = File.ReadAllBytes(Path.Combine(PlayniteTests.ResourcesPath, "Images", "applogo.png"));
                testImageMeta(new MetadataFile("test.png", contentImage), "*.png");

                // Local exe icon file
                testImage(Path.Combine(PlayniteTests.ResourcesPath, "Images", "YesIcon.exe"), "*.ico", ".ico", true);

                // Http exe icon file
                testImage(@"https://playnite.link/YesIcon.exe", "*.ico", ".ico", true);

                // No image
                testImage(Path.Combine(PlayniteTests.ResourcesPath, "Images", "YesIcon.exe"), "*.exe", ".exe", false);

                // No image http
                testImage(@"https://playnite.link/YesIcon.exe", "*.exe", ".exe", false);
            }
        }

        [Test]
        public void AddFileFailureTest()
        {
            using (var db = new GameDbTestWrapper())
            {
                var testId = Guid.NewGuid();
                var filesDir = Path.Combine(db.DbDirectory, "files", testId.ToString());

                void testImage(string path)
                {
                    Assert.IsNull(db.DB.AddFile(path, testId, true, CancellationToken.None));
                    Assert.IsTrue(!Directory.Exists(filesDir) || Directory.GetFiles(filesDir, "*.*").Count() == 0);
                    FileSystem.DeleteDirectory(filesDir);
                }

                // Missing http file
                testImage(@"https://playnite.link/doesntexists.png");

                // Missing local file
                testImage(@"c:\dir\nope.png");

                // Http exe no icon file
                testImage(@"https://playnite.link/NoIcon.exe");

                // Non-image filed
                testImage(Path.Combine(PlayniteTests.ResourcesPath, "Images", "NoIcon.exe"));
            }
        }

        [Test]
        public void ImageFilePathReuseTest()
        {
            using (var db = new GameDbTestWrapper())
            {
                var testId = Guid.NewGuid();
                var filesDir = Path.Combine(db.DbDirectory, "files", testId.ToString());
                FileSystem.CreateDirectory(filesDir, true);

                File.Copy(
                    Path.Combine(PlayniteTests.ResourcesPath, "Images", "applogo.png"),
                    Path.Combine(filesDir, "newFile.png"));

                var resFile = db.DB.AddFile(Path.Combine(filesDir, "newFile.png"), testId, true, CancellationToken.None);
                Assert.AreEqual(testId + "\\" + "newFile.png", resFile);
                Assert.AreEqual(1, Directory.GetFiles(filesDir, "*.*").Count());
            }
        }
    }
}

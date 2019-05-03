using Moq;
using NUnit.Framework;
using Playnite;
using Playnite.Common.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests.App
{
    [TestFixture]
    public class UpdateTests
    {
        private Mock<IDownloader> mockDownloader;
        private Mock<IPlayniteApplication> mockApp;
        private string stringManifest;

        [OneTimeSetUp]
        public void Init()
        {
            mockDownloader = new Mock<IDownloader>();
            mockApp = new Mock<IPlayniteApplication>();
            stringManifest = File.ReadAllText(Path.Combine(PlayniteTests.ResourcesPath, "TestUpdateManifest.json"));
        }

        [Test]
        public void ReleaseNotesParseTest()
        {
            IEnumerable<string> notesUrl = null;

            mockDownloader.Setup(a => a.DownloadString(It.IsAny<string>())).Returns(stringManifest);
            mockDownloader.Setup(a => a.DownloadString(It.IsAny<IEnumerable<string>>())).Callback<IEnumerable<string>>((a) =>
            {
                notesUrl = a;
            }).Returns("release note");

            var update = new Updater(mockApp.Object, mockDownloader.Object);
            var manifest = update.DownloadManifest();
            CollectionAssert.IsNotEmpty(manifest.DownloadServers);
            CollectionAssert.IsNotEmpty(manifest.Packages);
            CollectionAssert.IsNotEmpty(manifest.ReleaseNotes);
            CollectionAssert.IsNotEmpty(manifest.ReleaseNotesUrlRoots);
            Assert.IsNotNull(manifest.LatestVersion);

            var notes = update.DownloadReleaseNotes(new Version("1.0.0.0"));
            Assert.AreEqual(3, notes.Count);
            Assert.AreEqual("release note", notes[0].Note);
            Assert.AreEqual(new Version("4.21"), notes[0].Version);
            Assert.AreEqual("http://localhost/update/4.1.html", notesUrl.First());
        }

        [Test]
        public void ChooseBestDiffPassTest()
        {
            var update = new Updater(mockApp.Object, mockDownloader.Object);
            var manifest = new UpdateManifest()
            {
                LatestVersion = new Version(4, 21),
                Packages = new List<UpdateManifest.Package>()
                {
                    new UpdateManifest.Package() { BaseVersion = new Version("4.21"), FileName = "421.exe" },
                    new UpdateManifest.Package() { BaseVersion = new Version("4.2"), FileName = "42to42.exe" },
                    new UpdateManifest.Package() { BaseVersion = new Version("4.1"), FileName = "41to42.exe" }
                }
            };

            var ver = new Version("4.2.0.0");
            var package = update.GetUpdatePackage(manifest, ver);
            Assert.AreEqual(ver.ToString(2), package.BaseVersion.ToString());
            Assert.AreEqual("42to42.exe", package.FileName);
        }

        [Test]
        public void ChooseBestDiffFullTest()
        {
            mockDownloader.Setup(a => a.DownloadString(It.IsAny<string>())).Returns(stringManifest);
            var update = new Updater(mockApp.Object, mockDownloader.Object);
            var manifest = new UpdateManifest()
            {
                LatestVersion = new Version(4, 21),
                Packages = new List<UpdateManifest.Package>()
                {
                    new UpdateManifest.Package() { BaseVersion = new Version("4.21"), FileName = "421.exe" },
                    new UpdateManifest.Package() { BaseVersion = new Version("4.12"), FileName = "412to42.exe" },
                    new UpdateManifest.Package() { BaseVersion = new Version("4.1"), FileName = "41to42.exe" }
                }
            };

            var ver = new Version("4.9");
            var package = update.GetUpdatePackage(manifest, ver);
            Assert.AreEqual(manifest.LatestVersion, package.BaseVersion);
            Assert.AreEqual("421.exe", package.FileName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using NUnit.Framework;
using Playnite;
using Playnite.Settings;
using Playnite.Common;

namespace Playnite.Tests
{
    [TestFixture]
    public class SettingsTests
    {
        [SetUp]
        public void TestInit()
        {
            FileSystem.DeleteFile(PlaynitePaths.UninstallerPath);
        }

        [OneTimeTearDown]
        public void Cleanup()
        {
            FileSystem.DeleteFile(PlaynitePaths.UninstallerPath);
        }

        [Test]
        public void PortableInnoPathsTest()
        {
            Assert.IsTrue(PlayniteSettings.IsPortable);
            Assert.AreNotEqual(PlaynitePaths.UserProgramDataPath, PlaynitePaths.ConfigRootPath);
        }

        [Test]
        public void PeriodicUpdateCheckTest()
        {
            var settings = new PlayniteSettings();
            var testDates = new TestDateTimes();
            using (DateTimes.UseCustomDates(testDates))
            {
                // --- Program update
                // No date set check
                testDates.Now = new DateTime(2020, 10, 1, 12, 0, 0);
                settings.CheckForProgramUpdates = UpdateCheckFrequency.OnceADay;
                Assert.IsTrue(settings.ShouldCheckProgramUpdatePeriodic());

                // Once a day check
                settings.LastProgramUpdateCheck = new DateTime(2020, 10, 1, 10, 00, 0);
                Assert.IsFalse(settings.ShouldCheckProgramUpdatePeriodic());
                testDates.Now = new DateTime(2020, 10, 2, 10, 0, 0);
                Assert.IsTrue(settings.ShouldCheckProgramUpdatePeriodic());

                // Once a week check
                settings.CheckForProgramUpdates = UpdateCheckFrequency.OnceAWeek;
                Assert.IsFalse(settings.ShouldCheckProgramUpdatePeriodic());
                testDates.Now = new DateTime(2020, 10, 8, 10, 0, 0);
                Assert.IsTrue(settings.ShouldCheckProgramUpdatePeriodic());

                // Startup only check
                settings.CheckForProgramUpdates = UpdateCheckFrequency.OnEveryStartup;
                Assert.IsFalse(settings.ShouldCheckProgramUpdatePeriodic());

                // --- Addon update
                // No date set check
                testDates.Now = new DateTime(2020, 10, 1, 12, 0, 0);
                settings.CheckForAddonUpdates = UpdateCheckFrequency.OnceADay;
                Assert.IsTrue(settings.ShouldCheckAddonUpdatePeriodic());

                // Once a day check
                settings.LastAddonUpdateCheck = new DateTime(2020, 10, 1, 10, 00, 0);
                Assert.IsFalse(settings.ShouldCheckAddonUpdatePeriodic());
                testDates.Now = new DateTime(2020, 10, 2, 10, 0, 0);
                Assert.IsTrue(settings.ShouldCheckAddonUpdatePeriodic());

                // Once a week check
                settings.CheckForAddonUpdates = UpdateCheckFrequency.OnceAWeek;
                Assert.IsFalse(settings.ShouldCheckAddonUpdatePeriodic());
                testDates.Now = new DateTime(2020, 10, 8, 10, 0, 0);
                Assert.IsTrue(settings.ShouldCheckAddonUpdatePeriodic());

                // Startup only check
                settings.CheckForAddonUpdates = UpdateCheckFrequency.OnEveryStartup;
                Assert.IsFalse(settings.ShouldCheckAddonUpdatePeriodic());
            }
        }

        [Test]
        public void StartupUpdateCheckTest()
        {
            var settings = new PlayniteSettings();
            var testDates = new TestDateTimes();
            using (DateTimes.UseCustomDates(testDates))
            {
                // --- Program update
                // No date set check
                testDates.Now = new DateTime(2020, 10, 1, 12, 0, 0);
                settings.CheckForProgramUpdates = UpdateCheckFrequency.OnceADay;
                Assert.IsTrue(settings.ShouldCheckProgramUpdateStartup());

                // Once a day check
                settings.LastProgramUpdateCheck = new DateTime(2020, 10, 1, 10, 00, 0);
                Assert.IsFalse(settings.ShouldCheckProgramUpdateStartup());
                testDates.Now = new DateTime(2020, 10, 2, 10, 0, 0);
                Assert.IsTrue(settings.ShouldCheckProgramUpdateStartup());

                // Once a week check
                settings.CheckForProgramUpdates = UpdateCheckFrequency.OnceAWeek;
                Assert.IsFalse(settings.ShouldCheckProgramUpdateStartup());
                testDates.Now = new DateTime(2020, 10, 8, 10, 0, 0);
                Assert.IsTrue(settings.ShouldCheckProgramUpdateStartup());

                // Startup only check
                settings.CheckForProgramUpdates = UpdateCheckFrequency.OnEveryStartup;
                Assert.IsTrue(settings.ShouldCheckProgramUpdateStartup());

                // --- Addon update
                // No date set check
                testDates.Now = new DateTime(2020, 10, 1, 12, 0, 0);
                settings.CheckForAddonUpdates = UpdateCheckFrequency.OnceADay;
                Assert.IsTrue(settings.ShouldCheckAddonUpdateStartup());

                // Once a day check
                settings.LastAddonUpdateCheck = new DateTime(2020, 10, 1, 10, 00, 0);
                Assert.IsFalse(settings.ShouldCheckAddonUpdateStartup());
                testDates.Now = new DateTime(2020, 10, 2, 10, 0, 0);
                Assert.IsTrue(settings.ShouldCheckAddonUpdateStartup());

                // Once a week check
                settings.CheckForAddonUpdates = UpdateCheckFrequency.OnceAWeek;
                Assert.IsFalse(settings.ShouldCheckAddonUpdateStartup());
                testDates.Now = new DateTime(2020, 10, 8, 10, 0, 0);
                Assert.IsTrue(settings.ShouldCheckAddonUpdateStartup());

                // Startup only check
                settings.CheckForAddonUpdates = UpdateCheckFrequency.OnEveryStartup;
                Assert.IsTrue(settings.ShouldCheckAddonUpdateStartup());
            }
        }

        [Test]
        public void LibraryUpdateStartupCheckTest()
        {
            var settings = new PlayniteSettings();
            var testDates = new TestDateTimes();
            using (DateTimes.UseCustomDates(testDates))
            {
                // Lib update
                // Manual check
                settings.CheckForLibraryUpdates = LibraryUpdateCheckFrequency.Manually;
                Assert.IsFalse(settings.ShouldCheckLibraryOnStartup());

                // No date set check
                testDates.Now = new DateTime(2020, 10, 1, 12, 0, 0);
                settings.CheckForLibraryUpdates = LibraryUpdateCheckFrequency.OnceADay;
                Assert.IsTrue(settings.ShouldCheckLibraryOnStartup());

                // Once a day check
                settings.LastLibraryUpdateCheck = new DateTime(2020, 10, 1, 10, 00, 0);
                Assert.IsFalse(settings.ShouldCheckLibraryOnStartup());
                testDates.Now = new DateTime(2020, 10, 2, 10, 0, 0);
                Assert.IsTrue(settings.ShouldCheckLibraryOnStartup());

                // Once a week check
                settings.CheckForLibraryUpdates = LibraryUpdateCheckFrequency.OnceAWeek;
                Assert.IsFalse(settings.ShouldCheckLibraryOnStartup());
                testDates.Now = new DateTime(2020, 10, 8, 10, 0, 0);
                Assert.IsTrue(settings.ShouldCheckLibraryOnStartup());

                // Startup onyl check
                settings.CheckForLibraryUpdates = LibraryUpdateCheckFrequency.OnEveryStartup;
                Assert.IsTrue(settings.ShouldCheckLibraryOnStartup());

                // Emu update
                // Manual check
                settings.CheckForEmulatedLibraryUpdates = LibraryUpdateCheckFrequency.Manually;
                Assert.IsFalse(settings.ShouldCheckEmuLibraryOnStartup());

                // No date set check
                testDates.Now = new DateTime(2020, 10, 1, 12, 0, 0);
                settings.CheckForEmulatedLibraryUpdates = LibraryUpdateCheckFrequency.OnceADay;
                Assert.IsTrue(settings.ShouldCheckEmuLibraryOnStartup());

                // Once a day check
                settings.LastEmuLibraryUpdateCheck = new DateTime(2020, 10, 1, 10, 00, 0);
                Assert.IsFalse(settings.ShouldCheckEmuLibraryOnStartup());
                testDates.Now = new DateTime(2020, 10, 2, 10, 0, 0);
                Assert.IsTrue(settings.ShouldCheckEmuLibraryOnStartup());

                // Once a week check
                settings.CheckForEmulatedLibraryUpdates = LibraryUpdateCheckFrequency.OnceAWeek;
                Assert.IsFalse(settings.ShouldCheckEmuLibraryOnStartup());
                testDates.Now = new DateTime(2020, 10, 8, 10, 0, 0);
                Assert.IsTrue(settings.ShouldCheckEmuLibraryOnStartup());

                // Startup onyl check
                settings.CheckForEmulatedLibraryUpdates = LibraryUpdateCheckFrequency.OnEveryStartup;
                Assert.IsTrue(settings.ShouldCheckEmuLibraryOnStartup());
            }
        }
    }
}

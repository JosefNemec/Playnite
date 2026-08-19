using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using NUnit.Framework;
using Playnite.Common;
using Playnite.Converters;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Tests.Database;

namespace Playnite.Tests
{
    [TestFixture]
    public class LibraryObjectNameMatchToBoolConverterTests
    {
        [Test]
        public void ConvertTests()
        {
            using var temp = TempDirectory.Create();
            using var db = new TestGameDatabase(temp.TempPath);
            db.OpenDatabase();
            db.ImportGame(new GameMetadata
            {
                Platforms = new HashSet<MetadataProperty>
                {
                    new MetadataNameProperty("Test Platform"),
                    new MetadataNameProperty("Test Platform 2"),
                },
                Source = new MetadataNameProperty("Test Source"),
            }, Guid.Empty);

            var game = db.Games.First();
            var converter = new LibraryObjectNameMatchToBoolConverter();
            var convertedVal = converter.Convert(game.Source, typeof(bool), "test-source" , CultureInfo.CurrentCulture);
            Assert.IsTrue(convertedVal is true);

            convertedVal = converter.Convert(game.Platforms, typeof(bool), "TestPlatform2" , CultureInfo.CurrentCulture);
            Assert.IsTrue(convertedVal is true);

            convertedVal = converter.Convert(game.Platforms, typeof(Visibility), "TestPlatform2" , CultureInfo.CurrentCulture);
            Assert.IsTrue(convertedVal is Visibility.Visible);

            convertedVal = converter.Convert(game.Platforms, typeof(bool), "notplatform" , CultureInfo.CurrentCulture);
            Assert.IsTrue(convertedVal is false);

            convertedVal = converter.Convert(game.Genres, typeof(bool), "notplatform" , CultureInfo.CurrentCulture);
            Assert.IsTrue(convertedVal is false);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Playnite.Metadata.Providers;
using Playnite.SDK.Metadata;
using Playnite.SDK.Models;

namespace Playnite.Tests.Metadata
{
    [TestFixture]
    public class WikipediaMetadataProviderTests
    {
        private void ValidateGameDate(GameMetadata metadata)
        {
            Assert.IsNotNull(metadata.GameInfo.ReleaseDate);
            Assert.IsTrue(metadata.GameInfo.Developers.Count > 0 && !string.IsNullOrEmpty(metadata.GameInfo.Developers[0]));
            Assert.IsTrue(metadata.GameInfo.Publishers.Count > 0 && !string.IsNullOrEmpty(metadata.GameInfo.Publishers[0]));
            Assert.IsTrue(metadata.GameInfo.Genres.Count > 0 && !string.IsNullOrEmpty(metadata.GameInfo.Genres[0]));
        }

        private void ValidateBoxArt(GameMetadata game)
        {
            Assert.IsTrue(game.CoverImage?.OriginalUrl.IsNullOrEmpty() == false);
        }

        [Test]
        public void ParseGamePage_MetadataParsingTest()
        {
            var wiki = new WikipediaMetadataPlugin(null);

            // Standard page
            var metadata = wiki.ParseGamePage(wiki.GetPage("Guild Wars 2"));
            ValidateGameDate(metadata);
            ValidateBoxArt(metadata);

            // Without title in info box
            metadata = wiki.ParseGamePage(wiki.GetPage("Kingpin: Life of Crime"));
            ValidateGameDate(metadata);
            ValidateBoxArt(metadata);

            // Multiple release dates
            metadata = wiki.ParseGamePage(wiki.GetPage("Command & Conquer: Red Alert"));
            ValidateGameDate(metadata);
            ValidateBoxArt(metadata);

            // Multiple developers
            metadata = wiki.ParseGamePage(wiki.GetPage("Counter-Strike: Global Offensive"));
            ValidateGameDate(metadata);
            ValidateBoxArt(metadata);

            // Different page laytout
            metadata = wiki.ParseGamePage(wiki.GetPage("Quake III Arena"));
            ValidateGameDate(metadata);
            ValidateBoxArt(metadata);

            // Multiple property tables
            metadata = wiki.ParseGamePage(wiki.GetPage("TrackMania"), "TrackMania United");
            ValidateGameDate(metadata);
            ValidateBoxArt(metadata);

            // No image
            metadata = wiki.ParseGamePage(wiki.GetPage("State of War (video game)"));
            ValidateGameDate(metadata);

            // Different formats
            metadata = wiki.ParseGamePage(wiki.GetPage("Dungeon Siege"));
            ValidateGameDate(metadata);

            metadata = wiki.ParseGamePage(wiki.GetPage("Quake 4"));
            ValidateGameDate(metadata);

            metadata = wiki.ParseGamePage(wiki.GetPage("DEFCON (video game)"));
            ValidateGameDate(metadata);

            metadata = wiki.ParseGamePage(wiki.GetPage("Kid Chaos (video game)"));
            ValidateGameDate(metadata);

            metadata = wiki.ParseGamePage(wiki.GetPage("StarCraft II: Wings of Liberty"));
            ValidateGameDate(metadata);
        }
    }
}

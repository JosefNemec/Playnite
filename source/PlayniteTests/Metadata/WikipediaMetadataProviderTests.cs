using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Playnite.Metadata.Providers;
using Playnite.SDK.Models;

namespace PlayniteTests.Metadata
{
    [TestFixture]
    public class WikipediaMetadataProviderTests
    {
        private void ValidateGameDate(Game game)
        {
            Assert.IsNotNull(game.ReleaseDate);
            Assert.IsTrue(game.Developers.Count > 0 && !string.IsNullOrEmpty(game.Developers[0]));
            Assert.IsTrue(game.Publishers.Count > 0 && !string.IsNullOrEmpty(game.Publishers[0]));
            Assert.IsTrue(game.Genres.Count > 0 && !string.IsNullOrEmpty(game.Genres[0]));
        }

        private void ValidateBoxArt(Game game)
        {
            Assert.IsTrue(!string.IsNullOrEmpty(game.CoverImage));
        }

        [Test]
        public void ParseGamePage_MetadataParsingTest()
        {
            var wiki = new WikipediaMetadataProvider();

            // Standard page
            var game = wiki.ParseGamePage(wiki.GetPage("Guild Wars 2"));
            ValidateGameDate(game);
            ValidateBoxArt(game);

            // Without title in info box
            game = wiki.ParseGamePage(wiki.GetPage("Kingpin: Life of Crime"));
            ValidateGameDate(game);
            ValidateBoxArt(game);

            // Multiple release dates
            game = wiki.ParseGamePage(wiki.GetPage("Command & Conquer: Red Alert"));
            ValidateGameDate(game);
            ValidateBoxArt(game);

            // Multiple developers
            game = wiki.ParseGamePage(wiki.GetPage("Counter-Strike: Global Offensive"));
            ValidateGameDate(game);
            ValidateBoxArt(game);

            // Different page laytout
            game = wiki.ParseGamePage(wiki.GetPage("Quake III Arena"));
            ValidateGameDate(game);
            ValidateBoxArt(game);

            // Multiple property tables
            game = wiki.ParseGamePage(wiki.GetPage("TrackMania"), "TrackMania United");
            ValidateGameDate(game);
            ValidateBoxArt(game);

            // No image
            game = wiki.ParseGamePage(wiki.GetPage("State of War (video game)"));
            ValidateGameDate(game);

            // Different formats
            game = wiki.ParseGamePage(wiki.GetPage("Dungeon Siege"));
            ValidateGameDate(game);

            game = wiki.ParseGamePage(wiki.GetPage("MotoGP 3: Ultimate Racing Technology"));
            ValidateGameDate(game);

            game = wiki.ParseGamePage(wiki.GetPage("Quake 4"));
            ValidateGameDate(game);

            game = wiki.ParseGamePage(wiki.GetPage("DEFCON (video game)"));
            ValidateGameDate(game);

            game = wiki.ParseGamePage(wiki.GetPage("Kid Chaos (video game)"));
            ValidateGameDate(game);

            game = wiki.ParseGamePage(wiki.GetPage("StarCraft II: Wings of Liberty"));
            ValidateGameDate(game);
        }
    }
}

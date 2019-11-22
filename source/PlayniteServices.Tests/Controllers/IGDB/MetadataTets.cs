using Playnite.Common;
using PlayniteServices;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using SdkModels = Playnite.SDK.Models;

namespace PlayniteServicesTests.Controllers.IGDB
{
    [Collection("DefaultCollection")]
    public class MetadataTets
    {
        private readonly HttpClient client;

        public MetadataTets(TestFixture<Startup> fixture)
        {
            client = fixture.Client;
        }

        private async Task<ExpandedGame> GetMetadata(SdkModels.Game game)
        {
            var content = new StringContent(Serialization.ToJson(game), Encoding.UTF8, MediaTypeNames.Application.Json);
            var response = await client.PostAsync(@"/igdb/metadata", content);
            return Serialization.FromJson<ServicesResponse<ExpandedGame>>(await response.Content.ReadAsStringAsync()).Data;
        }

        [Fact]
        public async Task AlternateNameUseTest()
        {
            var metadata = await GetMetadata(new SdkModels.Game("pubg"));
            Assert.Equal("PLAYERUNKNOWN'S BATTLEGROUNDS", metadata.name);

            metadata = await GetMetadata(new SdkModels.Game("unreal 2"));
            Assert.Equal("Unreal II: The Awakening", metadata.name);
        }

        [Fact]
        public async Task SteamIdUseTest()
        {
            var steamGame = new SdkModels.Game
            {
                PluginId = Guid.Parse("CB91DFC9-B977-43BF-8E70-55F46E410FAB"),
                GameId = "7200"
            };

            var metadata = await GetMetadata(steamGame);
            Assert.Equal("TrackMania United", metadata.name);
        }

        [Fact]
        public async Task ReleaseDateUseTest()
        {
            var game = new SdkModels.Game("Tomb Raider")
            {
                ReleaseDate = new DateTime(1996, 1, 1)
            };

            var metadata = await GetMetadata(game);
            Assert.Equal(1996, metadata.first_release_date.ToDateFromUnixMs().Year);
            Assert.Equal("Core Design", metadata.developers[0]);

            game.ReleaseDate = new DateTime(2013, 1, 1);
            metadata = await GetMetadata(game);
            Assert.Equal(2013, metadata.first_release_date.ToDateFromUnixMs().Year);
            Assert.Equal("Crystal Dynamics", metadata.developers[0]);
        }

        [Fact]
        public async Task NameMatchingTest()
        {
            // & / and test
            var metadata = await GetMetadata(new SdkModels.Game("Command and Conquer"));
            Assert.NotNull(metadata.cover);
            Assert.Equal("Command & Conquer", metadata.name);
            Assert.Equal(1995, metadata.first_release_date.ToDateFromUnixMs().Year);

            // Matches exactly
            metadata = await GetMetadata(new SdkModels.Game("Grand Theft Auto IV"));
            Assert.Equal("Grand Theft Auto IV", metadata.name);

            // Roman numerals test
            metadata = await GetMetadata(new SdkModels.Game("Quake 3 Arena"));
            Assert.NotNull(metadata.cover);
            Assert.Equal("Quake III Arena", metadata.name);

            // THE test
            metadata = await GetMetadata(new SdkModels.Game("Witcher 3: Wild Hunt"));
            Assert.Equal("The Witcher 3: Wild Hunt", metadata.name);

            // No subtitle test
            metadata = await GetMetadata(new SdkModels.Game("The Witcher 3"));
            Assert.Equal("The Witcher 3: Wild Hunt", metadata.name);

            // Apostrophe test
            metadata = await GetMetadata(new SdkModels.Game("Dragons Lair"));
            Assert.Equal("Dragon's Lair", metadata.name);

            // Hyphen vs. colon test
            metadata = await GetMetadata(new SdkModels.Game("Legacy of Kain - Soul Reaver 2"));
            Assert.Equal("Legacy of Kain: Soul Reaver 2", metadata.name);

            metadata = await GetMetadata(new SdkModels.Game("Legacy of Kain: Soul Reaver 2"));
            Assert.Equal("Legacy of Kain: Soul Reaver 2", metadata.name);            

            // Trademarks test
            metadata = await GetMetadata(new SdkModels.Game("Dishonored®: Death of the Outsider™"));
            Assert.Equal("Dishonored: Death of the Outsider", metadata.name);
        }
    }
}

using Playnite.Common;
using Playnite.SDK;
using PlayniteServices;
using PlayniteServices.Controllers.IGDB;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var response = await client.PostAsync(@"/igdb/metadata_v2", content);
            return Serialization.FromJson<ServicesResponse<ExpandedGame>>(await response.Content.ReadAsStringAsync()).Data;
        }

        private int GetYearFromUnix(long date)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(date).DateTime.Year;
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
            var metadata = await GetMetadata(new SdkModels.Game("")
            {
                PluginId = BuiltinExtensions.GetIdFromExtension(BuiltinExtension.SteamLibrary),
                GameId = "7200"
            });

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
            Assert.Equal(1996, GetYearFromUnix(metadata.first_release_date));
            Assert.Equal("Core Design", metadata.involved_companies.Where(a => a.developer).First().company.name);

            game.ReleaseDate = new DateTime(2013, 1, 1);
            metadata = await GetMetadata(game);
            Assert.Equal(2013, GetYearFromUnix(metadata.first_release_date));
            Assert.Equal("Crystal Dynamics", metadata.involved_companies.Where(a => a.developer).First().company.name);
        }

        [Fact]
        public async Task NameMatchingTest()
        {
            // No-Intro naming
            var metadata = await GetMetadata(new SdkModels.Game("Bug's Life, A"));
            Assert.Equal((ulong)2847, metadata.id);

            metadata = await GetMetadata(new SdkModels.Game("Warhammer 40,000: Space Marine"));
            Assert.Equal((ulong)578, metadata.id);

            // & / and test
            metadata = await GetMetadata(new SdkModels.Game("Command and Conquer"));
            Assert.NotNull(metadata.cover);
            Assert.Equal("Command & Conquer", metadata.name);
            Assert.Equal(1995, GetYearFromUnix(metadata.first_release_date));

            // Matches exactly
            metadata = await GetMetadata(new SdkModels.Game("Grand Theft Auto IV"));
            Assert.Equal(2008, GetYearFromUnix(metadata.first_release_date));

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

        //[Fact]
        //public async Task BigMatchingTest()
        //{
        //    var resultPath = @"d:\Downloads\download_" + Guid.NewGuid() + ".txt";
        //    var gameList = File.ReadAllLines(@"d:\Downloads\export.csv");
        //    var results = new List<string>();
        //    foreach (var game in gameList)
        //    {
        //        if (game.IsNullOrEmpty())
        //        {
        //            continue;
        //        }

        //        var metadata = await GetMetadata(new SdkModels.Game(game));
        //        File.AppendAllText(resultPath, $"{game}#{metadata.id}#{metadata.name}" + Environment.NewLine);
        //    }
        //}
    }
}

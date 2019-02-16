using Newtonsoft.Json;
using PlayniteServices;
using PlayniteServices.Models.IGDB;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PlayniteServicesTests.Controllers.IGDB
{
    [Collection("DefaultCollection")]
    public class ControllerTests 
    {
        private readonly HttpClient client;

        public ControllerTests(TestFixture<PlayniteServices.Startup> fixture)
        {
            client = fixture.Client;
        }

        [Fact]
        public async Task CompanyControllerTest()
        {
            var response = await client.GetAsync("/igdb/company/2");
            var validResponse = JsonConvert.DeserializeObject<ServicesResponse<Company>>(await response.Content.ReadAsStringAsync());
            Assert.True(string.IsNullOrEmpty(validResponse.Error));
            Assert.False(string.IsNullOrEmpty(validResponse.Data.name));

            response = await client.GetAsync("/igdb/company/2?apikey=" + PlayniteServices.Controllers.IGDB.IGDB.ApiKey);
            validResponse = JsonConvert.DeserializeObject<ServicesResponse<Company>>(await response.Content.ReadAsStringAsync());
            Assert.True(string.IsNullOrEmpty(validResponse.Error));
            Assert.False(string.IsNullOrEmpty(validResponse.Data.name));
        }

        [Fact]
        public async Task GenreControllerTest()
        {
            var response = await client.GetAsync("/igdb/genre/2");
            var validResponse = JsonConvert.DeserializeObject<ServicesResponse<Genre>>(await response.Content.ReadAsStringAsync());
            Assert.True(string.IsNullOrEmpty(validResponse.Error));
            Assert.False(string.IsNullOrEmpty(validResponse.Data.name));

            response = await client.GetAsync("/igdb/genre/2?apikey=" + PlayniteServices.Controllers.IGDB.IGDB.ApiKey);
            validResponse = JsonConvert.DeserializeObject<ServicesResponse<Genre>>(await response.Content.ReadAsStringAsync());
            Assert.True(string.IsNullOrEmpty(validResponse.Error));
            Assert.False(string.IsNullOrEmpty(validResponse.Data.name));
        }

        [Fact]
        public async Task GameControllerTest()
        {
            var response = await client.GetAsync("/igdb/game/2");
            var validResponse = JsonConvert.DeserializeObject<ServicesResponse<Game>>(await response.Content.ReadAsStringAsync());
            Assert.True(string.IsNullOrEmpty(validResponse.Error));
            Assert.False(string.IsNullOrEmpty(validResponse.Data.name));
            Assert.False(string.IsNullOrEmpty(validResponse.Data.summary));
            Assert.NotEqual(0, validResponse.Data.first_release_date);

            response = await client.GetAsync("/igdb/game/2?apikey=" + PlayniteServices.Controllers.IGDB.IGDB.ApiKey);
            validResponse = JsonConvert.DeserializeObject<ServicesResponse<Game>>(await response.Content.ReadAsStringAsync());
            Assert.True(string.IsNullOrEmpty(validResponse.Error));
            Assert.False(string.IsNullOrEmpty(validResponse.Data.name));
            Assert.False(string.IsNullOrEmpty(validResponse.Data.summary));
            Assert.NotEqual(0, validResponse.Data.first_release_date);
        }

        [Fact]
        public async Task GameParsedControllerTest()
        {
            var response = await client.GetAsync("/igdb/game_parsed/2");
            var validResponse = JsonConvert.DeserializeObject<ServicesResponse<ParsedGame>>(await response.Content.ReadAsStringAsync());
            Assert.True(string.IsNullOrEmpty(validResponse.Error));
            Assert.False(string.IsNullOrEmpty(validResponse.Data.name));
            Assert.False(string.IsNullOrEmpty(validResponse.Data.summary));
            Assert.NotEqual(0, validResponse.Data.first_release_date);

            response = await client.GetAsync("/igdb/game_parsed/2?apikey=" + PlayniteServices.Controllers.IGDB.IGDB.ApiKey);
            validResponse = JsonConvert.DeserializeObject<ServicesResponse<ParsedGame>>(await response.Content.ReadAsStringAsync());
            Assert.True(string.IsNullOrEmpty(validResponse.Error));
            Assert.False(string.IsNullOrEmpty(validResponse.Data.name));
            Assert.False(string.IsNullOrEmpty(validResponse.Data.summary));
            Assert.NotEqual(0, validResponse.Data.first_release_date);
        }

        [Fact]
        public async Task GamesControllerTest()
        {
            var response = await client.GetAsync("/igdb/games/quake");
            var validResponse = JsonConvert.DeserializeObject<ServicesResponse<List<Game>>>(await response.Content.ReadAsStringAsync());
            Assert.NotEmpty(validResponse.Data);
            var game = validResponse.Data[0];
            Assert.NotEqual(UInt64.MinValue, game.id);
            Assert.False(string.IsNullOrEmpty(game.name));

            response = await client.GetAsync("/igdb/games/quake?apikey=" + PlayniteServices.Controllers.IGDB.IGDB.ApiKey);
            validResponse = JsonConvert.DeserializeObject<ServicesResponse<List<Game>>>(await response.Content.ReadAsStringAsync());
            Assert.NotEmpty(validResponse.Data);
            game = validResponse.Data[0];
            Assert.NotEqual(UInt64.MinValue, game.id);
            Assert.False(string.IsNullOrEmpty(game.name));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using PlayniteServices.Controllers;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayniteServices;
using PlayniteServices.Models.Steam;

namespace PlayniteServicesTests.Controllers.Steam
{
    [Collection("DefaultCollection")]
    public class LibraryControllerTests
    {
        private readonly HttpClient client;

        public LibraryControllerTests(TestFixture<PlayniteServices.Startup> fixture)
        {
            client = fixture.Client;
        }

        [Fact]
        public async Task GetLibraryTest()
        {
            var response = await client.GetAsync("/steam/library/nonexistinguser1234");
            var errorResponse = JsonConvert.DeserializeObject<GenericResponse>(await response.Content.ReadAsStringAsync());
            Assert.True(!string.IsNullOrEmpty(errorResponse.Error));
            Assert.Null(errorResponse.Data);

            response = await client.GetAsync("/steam/library/playnitedb");
            var validResponse = JsonConvert.DeserializeObject<ServicesResponse<List<GetOwnedGamesResult.Game>>>(await response.Content.ReadAsStringAsync());
            Assert.True(string.IsNullOrEmpty(validResponse.Error));
            Assert.True(validResponse.Data.Count > 0);

            response = await client.GetAsync("/steam/library/76561198358889790");
            validResponse = JsonConvert.DeserializeObject<ServicesResponse<List<GetOwnedGamesResult.Game>>>(await response.Content.ReadAsStringAsync());
            Assert.True(string.IsNullOrEmpty(validResponse.Error));
            Assert.True(validResponse.Data.Count > 0);
        }
    }
}

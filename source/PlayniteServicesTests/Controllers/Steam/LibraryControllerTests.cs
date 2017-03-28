using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using PlayniteServices.Controllers;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PlayniteServices;
using PlayniteServices.Controllers.Steam;
using static PlayniteServices.Controllers.Steam.LibraryController;

namespace PlayniteServicesTests.Controllers.Steam
{
    public class LibraryControllerTests : IClassFixture<TestFixture<PlayniteServices.Startup>>
    {
        private readonly HttpClient client;

        public LibraryControllerTests(TestFixture<PlayniteServices.Startup> fixture)
        {
            client = fixture.Client;
        }

        [Fact]
        public async Task GetLibraryTest()
        {
            var response = await client.GetAsync("/api/steam/library/nonexistinguser1234");
            var errorResponse = JsonConvert.DeserializeObject<GenericResponse>(await response.Content.ReadAsStringAsync());
            Assert.True(!string.IsNullOrEmpty(errorResponse.Error));
            Assert.Null(errorResponse.Data);

            response = await client.GetAsync("/api/steam/library/playnitedb");
            var validResponse = JsonConvert.DeserializeObject<ServicesResponse<List<GetOwnedGamesResult.Game>>>(await response.Content.ReadAsStringAsync());
            Assert.True(string.IsNullOrEmpty(validResponse.Error));            
            Assert.True(validResponse.Data.Count > 0);
        }
    }
}

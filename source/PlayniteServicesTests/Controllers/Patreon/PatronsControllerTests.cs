using Newtonsoft.Json;
using PlayniteServices;
using PlayniteServices.Models.Patreon;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PlayniteServicesTests.Controllers.Patreon
{
    [Collection("DefaultCollection")]
    public class PatronsControllerTests
    {
        private readonly HttpClient client;

        public PatronsControllerTests(TestFixture<Startup> fixture)
        {
            client = fixture.Client;
        }

        [Fact]
        public async Task CompanyControllerTest()
        {
            var response = await client.GetAsync("api/patreon/patrons");
            var validResponse = JsonConvert.DeserializeObject<ServicesResponse<List<string>>>(await response.Content.ReadAsStringAsync());
            Assert.NotEmpty(validResponse.Data);
            Assert.True(string.IsNullOrEmpty(validResponse.Error));
        }
    }
}

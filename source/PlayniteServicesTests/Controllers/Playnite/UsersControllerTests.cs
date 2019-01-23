using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PlayniteServicesTests.Controllers.Playnite
{
    [Collection("DefaultCollection")]
    public class UsersControllerTests
    {
        private readonly HttpClient client;

        public UsersControllerTests(TestFixture<PlayniteServices.Startup> fixture)
        {
            client = fixture.Client;
        }

        [Fact]
        public async Task PostUserTest()
        {
            var user = new PlayniteServices.Models.Playnite.User()
            {
                Id = "testId",
                WinVersion = "windversion",
                PlayniteVersion = "1.0"
            };

            var content = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(@"/playnite/users", content);
            Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

            // TODO add db check
        }
    }
}

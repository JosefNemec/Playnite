using Newtonsoft.Json;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Services
{
    public class BaseServicesClient
    {
        private static ILogger logger = LogManager.GetLogger();

        public readonly string Endpoint;

        public HttpClient HttpClient = new HttpClient()
        {
            Timeout = new TimeSpan(0, 0, 30)
        };

        public BaseServicesClient(string endpoint)
        {
            Endpoint = endpoint.TrimEnd('/');
        }

        public T ExecuteGetRequest<T>(string subUrl)
        {
            var url = Uri.EscapeUriString(Endpoint + subUrl);
            var strResult = HttpClient.GetStringAsync(url).GetAwaiter().GetResult();
            var result = JsonConvert.DeserializeObject<ServicesResponse<T>>(strResult);

            if (!string.IsNullOrEmpty(result.Error))
            {
                logger.Error("Service request error by proxy: " + result.Error);
                throw new Exception(result.Error);
            }

            return result.Data;
        }
    }
}

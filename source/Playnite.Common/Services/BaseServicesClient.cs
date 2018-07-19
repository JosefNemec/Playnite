using Newtonsoft.Json;
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
        public readonly string Endpoint;
        private static HttpClient httpClient = new HttpClient()
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
            var strResult = httpClient.GetStringAsync(url).GetAwaiter().GetResult();
            var result = JsonConvert.DeserializeObject<ServicesResponse<T>>(strResult);

            if (!string.IsNullOrEmpty(result.Error))
            {
                throw new Exception(result.Error);
            }

            return result.Data;
        }
    }
}

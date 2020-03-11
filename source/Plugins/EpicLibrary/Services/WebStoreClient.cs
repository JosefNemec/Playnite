using EpicLibrary.Models;
using Playnite.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary.Services
{
    public class WebStoreClient : IDisposable
    {
        private HttpClient httpClient = new HttpClient();

        public const string GraphQLEndpoint = @"https://graphql.epicgames.com/graphql";
        public const string ProductUrlBase = @"https://store-content.ak.epicgames.com/api/en-US/content/products/{0}";

        public WebStoreClient()
        {
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }

        public async Task<List<WebStoreModels.QuerySearchResponse.CatalogOfferElemen>> QuerySearch(string searchTerm)
        {
            var query = new WebStoreModels.QuerySearch();
            query.variables.query = searchTerm;
            var content = new StringContent(Serialization.ToJson(query), Encoding.UTF8, "application/json");
            var response = await httpClient.PostAsync(GraphQLEndpoint, content);
            var str = await response.Content.ReadAsStringAsync();
            var data = Serialization.FromJson<WebStoreModels.QuerySearchResponse>(str);
            return data.data.Catalog.catalogOffers.elements;
        }

        public async Task<WebStoreModels.ProductResponse> GetProductInfo(string productSlug)
        {
            var slugUri = productSlug.Split('/').First();
            var productUrl = string.Format(ProductUrlBase, slugUri);
            var str = await httpClient.GetStringAsync(productUrl);
            return Serialization.FromJson<WebStoreModels.ProductResponse>(str);
        }
    }
}

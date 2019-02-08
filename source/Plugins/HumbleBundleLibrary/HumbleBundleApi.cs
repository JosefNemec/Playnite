using HumbleBundleLibrary.Models;
using Playnite.SDK;
using System.Collections.Generic;
using System.Linq;

namespace HumbleBundleLibrary
{
    class HumbleBundleApi
    {
        private readonly IWebView webView;

        public HumbleBundleApi(IWebView webView)
        {
            this.webView = webView;
        }

        const string ordersUrl = "https://www.humblebundle.com/api/v1/user/order";

        private IEnumerable<string> GetOrderIds()
        {
            this.webView.NavigateAndWait(ordersUrl);
            if (!this.webView.GetCurrentAddress().Equals(ordersUrl))
                return new string[0];

            var json = this.webView.GetPageText();
            var summary = Newtonsoft.Json.JsonConvert.DeserializeObject<Summary>(json);
            return from info in summary
                   select info.GameKey;
        }

        private Order GetOrder(string orderId)
        {
            var orderUrl = string.Format("https://www.humblebundle.com/api/v1/order/{0}?all_tpkds=true", orderId);
            this.webView.NavigateAndWait(orderUrl);

            var json = this.webView.GetPageText();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(json);
        }

        public IEnumerable<Order> GetOrders()
        {
            foreach (var orderId in this.GetOrderIds())
            {
                yield return this.GetOrder(orderId);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Models.Patreon
{
    public class User
    {
        public string id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string full_name { get; set; }
        public string vanity { get; set; }
        public string email { get; set; }
        public string about { get; set; }
        public string facebook_id { get; set; }
        public string image_url { get; set; }
        public string thumb_url { get; set; }
        public string youtube { get; set; }
        public string twitter { get; set; }
        public string facebook { get; set; }
        public DateTime? created { get; set; }
        public string url { get; set; }
    }

    public class Pledge
    {
        public string id { get; set; }
        public User patron { get; set; }
        public int amount_cents { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? declined_since { get; set; }
        public int pledge_cap_cents { get; set; }
        public bool patron_pays_fees { get; set; }
        public int total_historical_amount_cents { get; set; }
        public bool is_paused { get; set; }
        public bool has_shipping_address { get; set; }
        public int outstanding_payment_amount_cents { get; set; }
    }
}
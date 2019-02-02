using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlayniteServices.Models.Patreon
{
    public class User
    {
        public string id;
        public string first_name;
        public string last_name;
        public string full_name;
        public string vanity;
        public string email;
        public string about;
        public string facebook_id;
        public string image_url;
        public string thumb_url;
        public string youtube;
        public string twitter;
        public string facebook;
        public DateTime? created;
        public string url;
    }

    public class Pledge
    {
        public string id;
        public User patron;
        public int amount_cents;
        public DateTime? created_at;
        public DateTime? declined_since;
        public int pledge_cap_cents;
        public bool patron_pays_fees;
        public int total_historical_amount_cents;
        public bool is_paused;
        public bool has_shipping_address;
        public int outstanding_payment_amount_cents;
    }
}

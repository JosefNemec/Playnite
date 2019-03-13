using System;

namespace DiscordLibrary.Models
{
    public class Sku
    {
        public DateTime release_date;
        public string application_id;
        public string name;
        public string slug;
        public int content_rating_agency;
        public FeatureType[] features;
        public string id;
        public int flags;
        public Price price;
        public ContentRating content_rating;
        public Genre[] genres;
        public Application application;
    }

    public class Price
    {
        public string currency;
        public double amount;
    }

    public class ContentRating
    {
        public int rating;
        public int[] descriptors;
    }
}

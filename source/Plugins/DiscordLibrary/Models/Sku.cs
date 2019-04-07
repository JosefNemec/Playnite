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
        public string id;
        public int flags;
        public Price price;
        public ContentRating content_rating;
        public Application application;

        // It would be nice to expose these as metadata, but the enum definition can't be created automatically
        //public FeatureType[] features;
        //public Genre[] genres;
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

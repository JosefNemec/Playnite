using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary.Models
{
    public class WebStoreModels
    {
        public class QuerySearch
        {
            public class Variables
            {
                public string @namespace = "epic";
                public string locale = "en-US";
                public string country = "US";
                public string query;
            }

            public Variables variables = new Variables();
            public string query = @"query searchQuery($namespace: String!,$locale: String!,$country: String!,$query: String!,$hasCountryFilter: Boolean,$filterCountry: String,$filterAgeGroup: Int){ Catalog {catalogOffers(namespace: $namespace, locale: $locale, params: {  keywords: $query,  country: $country}, countryAgeFilter: {shouldCheck: $hasCountryFilter,filterCountry: $filterCountry,filterAgeGroup: $filterAgeGroup}) { elements { url title id  productSlug categories { path }}}}}";
        }

        public class QuerySearchResponse
        {
            public class CatalogOfferElemen
            {
                public string url;
                public string title;
                public string id;
                public string productSlug;
            }

            public class Data
            {
                public class CatalogItem
                {
                    public class CatalogOffer
                    {
                        public List<CatalogOfferElemen> elements;
                    }

                    public CatalogOffer catalogOffers;
                }

                public CatalogItem Catalog;
            }

            public Data data;
        }

        public class ProductResponse
        {
            public class PageData
            {
                public class About
                {
                    public string developerAttribution;
                    public string description;
                    public string title;
                }

                public class Hero
                {
                    public string portraitBackgroundImageUrl;
                    public string backgroundImageUrl;
                }

                public Dictionary<string, string> socialLinks;
                public About about;
                public Hero hero;
            }

            public class Page
            {
                public string @namespace;
                public string _title;
                public string regionBlock;
                public string productName;
                public string _urlPattern;
                public string _slug;
                public DateTime? _activeDate;
                public DateTime? lastModified;
                public string _locale;
                public string _id;
                public PageData data;
            }

            public string @namespace;
            public string _title;
            public string regionBlock;
            public string productName;
            public string _urlPattern;
            public string _slug;
            public DateTime? _activeDate;
            public DateTime? lastModified;
            public string _locale;
            public string _id;
            public List<Page> pages;
        }
    }
}

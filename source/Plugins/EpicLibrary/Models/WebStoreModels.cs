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
                public string locale = "en-US";
                public string country = "US";
                public string allowCountries = "US";
                public string sortBy = "title";
                public string sortDir = "DESC";
                public string category = "games/edition/base|bundles/games|editors";
                public string keywords;
            }

            public Variables variables = new Variables();
            public string query = @"query searchStoreQuery($allowCountries: String, $category: String, $count: Int, $country: String!, $keywords: String, $locale: String, $namespace: String, $itemNs: String, $sortBy: String, $sortDir: String, $start: Int, $tag: String, $releaseDate: String, $withPrice: Boolean = false, $withPromotions: Boolean = false) {  Catalog {    searchStore(allowCountries: $allowCountries, category: $category, count: $count, country: $country, keywords: $keywords, locale: $locale, namespace: $namespace, itemNs: $itemNs, sortBy: $sortBy, sortDir: $sortDir, releaseDate: $releaseDate, start: $start, tag: $tag) {      elements {        title        id        namespace        description        effectiveDate        keyImages {          type          url        }        seller {          id          name        }        productSlug        urlSlug        url        items {          id          namespace        }        customAttributes {          key          value        }        categories {          path        }        price(country: $country) @include(if: $withPrice) {          totalPrice {            discountPrice            originalPrice            voucherDiscount            discount            currencyCode            currencyInfo {              decimals            }            fmtPrice(locale: $locale) {              originalPrice              discountPrice              intermediatePrice            }          }          lineOffers {            appliedRules {              id              endDate              discountSetting {                discountType              }            }          }        }        promotions(category: $category) @include(if: $withPromotions) {          promotionalOffers {            promotionalOffers {              startDate              endDate              discountSetting {                discountType                discountPercentage              }            }          }          upcomingPromotionalOffers {            promotionalOffers {              startDate              endDate              discountSetting {                discountType                discountPercentage              }            }          }        }      }      paging {        count        total      }    }  }}";
        }

        public class QuerySearchResponse
        {
            public class SearchStoreElement
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
                    public class SearchStore
                    {
                        public List<SearchStoreElement> elements;
                    }

                    public SearchStore searchStore;
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
                public string tag;
                public string type;
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

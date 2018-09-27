using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GogLibrary.Models
{
    public class StorePageResult
    {
        public class ProductDetails
        {
            public class Feature
            {
                public string title;
                public string slug;
            }

            public class SluggedName
            {
                public string name;
                public string slug;
            }

            public List<SluggedName> genres;
            public List<Feature> features;
            public SluggedName publisher;
            public SluggedName developer;
            public int? releaseDate;
            public int id;
            public int rating;
        }

        public ProductDetails gameProductData;
    }
}

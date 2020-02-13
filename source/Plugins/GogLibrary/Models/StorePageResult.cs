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
            public class SluggedName
            {
                public string name;
                public string slug;
            }

            public class Feature
            {
                public string name;
                public string id;
            }

            public List<SluggedName> genres;
            public List<SluggedName> tags;
            public List<Feature> features;
            public string publisher;
            public List<SluggedName> developers;
            public DateTime? globalReleaseDate;
            public string id;
            public string galaxyBackgroundImage;
            public string backgroundImage;
            public string image;
        }

        public ProductDetails cardProduct;
    }
}

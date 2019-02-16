using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpicLibrary.Models
{
    public class CatalogItem
    {
        public class CustomAttribute
        {
            public string type;
            public string value;
        }

        public class Image
        {
            public string url;
            public string type;
        }

        public class Category
        {
            public string path;
        }

        public string id;
        public string title;
        public string longDescription;
        public string description;
        public string @namespace;
        public string status;
        public DateTime? creationDate;
        public string entitlementName;
        public string entitlementType;
        public string itemType;
        public string developerId;
        public Dictionary<string, CustomAttribute> customAttributes;
        public List<Category> categories;
    }
}

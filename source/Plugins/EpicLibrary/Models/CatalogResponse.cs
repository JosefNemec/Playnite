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

        public class ReleaseInfo
        {
            public string appId;
            public List<string> platform;
            public DateTime? dateAdded;
        }

        public string id;
        public string title;
        public string description;
        public List<Image> keyImages;
        public List<Category> categories;
        public string @namespace;
        public string status;
        public DateTime? creationDate;
        public DateTime? lastModifiedDate;
        public Dictionary<string, CustomAttribute> customAttributes;
        public string entitlementName;
        public string entitlementType;
        public string itemType;
        public List<ReleaseInfo> releaseInfo;
        public string developer;
        public string developerId;
        public bool endOfSupport;
    }


}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HumbleLibrary.Models
{
    public class Order
    {
        public class Product
        {
            public string category;
            public string machine_name;
            public string human_name;
        }

        public class SubProduct
        {
            public class Download
            {
                public class DownloadStruct
                {
                    public class Url
                    {
                        public string web;
                        public string bittorrent;
                    }

                    public string human_size;
                    public string name;
                    public string sha1;
                    public ulong file_size;
                    public string md5;
                    public Url url;
                }

                public List<DownloadStruct> download_struct;
                public string machine_name;
                public string platform;
                public bool android_app_only;
            }

            public string machine_name;
            public string url;
            public List<Download> downloads;
            public string human_name;
            public string icon;
            public string library_family_name;
        }

        public class TpkdDict
        {
            public class Tpk
            {
                public string machine_name;
                public string gamekey;
                public string key_type;
                public bool visible;
                public string instructions_html;
                public string key_type_human_name;
                public string human_name;
                public string @class;
                public string library_family_name;
            }

            public List<Tpk> all_tpks;
        }

        public string gamekey;
        public string uid;
        public Product product;
        public List<SubProduct> subproducts;
        public TpkdDict tpkd_dict;
        public List<string> path_ids;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Playnite
{
    public class Web
    {
        public static string DownloadString(string url)
        {
            var webClient = new WebClient();
            return webClient.DownloadString(url);
        }

        public static void DownloadString(string url, string path)
        {
            var webClient = new WebClient();
            var data = webClient.DownloadString(url);
            File.WriteAllText(path, data);
        }

        public static byte[] DownloadData(string url)
        {
            var webClient = new WebClient();
            return webClient.DownloadData(url);
        }

        public static void DownloadFile(string url, string path)
        {
            var webClient = new WebClient();
            webClient.DownloadFile(url, path);
        }
    }
}

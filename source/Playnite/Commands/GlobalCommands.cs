using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.Commands
{
    public static class GlobalCommands
    {
        public static RelayCommand<object> NavigateUrlCommand
        {
            get => new RelayCommand<object>((url) =>
            {
                NavigateUrl(url);
            });
        }

        public static void NavigateUrl(object url)
        {
            if (url is string stringUrl)
            {
                NavigateUrl(stringUrl);
            }
            if (url is Link linkUrl)
            {
                NavigateUrl(linkUrl.Url);
            }
            else if (url is Uri uriUrl)
            {
                NavigateUrl(uriUrl.OriginalString);
            }
            else
            {
                throw new Exception("Unsupported URL format.");
            }
        }

        public static void NavigateUrl(string url)
        {
            if (!Regex.IsMatch(url, @"^.*:\/\/"))
            {
                url = "http://" + url;
            }

            ProcessStarter.StartUrl(url);
        }
    }
}

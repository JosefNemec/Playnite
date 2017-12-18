using NLog;
using Playnite;
using Playnite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUI.Commands
{
    public static class GeneralCommands
    {
        public static RelayCommand<string> NavigateUrlCommand
        {
            get => new RelayCommand<string>((url) =>
            {
                NavigateUrl(url);
            });
        }

        public static void NavigateUrl(string url)
        {
            System.Diagnostics.Process.Start(url);
        }
    }
}

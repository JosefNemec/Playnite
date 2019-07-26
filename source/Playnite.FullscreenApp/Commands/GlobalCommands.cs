using Playnite.Commands;
using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.FullscreenApp
{
    public class NavigateUrlCommand : RelayCommand<object>
    {
        public NavigateUrlCommand() : base(Navigate)
        {            
        }

        private static void Navigate(object link)
        {
            if (FullscreenApplication.Current.Dialogs.ShowMessage(
                string.Format(ResourceProvider.GetString("LOCUrlNavigationMessage"), link.ToString()),
                string.Empty,
                MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                GlobalCommands.NavigateUrl(link);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.DesktopApp.ViewModels;
using Playnite.DesktopApp.Windows;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace Playnite.DesktopApp
{
    public class DesktopActionSelector : IActionSelector
    {
        public object SelectPlayAction(List<PlayController> controllers, List<GameAction> actions)
        {
            return new ActionSelectionViewModel(new ActionSelectionWindowFactory()).SelectPlayAction(controllers, actions);
        }

        public InstallController SelectInstallAction(List<InstallController> pluginActions)
        {
            return new ActionSelectionViewModel(new ActionSelectionWindowFactory()).SelectInstallAction(pluginActions);
        }

        public UninstallController SelectUninstallAction(List<UninstallController> pluginActions)
        {
            return new ActionSelectionViewModel(new ActionSelectionWindowFactory()).SelectUninstallAction(pluginActions);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.FullscreenApp.ViewModels;
using Playnite.FullscreenApp.Windows;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;

namespace Playnite.FullscreenApp
{
    public class FullscreenActionSelector : IActionSelector
    {
        public object SelectPlayAction(List<PlayController> controllers, List<GameAction> actions)
        {
            var allActions = new List<NamedObject<object>>();
            if (controllers.HasItems())
            {
                allActions.AddRange(controllers.Select(a => new NamedObject<object>(a, a.Name)));
            }

            if (actions.HasItems())
            {
                allActions.AddRange(actions.Select(a => new NamedObject<object>(a, a.Name)));
            }

            return new SingleItemSelectionViewModel<object>(
                new SingleItemSelectionWindowFactory(),
                ResourceProvider.GetString(LOC.SelectActionTitle)).
                SelectItem(allActions);
        }

        public InstallController SelectInstallAction(List<InstallController> pluginActions)
        {
            return new SingleItemSelectionViewModel<InstallController>(
                new SingleItemSelectionWindowFactory(),
                ResourceProvider.GetString(LOC.SelectActionTitle)).
                SelectItem(pluginActions.Select(a => new NamedObject<InstallController>(a, a.Name)).ToList());
        }

        public UninstallController SelectUninstallAction(List<UninstallController> pluginActions)
        {
            return new SingleItemSelectionViewModel<UninstallController>(
                new SingleItemSelectionWindowFactory(),
                ResourceProvider.GetString(LOC.SelectActionTitle)).
                SelectItem(pluginActions.Select(a => new NamedObject<UninstallController>(a, a.Name)).ToList());
        }
    }
}

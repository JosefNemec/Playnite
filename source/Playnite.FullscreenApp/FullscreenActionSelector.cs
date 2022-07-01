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
            var allActions = new List<SelectableNamedObject<object>>();
            if (controllers.HasItems())
            {
                allActions.AddRange(controllers.Select(a => new SelectableNamedObject<object>(a, a.Name)));
            }

            if (actions.HasItems())
            {
                allActions.AddRange(actions.Select(a => new SelectableNamedObject<object>(a, a.Name)));
            }

            ItemSelector.SelectSingle(LOC.SelectActionTitle, "", allActions, out var selectedItem);
            return selectedItem;
        }

        public InstallController SelectInstallAction(List<InstallController> pluginActions)
        {
            ItemSelector.SelectSingle(LOC.SelectActionTitle, "", pluginActions.Select(a => new SelectableNamedObject<InstallController>(a, a.Name)).ToList(), out var selectedItem);
            return selectedItem;
        }

        public UninstallController SelectUninstallAction(List<UninstallController> pluginActions)
        {
            ItemSelector.SelectSingle(LOC.SelectActionTitle, "", pluginActions.Select(a => new SelectableNamedObject<UninstallController>(a, a.Name)).ToList(), out var selectedItem);
            return selectedItem;
        }
    }
}

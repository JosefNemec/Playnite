using Playnite.Common;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using Playnite.Services;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Playnite.DesktopApp.ViewModels
{
    public class ActionSelectionViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;

        public List<SelectableItem<object>> Actions { get; set; }
        public object SelectedAction { get; set; }

        public RelayCommand<object> PlayCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectedAction = Actions.FirstOrDefault(i => i.Selected == true)?.Item;
                window.Close(true);
            });
        }

        public RelayCommand<object> PlaySpecificCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SelectedAction = a;
                window.Close(true);
            });
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                window.Close(null);
            });
        }

        public ActionSelectionViewModel(IWindowFactory window)
        {
            this.window = window;
        }

        public object SelectPlayAction(List<PlayController> controllers, List<GameAction> actions)
        {
            if (!actions.HasItems() && !controllers.HasItems())
            {
                throw new ArgumentNullException("Not actions provided!");
            }

            Actions = new List<SelectableItem<object>>();
            if (controllers.HasItems())
            {
                Actions.AddRange(controllers.Select(a => new SelectableItem<object>(a)));
            }

            if (actions.HasItems())
            {
                Actions.AddRange(actions.Select(a => new SelectableItem<object>(a)));
            }

            Actions[0].Selected = true;
            if (window.CreateAndOpenDialog(this) == true)
            {
                return SelectedAction;
            }
            else
            {
                return null;
            }
        }

        public InstallController SelectInstallAction(List<InstallController> pluginActions)
        {
            if (!pluginActions.HasItems())
            {
                throw new ArgumentNullException("Not install action provided!");
            }

            Actions = pluginActions.Select(a => new SelectableItem<object>(a)).ToList();
            Actions[0].Selected = true;
            if (window.CreateAndOpenDialog(this) == true)
            {
                return (InstallController)SelectedAction;
            }
            else
            {
                return null;
            }
        }

        public UninstallController SelectUninstallAction(List<UninstallController> pluginActions)
        {
            if (!pluginActions.HasItems())
            {
                throw new ArgumentNullException("Not uninstall action provided!");
            }

            Actions = pluginActions.Select(a => new SelectableItem<object>(a)).ToList();
            Actions[0].Selected = true;
            if (window.CreateAndOpenDialog(this) == true)
            {
                return (UninstallController)SelectedAction;
            }
            else
            {
                return null;
            }
        }
    }
}

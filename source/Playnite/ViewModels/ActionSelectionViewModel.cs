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

namespace Playnite.ViewModels
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

        public ActionSelectionViewModel(
            ActionSelectionWindowFactory window,
            List<PlayController> pluginActions,
            List<GameAction> customActions)
        {
            this.window = window;
            Actions = new List<SelectableItem<object>>();
            pluginActions?.ForEach(a => Actions.Add(new SelectableItem<object>(a)));
            customActions?.ForEach(a => Actions.Add(new SelectableItem<object>(a)));
            Actions[0].Selected = true;
        }

        public ActionSelectionViewModel(
            ActionSelectionWindowFactory window,
            List<InstallController> pluginActions)
        {
            this.window = window;
            Actions = new List<SelectableItem<object>>();
            pluginActions?.ForEach(a => Actions.Add(new SelectableItem<object>(a)));
            Actions[0].Selected = true;
        }

        public ActionSelectionViewModel(
            ActionSelectionWindowFactory window,
            List<UninstallController> pluginActions)
        {
            this.window = window;
            Actions = new List<SelectableItem<object>>();
            pluginActions?.ForEach(a => Actions.Add(new SelectableItem<object>(a)));
            Actions[0].Selected = true;
        }

        public object SelectAction()
        {
            if (window.CreateAndOpenDialog(this) == true)
            {
                return SelectedAction;
            }
            else
            {
                return null;
            }
        }
    }
}

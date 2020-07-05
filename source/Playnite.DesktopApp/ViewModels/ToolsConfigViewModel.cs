using Playnite.Common;
using Playnite.Common.Media.Icons;
using Playnite.Database;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.ViewModels
{
    public class ToolsConfigViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private IDialogsFactory dialogs;
        private IResourceProvider resources;
        private GameDatabase database;

        public ObservableCollection<AppSoftware> EditingApps
        {
            get;
        }

        private AppSoftware selectedApp;
        public AppSoftware SelectedApp
        {
            get => selectedApp;
            set
            {
                selectedApp = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> AddAppCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddApp();
            });
        }

        public RelayCommand<object> AddAppFromExeCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddAppFromExe();
            });
        }

        public RelayCommand<AppSoftware> RemoveAppCommand
        {
            get => new RelayCommand<AppSoftware>((a) =>
            {
                RemoveApp(a);
            }, (_) => SelectedApp != null);
        }

        public RelayCommand<AppSoftware> SelectIconCommand
        {
            get => new RelayCommand<AppSoftware>((a) =>
            {
                SelectIcon();
            }, (_) => SelectedApp != null);
        }

        public RelayCommand<AppSoftware> RemoveIconCommand
        {
            get => new RelayCommand<AppSoftware>((a) =>
            {
                RemoveIcon();
            }, (_) => SelectedApp != null);
        }

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ConfirmDialog();
            });
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView();
            });
        }

        public ToolsConfigViewModel(
            GameDatabase database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources)
        {
            this.database = database;
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;

            EditingApps = database.SoftwareApps.GetClone().OrderBy(a => a.Name).ToObservable();
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView()
        {
            window.Close(false);
        }

        public void ConfirmDialog()
        {
            string addNewFile(string path, Guid parent)
            {
                var newPath = database.AddFile(path, parent);
                if (Paths.AreEqual(Path.GetDirectoryName(path), PlaynitePaths.TempPath))
                {
                    File.Delete(path);
                }

                return newPath;
            }

            database.SoftwareApps.BeginBufferUpdate();

            // Update modified in database
            foreach (var app in EditingApps.Where(a => database.SoftwareApps[a.Id] != null).ToList())
            {
                var dbItem = database.SoftwareApps.Get(app.Id);
                if (app.IsEqualJson(dbItem))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(app.Icon) && File.Exists(app.Icon))
                {
                    app.Icon = addNewFile(app.Icon, dbItem.Id);
                }

                database.SoftwareApps.Update(app);
            }

            // Remove deleted from database
            var removedItems = database.SoftwareApps.Where(a => EditingApps.FirstOrDefault(b => b.Id == a.Id) == null).ToList();
            database.SoftwareApps.Remove(removedItems);

            // Add new to database
            foreach (var addedItem in EditingApps.Where(a => database.SoftwareApps[a.Id] == null).ToList())
            {
                if (!string.IsNullOrEmpty(addedItem.Icon))
                {
                    addedItem.Icon = addNewFile(addedItem.Icon, addedItem.Id);
                }

                database.SoftwareApps.Add(addedItem);
            }

            database.SoftwareApps.EndBufferUpdate();
            window.Close(true);
        }

        private void AddApp()
        {
            var res = dialogs.SelectString("LOCEnterName", "LOCAddNewItem", "");
            if (res.Result && !res.SelectedString.IsNullOrEmpty())
            {
                if (EditingApps.Any(a => a.Name.Equals(res.SelectedString, StringComparison.InvariantCultureIgnoreCase)))
                {
                    dialogs.ShowErrorMessage("LOCItemAlreadyExists", "");
                }
                else
                {
                    EditingApps.Add(new AppSoftware(res.SelectedString));
                }
            }
        }

        private void AddAppFromExe()
        {
            var filePath = dialogs.SelectFile("*.exe,*.lnk|*.exe;*.lnk");
            if (!filePath.IsNullOrEmpty())
            {
                var program = Programs.GetProgramData(filePath);
                var app = new AppSoftware(program.Name)
                {
                    Path = program.Path,
                    Arguments = program.Arguments,
                    WorkingDir = program.WorkDir
                };

                if (!program.Icon.IsNullOrEmpty())
                {
                    if (program.Icon.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                    {
                        app.Icon = program.Icon;
                    }
                    else
                    {
                        var icoPath = Path.Combine(PlaynitePaths.TempPath, Guid.NewGuid() + ".ico");
                        if (IconExtractor.ExtractMainIconFromFile(program.Icon, icoPath))
                        {
                            app.Icon = icoPath;
                        }
                    }
                }

                EditingApps.Add(app);
                SelectedApp = app;
            }
        }

        private void RemoveApp(AppSoftware a)
        {
            EditingApps.Remove(a);
        }

        private void SelectIcon()
        {
            var iconPath = dialogs.SelectIconFile();
            if (string.IsNullOrEmpty(iconPath))
            {
                return;
            }

            if (iconPath.EndsWith("exe", StringComparison.OrdinalIgnoreCase))
            {
                var convertedPath = Path.Combine(PlaynitePaths.TempPath, Guid.NewGuid() + ".ico");
                if (IconExtractor.ExtractMainIconFromFile(iconPath, convertedPath))
                {
                    iconPath = convertedPath;
                }
                else
                {
                    iconPath = null;
                }
            }

            SelectedApp.Icon = iconPath;
        }

        private void RemoveIcon()
        {
            SelectedApp.Icon = null;
        }
    }
}

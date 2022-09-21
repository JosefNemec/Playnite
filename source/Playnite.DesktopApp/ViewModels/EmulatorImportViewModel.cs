using Playnite;
using Playnite.Database;
using Playnite.SDK.Models;
using Playnite.SDK;
using Playnite.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Playnite.Common;
using Playnite.Windows;
using Playnite.DesktopApp.Windows;
using Playnite.ViewModels;
using Playnite.Emulators;

namespace Playnite.DesktopApp.ViewModels
{
    public class EmulatorImportViewModel : ObservableObject
    {
        private readonly object listSyncLock = new object();
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private readonly IDialogsFactory dialogs;
        private readonly IResourceProvider resources;
        private readonly IGameDatabaseMain database;
        private readonly List<string> importedDirs;

        public List<ScannedEmulator> SelectedEmulators;

        private ObservableCollection<ScannedEmulator> emulatorList = new ObservableCollection<ScannedEmulator>();
        public ObservableCollection<ScannedEmulator> EmulatorList
        {
            get => emulatorList;
            set
            {
                emulatorList = value;
                OnPropertyChanged();
            }
        }

        private ListCollectionView collectionView;
        public ListCollectionView CollectionView
        {
            get => collectionView;
            private set
            {
                collectionView = value;
                OnPropertyChanged();
            }
        }

        private bool hideImported = true;
        public bool HideImported
        {
            get => hideImported;
            set
            {
                hideImported = value;
                OnPropertyChanged();
                CollectionView.Refresh();
            }
        }

        public RelayCommand CancelCommand =>
            new RelayCommand(() => CloseView(false));

        public RelayCommand ScanCommmand =>
            new RelayCommand(() => ScanEmulators());

        public RelayCommand ImportCommand =>
            new RelayCommand(() => ImportEmulators());

        public RelayCommand SelectAllCommmand =>
            new RelayCommand(
                () => EmulatorList.ForEach(e => e.Import = true),
                () => EmulatorList.HasItems());

        public RelayCommand DeselectAllCommmand =>
            new RelayCommand(
                () => EmulatorList.ForEach(e => e.Import = false),
                () => EmulatorList.HasItems());

        public EmulatorImportViewModel(
            IGameDatabaseMain database,
            IWindowFactory window,
            IDialogsFactory dialogs,
            IResourceProvider resources)
        {
            this.window = window;
            this.dialogs = dialogs;
            this.resources = resources;
            this.database = database;
            importedDirs = database.Emulators.Select(a => a.InstallDir).ToList();
            CollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(EmulatorList);
            CollectionView.Filter = ListFilter;
            BindingOperations.EnableCollectionSynchronization(EmulatorList, listSyncLock);
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void CloseView(bool? result)
        {
            window.Close(result);
        }

        private bool ListFilter(object item)
        {
            var emulator = (ScannedEmulator)item;
            if (HideImported)
            {
                return !importedDirs.ContainsString(emulator.InstallDir, StringComparison.OrdinalIgnoreCase);
            }

            return true;
        }

        public void ScanEmulators()
        {
            var dirToScan = dialogs.SelectFolder();
            if (dirToScan.IsNullOrEmpty())
            {
                return;
            }

            var scanRes = dialogs.ActivateGlobalProgress((args) =>
            {
                var emulators = EmulatorScanner.SearchForEmulators(dirToScan, Emulation.Definitions, args.CancelToken);
                window.Window.Dispatcher.Invoke(() => EmulatorList.AddRange(emulators));
            },
            new GlobalProgressOptions(LOC.EmuWizardScanning)
            {
                Cancelable = true,
                IsIndeterminate = true
            });

            if (scanRes.Error != null)
            {
                dialogs.ShowErrorMessage(LOC.EmulatorScanFailed + "\n" + scanRes.Error.Message, "");
            }
        }

        public void ImportEmulators()
        {
            SelectedEmulators = CollectionView.Cast<ScannedEmulator>().Where(a => a.Import).ToList();
            CloseView(true);
        }
    }
}

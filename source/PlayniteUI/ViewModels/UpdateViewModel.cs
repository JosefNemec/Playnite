using Playnite;
using Playnite.App;
using Playnite.SDK;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI.ViewModels
{
    public class UpdateViewModel : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private Updater updater;
        private IResourceProvider resources;
        private IDialogsFactory dialogs;
        private readonly SynchronizationContext context;

        private int updateProgress;
        public int UpdateProgress
        {
            get => updateProgress;
            set
            {
                updateProgress = value;
                OnPropertyChanged();
            }
        }

        private bool showProgress;
        public bool ShowProgress
        {
            get => showProgress;
            set
            {
                showProgress = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> CloseCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                CloseView();
            });
        }

        public RelayCommand<object> InstallUpdateCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                InstallUpdate();
            });
        }

        public List<ReleaseNoteData> ReleaseNotes
        {
            get;
            private set;
        }

        public static bool InstanceInUse
        {
            get; set;
        }

        public UpdateViewModel(Updater updater, IWindowFactory window, IResourceProvider resources, IDialogsFactory dialogs)
        {
            InstanceInUse = true;
            context = SynchronizationContext.Current;
            this.window = window;
            this.updater = updater;
            this.resources = resources;
            this.dialogs = dialogs;

            try
            {
                ReleaseNotes = updater.DownloadReleaseNotes(Updater.GetCurrentVersion());
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to download release notes.");
            }
        }

        public void OpenView()
        {
            window.CreateAndOpenDialog(this);
        }

        public void CloseView()
        {
            InstanceInUse = false;
            window.Close();
        }

        public async void InstallUpdate()
        {
            if (GlobalTaskHandler.IsActive)
            {
                if (dialogs.ShowMessage(resources.FindString("LOCUpdateProgressCancelAsk"), "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    var progressModel = new ProgressViewViewModel(new ProgressWindowFactory(), () =>
                    {
                        try
                        {
                            GlobalTaskHandler.CancelAndWait();
                        }
                        catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                        {
                            logger.Error(exc, "Failed to cancel global progress task.");
                            throw;
                        }
                    }, resources.FindString("LOCProgressReleasingResources"));
                    progressModel.ActivateProgress();                    
                }
                else
                {
                    window.Close();
                    return;
                }
            }

            try
            {
                ShowProgress = true;
                var package = updater.GetUpdatePackage(Updater.GetCurrentVersion());
                await updater.DownloadUpdate(package, (e) =>
                {
                    context.Post((a) => UpdateProgress = e.ProgressPercentage, null);
                });
                updater.InstallUpdate();
            }
            catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                ShowProgress = false;
                logger.Error(exc, "Failed to download and install update.");
                dialogs.ShowMessage(
                    resources.FindString("LOCGeneralUpdateFailMessage") + $"\n{exc.Message}",
                    resources.FindString("LOCUpdateError"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                window.Close();
                return;
            }
        }
    }
}

using NLog;
using Playnite;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI.ViewModels
{
    public class UpdateViewModel : ObservableObject
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IWindowFactory window;
        private Update update;
        private IResourceProvider resources;
        private IDialogsFactory dialogs;

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

        public List<Update.ReleaseNoteData> ReleaseNotes
        {
            get;
            private set;
        }

        public UpdateViewModel(Update update, IWindowFactory window, IResourceProvider resources, IDialogsFactory dialogs)
        {
            this.window = window;
            this.update = update;
            this.resources = resources;
            this.dialogs = dialogs;
            ReleaseNotes = update.LatestReleaseNotes.OrderBy(a => a.Version).ToList();
        }

        public void OpenView()
        {
            window.CreateAndOpenDialog(this);
        }

        public void CloseView()
        {
            window.Close();
        }

        public void InstallUpdate()
        {
            if (GlobalTaskHandler.IsActive)
            {
                if (dialogs.ShowMessage(resources.FindString("UpdateProgressCancelAsk"), "", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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
                    }, resources.FindString("ProgressReleasingResources"));

                    progressModel.ActivateProgress();
                    update.InstallUpdate();
                }
                else
                {
                    window.Close();
                }
            }
            else
            {
                update.InstallUpdate();
            }
        }
    }
}

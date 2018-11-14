using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayniteUI.ViewModels
{
    public class ProgressViewViewModel : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private Action progresAction;

        private string progressText;
        public string ProgressText
        {
            get => progressText;
            set
            {
                progressText = value;
                OnPropertyChanged();
            }
        }

        public Exception FailException { get; private set; }

        public ProgressViewViewModel(IWindowFactory window, Action progresAction)
        {
            this.window = window;
            this.progresAction = progresAction;
        }

        public ProgressViewViewModel(IWindowFactory window, Action progresAction, string text) : this(window, progresAction)
        {
            ProgressText = text;
        }

        public bool? ActivateProgress()
        {
            Task.Run(() =>
            {
                try
                {
                    progresAction();
                }
                catch (Exception exc)
                {
                    FailException = exc;
                    window.Close(false);
                    return;
                }

                window.Close(true);
            });
            return window.CreateAndOpenDialog(this);
        }

        public static bool? ActivateProgress(Action progresAction, string progressText)
        {
            var progressModel = new ProgressViewViewModel(new ProgressWindowFactory(), progresAction, progressText);
            return progressModel.ActivateProgress();
        }
    }
}

using NLog;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteUI.ViewModels
{
    public class ProgressViewViewModel : ObservableObject
    {
        private Action progresAction;

        private string progressText;
        public string ProgressText
        {
            get => progressText;
            set
            {
                progressText = value;
                OnPropertyChanged("ProgressText");
            }
        }

        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IWindowFactory window;

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
            Task.Factory.StartNew(progresAction).
                ContinueWith((a) =>
                {
                    if (a.Exception == null)
                    {
                        window.Close(true);
                    }
                    else
                    {
                        window.Close(false);
                    }
                });
            return window.CreateAndOpenDialog(this);
        }
    }
}

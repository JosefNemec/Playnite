using Playnite.SDK;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Playnite.ViewModels
{
    public class ProgressViewViewModel : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private Action<GlobalProgressActionArgs> progresAction;
        private GlobalProgressOptions args;
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private bool canCancel = false;

        private GlobalProgressActionArgs progressArgs;
        public GlobalProgressActionArgs ProgressArgs
        {
            get => progressArgs;
            set
            {
                progressArgs = value;
                OnPropertyChanged();
            }
        }

        private bool indeterminate = true;
        public bool Indeterminate
        {
            get => indeterminate;
            set
            {
                indeterminate = value;
                OnPropertyChanged();
            }
        }

        private bool cancelable;
        public bool Cancelable
        {
            get => cancelable;
            set
            {
                cancelable = value;
                OnPropertyChanged();
            }
        }

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

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                canCancel = false;
                cancellationToken.Cancel();
            }, (a) => Cancelable && canCancel);
        }

        public Exception FailException { get; private set; }

        public ProgressViewViewModel(IWindowFactory window, Action<GlobalProgressActionArgs> progresAction, GlobalProgressOptions args)
        {
            this.window = window;
            this.progresAction = progresAction;
            this.args = args;

            Cancelable = args.Cancelable;
            canCancel = Cancelable;
            if (args.Text?.StartsWith("LOC") == true)
            {
                ProgressText = ResourceProvider.GetString(args.Text);
            }
            else
            {
                ProgressText = args.Text;
            }
        }

        public GlobalProgressResult ActivateProgress()
        {
            Task.Run(() =>
            {
                try
                {
                    ProgressArgs = new GlobalProgressActionArgs(
                        PlayniteApplication.Current.SyncContext,
                        PlayniteApplication.CurrentNative.Dispatcher,
                        cancellationToken)
                    {
                        Text = ProgressText
                    };
                    progresAction(ProgressArgs);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    FailException = exc;
                    window.Close(false);
                    return;
                }

                window.Close(true);
            });

            var res = window.CreateAndOpenDialog(this);
            return new GlobalProgressResult(res, cancellationToken.IsCancellationRequested, FailException);
        }
    }

    public class GlobalProgress
    {
        public static GlobalProgressResult ActivateProgress(Action<GlobalProgressActionArgs> progresAction, GlobalProgressOptions progressArgs)
        {
            var progressModel = new ProgressViewViewModel(new ProgressWindowFactory(), progresAction, progressArgs)
            {
                Indeterminate = progressArgs.IsIndeterminate
            };
            return progressModel.ActivateProgress();
        }
    }
}

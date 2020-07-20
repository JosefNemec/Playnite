using Playnite.SDK;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.ViewModels
{
    public class ProgressViewViewModel : ObservableObject
    {
        private static ILogger logger = LogManager.GetLogger();
        private IWindowFactory window;
        private Action<ProgressActionArgs> progresAction;
        private ProgressViewArgs args;
        private CancellationTokenSource cancellationToken = new CancellationTokenSource();
        private bool canCancel = false;

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

        public ProgressViewViewModel(IWindowFactory window, Action<ProgressActionArgs> progresAction, ProgressViewArgs args)
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
                    progresAction(new ProgressActionArgs(cancellationToken));
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

    public class ProgressViewArgs
    {
        public string Text { get; set; }
        public bool Cancelable { get; set; }

        public ProgressViewArgs(string text)
        {
            Text = text;
        }

        public ProgressViewArgs(string text, bool cancelable) : this(text)
        {
            Cancelable = cancelable;
        }
    }

    public class ProgressActionArgs
    {
        public SynchronizationContext SyncContext => PlayniteApplication.Current.SyncContext;
        public CancellationTokenSource CancelToken { get; }

        public ProgressActionArgs(CancellationTokenSource cancelToken)
        {
            CancelToken = cancelToken;
        }
    }

    public class GlobalProgressResult
    {
        public Exception Error { get; set; }
        public bool? Result { get; set; }
        public bool Canceled { get; set; }

        public GlobalProgressResult(bool? result, bool canceled, Exception error)
        {
            Result = result;
            Error = error;
            Canceled = canceled;
        }
    }

    public class GlobalProgress
    {
        public static GlobalProgressResult ActivateProgress(Action<ProgressActionArgs> progresAction, ProgressViewArgs progressArgs)
        {
            var progressModel = new ProgressViewViewModel(new ProgressWindowFactory(), progresAction, progressArgs);
            return progressModel.ActivateProgress();
        }
    }
}

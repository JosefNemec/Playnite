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
        private CancellationTokenSource cancellationToken;
        private bool canCancel = false;
        private bool wasCancelled = false;
        private bool wasProcessed = false;

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
                wasCancelled = true;
                cancellationToken.Cancel();
            }, (a) => Cancelable && canCancel && !cancellationToken.IsCancellationRequested);
        }

        public Exception FailException { get; private set; }

        public ProgressViewViewModel(IWindowFactory window, Action<GlobalProgressActionArgs> progresAction, GlobalProgressOptions args)
        {
            this.window = window;
            this.progresAction = progresAction;
            this.args = args;

            cancellationToken = new CancellationTokenSource();
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
            if (wasProcessed)
            {
                throw new Exception("Progress can be shown only once per instance.");
            }

            Task.Run(() =>
            {
                try
                {
                    ProgressArgs = new GlobalProgressActionArgs(
                        PlayniteApplication.Current.SyncContext,
                        PlayniteApplication.CurrentNative.Dispatcher,
                        cancellationToken.Token)
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
                finally
                {
                    cancellationToken.Dispose();
                }

                window.Close(true);
            });

            var res = window.CreateAndOpenDialog(this);
            wasProcessed = true;
            return new GlobalProgressResult(res, wasCancelled, FailException);
        }

        public GlobalProgressResult ActivateProgress(int delay)
        {
            if (wasProcessed)
            {
                throw new Exception("Progress can be shown only once per instance.");
            }

            ProgressArgs = new GlobalProgressActionArgs(
                PlayniteApplication.Current.SyncContext,
                PlayniteApplication.CurrentNative.Dispatcher,
                cancellationToken.Token)
                {
                    Text = ProgressText
                };
            bool? res = null;
            var progressTask = Task.Run(() =>
            {
                try
                {
                    progresAction(ProgressArgs);
                }
                catch (Exception exc) when (!PlayniteEnvironment.ThrowAllErrors)
                {
                    FailException = exc;
                    if (window.Window?.IsShown == true)
                    {
                        window.Close(false);
                    }

                    res = false;
                    return;
                }
                finally
                {
                    cancellationToken.Dispose();
                }

                if (window.Window?.IsShown == true)
                {
                    window.Close(true);
                }

                res = true;
            });

            if (!progressTask.Wait(delay))
            {
                window.CreateAndOpenDialog(this);
            }

            if (window.Window?.IsShown == true)
            {
                window.Close(true);
            }

            wasProcessed = true;
            return new GlobalProgressResult(res, wasCancelled, FailException);
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

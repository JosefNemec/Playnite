using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace PlayniteInstaller
{
    public enum InstallStatus
    {
        Idle,
        Downloading,
        Installing
    }

    public class MainViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly Window windowHost;
        private readonly List<string> UrlMirrors;
        private WebClient webClient;

        private InstallStatus status;
        public InstallStatus Status
        {
            get => status;
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }

        private double progressValue;
        public double ProgressValue
        {
            get => progressValue;
            set
            {
                progressValue = value;
                OnPropertyChanged();
            }
        }

        private string destinationFolder;
        public string DestionationFolder
        {
            get => destinationFolder;
            set
            {
                destinationFolder = value;
                OnPropertyChanged();
            }
        }

        private bool portable;
        public bool Portable
        {
            get => portable;
            set
            {
                portable = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<object> BrowseCommand
        {
            get => new RelayCommand<object>((_) =>
            {
                Browse();
            });
        }

        public RelayCommand<object> InstallCommand
        {
            get => new RelayCommand<object>(async (_) =>
            {
                await Install();
            }, (_) => Status == InstallStatus.Idle);
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((_) =>
            {
                Cancel();
            }, (_) => Status != InstallStatus.Installing);
        }

        public static RelayCommand<string> NavigateUrlCommand
        {
            get => new RelayCommand<string>((url) =>
            {
                try
                {
                    Process.Start(url);
                }
                catch (Exception e) when (!Debugger.IsAttached)
                {
                    logger.Error(e, "Failed to open url.");
                }
            });
        }

        private List<string> ParseList(string input)
        {
            return input.Split(new[] { '\r', '\n' }).Where(a => !string.IsNullOrWhiteSpace(a)).ToList();
        }

        public MainViewModel(Window window)
        {
            windowHost = window;
            DestionationFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Playnite");
            UrlMirrors = ParseList(Resources.ReadFileFromResource("PlayniteInstaller.installer_mirrors.txt"));
            logger.Debug("Server mirrors in use:");
            UrlMirrors.ForEach(a => logger.Debug(a));
        }

        public void Browse()
        {
            var dialog = new FolderBrowserDialog()
            {
                Description = "Select Destination Folder...",
                ShowNewFolderButton = true
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DestionationFolder = Path.Combine(dialog.SelectedPath, "Playnite");
            }
        }

        public async Task Install()
        {
            if (DestionationFolder.StartsWith(@"c:\Program Files", StringComparison.OrdinalIgnoreCase))
            {
                System.Windows.MessageBox.Show(
                    "Can't install Playnite to selected directory, use a different path.",
                    "Location not supported",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!FileSystem.CanWriteToFolder(DestionationFolder))
            {
                System.Windows.MessageBox.Show(
                    "Can't install Playnite to selected directory, use a different path.",
                    "Access error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Status = InstallStatus.Downloading;

            try
            {
                FileSystem.DeleteFile(App.InstallerDownloadPath);
                if (webClient != null)
                {
                    webClient.Dispose();
                    webClient = null;
                }

                webClient = new WebClient();
                var installerUrls = await TryDownloadManifest(UrlMirrors);
                webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
                if (await TryDownloadInstaller(installerUrls) == false)
                {
                    return;
                }

                Status = InstallStatus.Installing;
                var args = string.Format(@"/VERYSILENT /NOCANCEL /DIR=""{0}"" ", DestionationFolder);
                args += Portable ? "/PORTABLE" : "";
                logger.Info($"Starting:\n{App.InstallerDownloadPath}\n{args}");

                var installed = false;
                await Task.Run(() =>
                {
                    using (var process = Process.Start(App.InstallerDownloadPath, args))
                    {
                        process.WaitForExit();
                        if (process.ExitCode == 0)
                        {
                            installed = true;
                        }
                        else
                        {
                            logger.Error($"Installer failed {process.ExitCode}");
                            System.Windows.MessageBox.Show(
                                $"Failed to install Playnite. Error code {process.ExitCode}",
                                "Installation error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
                            Status = InstallStatus.Idle;
                        }
                    }
                });

                FileSystem.DeleteFileSafe(App.InstallerDownloadPath);
                if (installed)
                {
                    windowHost.Close();
                }
            }
            catch (Exception e) when (!Debugger.IsAttached)
            {
                logger.Error(e, "Failed to download and install Playnite.");
                System.Windows.MessageBox.Show(
                    $"Failed to install Playnite:\n{e.Message}",
                    "Installation error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Status = InstallStatus.Idle;
            }
            finally
            {
                webClient?.Dispose();
                webClient = null;
            }
        }

        private async Task<List<string>> TryDownloadManifest(List<string> urls)
        {
            foreach (var url in urls)
            {
                try
                {
                    return ParseList(await webClient.DownloadStringTaskAsync(url));
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to download installer manifest from {url}");
                }
            }

            throw new Exception("Failed to download installer manifest.");
        }

        private async Task<bool> TryDownloadInstaller(List<string> urls)
        {
            foreach (var url in urls)
            {
                try
                {
                    await webClient.DownloadFileTaskAsync(url, App.InstallerDownloadPath);
                    return true;
                }
                catch (WebException webExp)
                {
                    if (webExp.Status == WebExceptionStatus.RequestCanceled)
                    {
                        return false;
                    }
                    else
                    {
                        logger.Error(webExp, $"Failed to download installer file from {url}");
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e, $"Failed to download installer file from {url}");
                }
            }

            throw new Exception("Failed to download installer file.");
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProgressValue = e.ProgressPercentage;
        }

        public void Cancel()
        {
            if (Status == InstallStatus.Downloading)
            {
                webClient.CancelAsync();
                webClient.Dispose();
                webClient = null;
                Status = InstallStatus.Idle;
            }
            else
            {
                windowHost.Close();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Playnite.Common;

namespace Playnite.FullscreenApp.Windows
{
    public class ThemeOptionsSettings
    {   public const string ThemeOptionGuid = "904cbf3b-573f-48f8-9642-0a09d05c64ef";
        public Dictionary<string, List<string>> SelectedPresets;
    }

    public partial class VideoSplash : Window
    {
        private DispatcherTimer _fadeTimer;
        private double _volumeStep;
        private double _opacityStep;

        public VideoSplash(string videoSource)
        {
            InitializeComponent();
            SplashVideo.Source = new Uri(videoSource, UriKind.RelativeOrAbsolute);
        }

        private void SplashVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            SplashVideo.LoadedBehavior = MediaState.Pause;
        }

        private void SplashVideo_MediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            CloseSplashScreen();
        }

        public void CloseSplashScreen()
        {
            StartFadeOut();
        }

        private void StartFadeOut()
        {
            _volumeStep = SplashVideo.Volume / 10.0;        // 10 steps for 1 seconds
            _opacityStep = this.Opacity / 10.0;             // 10 steps for 1 seconds
            _fadeTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)   // 100 ms interval
            };
            _fadeTimer.Tick += FadeOut;
            _fadeTimer.Start();
        }

        private void FadeOut(object sender, EventArgs e)
        {
            if (SplashVideo.Volume > 0)
            {
                SplashVideo.Volume -= _volumeStep;
            }

            if (this.Opacity > 0)
            {
                this.Opacity -= _opacityStep;
            }

            if (SplashVideo.Volume <= 0 && this.Opacity <= 0)
            {
                _fadeTimer.Stop();
                SplashVideo.LoadedBehavior = MediaState.Close; // Stop the media element
                Dispatcher.InvokeShutdown();
            }
        }
    }

    public class ExtendedSplashScreen
    {
        private Thread _splashThread;
        private readonly string _videoSource;
        private Dispatcher _splashDispatcher;
        private VideoSplash _splash;
        private SplashScreen _splashScreen;

        public ExtendedSplashScreen(string splashImage)
        {
            _videoSource = ChooseVideo();
            if ( _videoSource.IsNullOrEmpty())
            {
                _splashScreen = new SplashScreen(splashImage);
            }
        }
        public void Show(bool hideSmooth)
        {
            if (_videoSource.IsNullOrEmpty())
            {
                _splashScreen?.Show(hideSmooth);
            }
            else
            {
                ShowVideoSplash();
            }
        }

        public void Close(TimeSpan delay)
        {
            if (_videoSource.IsNullOrEmpty())
            {
                _splashScreen?.Close(delay);
            }
            else
            {
                CloseVideoSplash(delay);
            }
        }

        private void ShowVideoSplash()
        {
            if (_splashThread != null)
            {
                return;
            }

            _splashThread = new Thread(() =>
            {
                _splashDispatcher = Dispatcher.CurrentDispatcher;
                _splash = new VideoSplash(_videoSource);
                _splash.Show();
                Dispatcher.Run();
            });

            _splashThread.SetApartmentState(ApartmentState.STA);
            _splashThread.IsBackground = true;
            _splashThread.Start();
        }

        private void CloseVideoSplash(TimeSpan delay)
        {
            if (_videoSource.IsNullOrEmpty())
            {
                _splashScreen?.Close(delay);
            }
            else if (_splashThread != null)
            {
                Task.Delay(delay).ContinueWith(_ =>
                {
                    _splashDispatcher?.Invoke(() =>
                    {
                        _splash.CloseSplashScreen();
                    });
                    _splashThread.Join();
                    _splashThread = null;
                });
            }
        }
        private string ChooseVideo()
        {
            // try to open settings and find fullscreen theme name
            string settingsFile = PlaynitePaths.FullscreenConfigFilePath;
            if (File.Exists(settingsFile))
            {
                FullscreenSettings settings = Serialization.FromJsonFile<FullscreenSettings>(settingsFile);
                string currentTheme = settings.Theme;
                ThemeManifest theme = ThemeManager.GetAvailableThemes(SDK.ApplicationMode.Fullscreen).FirstOrDefault(t => t.Id == currentTheme);

                if ( theme?.DirectoryPath is string themeDir && Directory.Exists(themeDir) )
                {
                    string video = Directory.GetFiles(themeDir, "SplashVideo.mp4", SearchOption.AllDirectories).FirstOrDefault();
                    if ( !String.IsNullOrEmpty(video) ) 
                    {
                        return video;
                    }

                    // Suport of ThemeOption extension 
                    if ( File.Exists(Path.Combine(themeDir,"options.yaml")))
                    {
                        // theme is Option
                        string themeOptionsConfig = Path.Combine(PlaynitePaths.ExtensionsDataPath, ThemeOptionsSettings.ThemeOptionGuid, "config.json");
                        if (File.Exists(themeOptionsConfig))
                        {
                            ThemeOptionsSettings themeOptionSettings = Serialization.FromJsonFile<ThemeOptionsSettings>(themeOptionsConfig);
                            if (themeOptionSettings.SelectedPresets.TryGetValue(theme.Id, out List<string> presets))
                            {
                                var selectedPreset = presets.FirstOrDefault(s => s.ToLower().StartsWith("splashvideo."))??"splashVideo.default";
                                if (!selectedPreset.IsNullOrEmpty() && Directory.GetFiles(themeDir, selectedPreset + ".mp4", SearchOption.AllDirectories).FirstOrDefault() is string selectedVideo )
                                {
                                    return selectedVideo;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }
    }
}

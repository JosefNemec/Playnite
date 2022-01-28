using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin
{
    public class TestPluginSettings : ObservableObject
    {
        private string option1 = "test default";
        public string Option1
        {
            get => option1;
            set
            {
                option1 = value;
                OnPropertyChanged();
            }
        }

        private int option2 = 666;
        public int Option2
        {
            get => option2;
            set
            {
                option2 = value;
                OnPropertyChanged();
            }
        }
    }

    public class TestPluginSettingsViewModel : ObservableObject, ISettings
    {
        public readonly ILogger Logger = LogManager.GetLogger();
        public IPlayniteAPI PlayniteApi { get; set; }
        public TestPlugin Plugin { get; set; }
        public TestPluginSettings EditingClone { get; set; }

        public RelayCommand TestCommand => new RelayCommand(() =>
        {
            //using (var webview = PlayniteApi.WebViews.CreateOffscreenView())
            //{
            //    var cooks = webview.GetCookies();
            //}
        });

        private TestPluginSettings settings;
        public TestPluginSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public TestPluginSettingsViewModel(TestPlugin plugin, IPlayniteAPI playniteApi)
        {
            Plugin = plugin;
            PlayniteApi = playniteApi;

            var savedSettings = Plugin.LoadPluginSettings<TestPluginSettings>();
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new TestPluginSettings();
            }
        }

        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings = EditingClone;
        }

        public void EndEdit()
        {
            Plugin.SavePluginSettings(Settings);
        }

        public virtual bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}

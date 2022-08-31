using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.Markup
{
    public class Api : Extensions.Markup.Api
    {
        public Api() : base()
        {
        }

        public Api(string path) : base(path)
        {
        }
    }

    public class MainViewModel : Extensions.Markup.MainViewModel<DesktopAppViewModel, DesignMainViewModel, DesktopApplication>
    {
        public MainViewModel() : base()
        {
        }

        public MainViewModel(string path) : base(path)
        {
        }
    }

    public class PluginSettings : Extensions.Markup.PluginSettings
    {
        public PluginSettings() : base()
        {
        }

        public PluginSettings(string path) : base(path)
        {
        }
    }

    public class Settings : Extensions.Markup.Settings
    {
        public Settings() : base()
        {
        }

        public Settings(string path) : base(path)
        {
        }
    }

    public class ThemeFile : Extensions.Markup.ThemeFile
    {
        public ThemeFile() : base(ApplicationMode.Desktop)
        {
        }

        public ThemeFile(string path) : base(path, ApplicationMode.Desktop)
        {
        }
    }

    public class ThemeFileBinding : Extensions.Markup.ThemeFileBinding
    {
        public ThemeFileBinding() : base()
        {
        }

        public ThemeFileBinding(string path) : base(path)
        {
        }
    }

    public class PluginStatus : Extensions.Markup.PluginStatus
    {
    }

    public class PluginConverter : Extensions.Markup.PluginConverter
    {
    }
}

using Playnite.SDK;

namespace Playnite.FullscreenApp.Markup
{
    public class ThemeFile : Extensions.Markup.ThemeFile
    {
        public ThemeFile() : base(ApplicationMode.Fullscreen)
        {
        }

        public ThemeFile(string path) : base(path, ApplicationMode.Fullscreen)
        {
        }

        public static ThemeManifest GetDesignTimeDefaultTheme()
        {
            return GetDesignTimeDefaultTheme(ApplicationMode.Fullscreen);
        }
    }
}

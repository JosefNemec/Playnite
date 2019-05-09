using Playnite.SDK;

namespace Playnite.DesktopApp.Markup
{
    public class ThemeFile : Extensions.Markup.ThemeFile
    {
        public ThemeFile() : base(ApplicationMode.Desktop)
        {
        }

        public ThemeFile(string path) : base(path, ApplicationMode.Desktop)
        {
        }
    }
}

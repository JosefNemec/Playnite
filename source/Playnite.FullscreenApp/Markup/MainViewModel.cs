using Playnite.FullscreenApp.ViewModels;
using Playnite.FullscreenApp.ViewModels.DesignData;

namespace Playnite.FullscreenApp.Markup
{
    public class MainViewModel : Extensions.Markup.MainViewModel<FullscreenAppViewModel, DesignMainViewModel, FullscreenApplication>
    {
        public MainViewModel() : base()
        {
        }

        public MainViewModel(string path) : base(path)
        {
        }
    }
}

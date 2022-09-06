using Playnite.Common;
using Playnite.Controls;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.ViewModels;
using Playnite.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Playnite.DesktopApp.Windows
{
    public class SearchWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new SearchWindow();
        }
    }

    public partial class SearchWindow : WindowBase
    {
        public SearchWindow() : base("SearchWindow", false)
        {
            InitializeComponent();
            Activated += SearchWindow_Activated;
            Loaded += SearchWindow_Loaded;
            TextSearchBox.IsEnabledChanged += TextSearchBox_IsEnabledChanged;
        }

        private void SearchWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Needed because we use manual startup position and default one is "random".
            // Manual is needed in case we open this window while Playnite is minimized.
            if (!PositionHandler.HasSavedData())
            {
                var screen = Computer.GetPrimaryScreen();
                var dpi = VisualTreeHelper.GetDpi(this);
                Left = (screen.Bounds.X / dpi.DpiScaleX) + ((screen.Bounds.Width / dpi.DpiScaleX - Width) / 2);
                Top = ((screen.Bounds.Y / dpi.DpiScaleY) + ((screen.Bounds.Height / 2) - Height - Height)) / dpi.DpiScaleY;
            }
        }

        private void SearchWindow_Activated(object sender, System.EventArgs e)
        {
            this.RestoreWindow();   // Needed in case search is opened while Playnite is not active.
            TextSearchBox.Focus();
        }

        private void TextSearchBox_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (TextSearchBox.IsEnabled)
            {
                TextSearchBox.Focus();
            }
        }
    }

    public class SearchItemTemplateSelector : DataTemplateSelector
    {
        private readonly DataTemplate gameTemplate;
        private readonly DataTemplate genericTemplate;

        public SearchItemTemplateSelector()
        {
            gameTemplate = ResourceProvider.GetResource<DataTemplate>("SearchWindowGameItemTemplate");
            genericTemplate = ResourceProvider.GetResource<DataTemplate>("SearchWindowSearchItemTemplate");
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is GameSearchItem)
            {
                return gameTemplate;
            }
            else
            {
                return genericTemplate;
            }
        }
    }
}

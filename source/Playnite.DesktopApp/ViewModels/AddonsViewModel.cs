using Playnite.SDK;
using Playnite.Services;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.ViewModels
{
    public class AddonsViewModel : ObservableObject
    {
        private enum View : int
        {
            BrowseLibraries = 0,
            BrowseMetadata = 1,
            BrowseGeneric = 2,
            BrowseThemesDesktop = 3,
            BrowseThemesFullscreen = 4
        }

        private IWindowFactory window;
        private IPlayniteAPI api;
        private ServicesClient serviceClient;
        private readonly Dictionary<View, UserControl> sectionViews;

        private UserControl selectedSectionView;
        public UserControl SelectedSectionView
        {
            get => selectedSectionView;
            set
            {
                selectedSectionView = value;
                OnPropertyChanged();
            }
        }

        private List<AddonManifest> onlineAddonList;
        public List<AddonManifest> OnlineAddonList
        {
            get => onlineAddonList;
            set
            {
                onlineAddonList = value;
                OnPropertyChanged();
            }
        }

        private bool isOnlineListLoading;
        public bool IsOnlineListLoading
        {
            get => isOnlineListLoading;
            set
            {
                isOnlineListLoading = value;
                OnPropertyChanged();
            }
        }

        private string addonSearchText;
        public string AddonSearchText
        {
            get => addonSearchText;
            set
            {
                addonSearchText = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand<RoutedPropertyChangedEventArgs<object>> SectionChangedChangedCommand
        {
            get => new RelayCommand<RoutedPropertyChangedEventArgs<object>>((a) =>
            {
                SectionChanged(a);
            });
        }

        public RelayCommand<object> SearchAddonCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                SearchAddon();
            });
        }

        public AddonsViewModel(
            IWindowFactory window,
            IPlayniteAPI api,
            ServicesClient serviceClient)
        {
            this.window = window;
            this.api = api;
            this.serviceClient = serviceClient;

            sectionViews = new Dictionary<View, UserControl>()
            {
                { View.BrowseLibraries, new Controls.AddonsSections.BrowseAddons() { DataContext = this } },
                { View.BrowseMetadata, new Controls.AddonsSections.BrowseAddons() { DataContext = this } },
                { View.BrowseGeneric, new Controls.AddonsSections.BrowseAddons() { DataContext = this } },
                { View.BrowseThemesDesktop, new Controls.AddonsSections.BrowseAddons() { DataContext = this } },
                { View.BrowseThemesFullscreen, new Controls.AddonsSections.BrowseAddons() { DataContext = this } },
            };
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        private void SectionChanged(RoutedPropertyChangedEventArgs<object> selectedItem)
        {
            int viewIndex = -1;
            if (selectedItem.NewValue is TreeViewItem treeItem)
            {
                if (treeItem.Tag != null)
                {
                    viewIndex = int.Parse(treeItem.Tag.ToString());
                }
            }

            if (viewIndex == -1)
            {
                return;
            }

            var view = (View)viewIndex;
            switch (view)
            {
                case View.BrowseLibraries:
                    IsOnlineListLoading = true;
                    SelectedSectionView = sectionViews[view];
                    Task.Run(() =>
                    {
                        try
                        {
                            OnlineAddonList = serviceClient.GetAllAddons().ToList();
                        }
                        finally
                        {
                            IsOnlineListLoading = false;
                        }
                    });
                    break;
                case View.BrowseMetadata:
                    break;
                case View.BrowseGeneric:
                    break;
                case View.BrowseThemesDesktop:
                    break;
                case View.BrowseThemesFullscreen:
                    break;
                default:
                    break;
            }
        }

        private void SearchAddon()
        {
        }
    }
}

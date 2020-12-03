using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.DesktopApp.ViewModels
{
    public partial class AddonsViewModel : ObservableObject
    {
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
    }
}

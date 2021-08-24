using Playnite.SDK;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.ViewModels
{
    public class LicenseAgreementViewModel : ObservableObject
    {
        private IWindowFactory window;

        public string License { get; set; }
        public string LicenseTitle { get; set; }

        public RelayCommand<object> AcceptCommnad
        {
            get => new RelayCommand<object>((a) =>
            {
                window.Close(true);
            });
        }

        public RelayCommand<object> DeclineCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                window.Close(false);
            });
        }

        public LicenseAgreementViewModel(IWindowFactory window, string license, string addonName)
        {
            this.window = window;
            License = license;
            LicenseTitle = string.Format(ResourceProvider.GetString(LOC.AddonLicenseWindowTitle), addonName);
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }
    }
}

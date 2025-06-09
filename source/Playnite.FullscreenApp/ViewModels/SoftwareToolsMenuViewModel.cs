using Playnite.Common;
using Playnite.FullscreenApp.Windows;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.ViewModels;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.FullscreenApp.ViewModels
{
    public class SoftwareToolsMenuViewModel : ObservableObject
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private readonly IWindowFactory window;
        private readonly AppSoftware noApp = new AppSoftware(LOC.NoItemsFound.GetLocalized());
        public FullscreenAppViewModel MainModel { get; }
        public List<AppSoftware> Tools { get; }

        public RelayCommand CloseCommand => new RelayCommand(() => Close());
        public RelayCommand<AppSoftware> OpenToolCommand => new RelayCommand<AppSoftware>((c) =>
        {
            Close();
            if (c != noApp)
                MainModel.StartSoftwareTool(c);
        });

        public SoftwareToolsMenuViewModel(
            IWindowFactory window,
            FullscreenAppViewModel mainModel)
        {
            this.window = window;
            this.MainModel = mainModel;
            Tools = mainModel.Database.SoftwareApps.OrderBy(a => a.Name).ToList();
            if (Tools.Count == 0)
                Tools = new List<AppSoftware> { noApp };
        }

        public bool? OpenView()
        {
            return window.CreateAndOpenDialog(this);
        }

        public void Close()
        {
            window.Close(true);
        }
    }
}

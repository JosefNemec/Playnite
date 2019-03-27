using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.ViewModels.Desktop.DesignData
{
    public class DesignMainViewModel : MainViewModelBase
    {
        public DesignMainViewModel()
        {
            ProgressStatus = "Status example in progress...";
            ProgressValue = 50;
            ProgressTotal = 100;
            ProgressVisible = true;            
        }
    }
}

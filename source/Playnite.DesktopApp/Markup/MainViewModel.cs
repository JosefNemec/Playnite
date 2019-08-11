using Playnite.API.DesignData;
using Playnite.Converters;
using Playnite.DesktopApp.ViewModels;
using Playnite.Extensions;
using Playnite.Extensions.Markup;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.DesktopApp.Markup
{

    public class MainViewModel : Extensions.Markup.MainViewModel<DesktopAppViewModel, DesignMainViewModel, DesktopApplication>
    {
        public MainViewModel() : base()
        {
        }

        public MainViewModel(string path) : base(path)
        {
        }
    }
}

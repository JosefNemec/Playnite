using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Controls;
using Playnite.DesktopApp.ViewModels;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace Playnite.DesktopApp.Controls.Views
{
    public class LibraryDetailsView : BaseGamesView
    {
        static LibraryDetailsView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LibraryDetailsView), new FrameworkPropertyMetadata(typeof(LibraryDetailsView)));
        }

        public LibraryDetailsView() : base(ViewType.Details)
        {            
        }

        public LibraryDetailsView(DesktopAppViewModel mainModel) : base (ViewType.Details, mainModel)
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}
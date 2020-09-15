using Playnite.Common;
using Playnite.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.Controls
{
    public class SidebarItem : Button
    {
        static SidebarItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SidebarItem), new FrameworkPropertyMetadata(typeof(SidebarItem)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            BindingTools.SetBinding(this,
                Button.CommandProperty,
                nameof(SideBarItem.Command));
            BindingTools.SetBinding(this,
                Button.CommandParameterProperty,
                nameof(SideBarItem.CommandParameter));
            BindingTools.SetBinding(this,
                ContentPresenter.ContentProperty,
                nameof(SideBarItem.ImageObject));
        }
    }
}

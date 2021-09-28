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
    [TemplatePart(Name = "PART_ProgressStatus", Type = typeof(ProgressBar))]
    public class SidebarItem : Button
    {
        private ProgressBar ProgressStatus;

        static SidebarItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SidebarItem), new FrameworkPropertyMetadata(typeof(SidebarItem)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            BindingTools.SetBinding(this,
                Button.CommandProperty,
                nameof(SidebarWrapperItem.Command));
            BindingTools.SetBinding(this,
                ContentPresenter.ContentProperty,
                nameof(SidebarWrapperItem.IconObject));
            BindingTools.SetBinding(this,
                ContentPresenter.VisibilityProperty,
                nameof(SidebarWrapperItem.Visible),
                converter: new BooleanToVisibilityConverter());
            BindingTools.SetBinding(this,
                ContentPresenter.ToolTipProperty,
                nameof(SidebarWrapperItem.Title));

            ProgressStatus = Template.FindName("PART_ProgressStatus", this) as ProgressBar;
            if (ProgressStatus != null)
            {
                BindingTools.SetBinding(ProgressStatus,
                    ProgressBar.MaximumProperty,
                    nameof(SidebarWrapperItem.ProgressMaximum));
                BindingTools.SetBinding(ProgressStatus,
                    ProgressBar.ValueProperty,
                    nameof(SidebarWrapperItem.ProgressValue));
            }
        }
    }
}

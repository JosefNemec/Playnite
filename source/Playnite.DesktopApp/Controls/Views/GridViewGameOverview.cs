using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Playnite.Behaviors;
using Playnite.Common;

namespace Playnite.DesktopApp.Controls.Views
{
    public class GridViewGameOverview : GameOverview
    {
        static GridViewGameOverview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridViewGameOverview), new FrameworkPropertyMetadata(typeof(GridViewGameOverview)));
        }

        public GridViewGameOverview() : base(DesktopView.Grid)
        {
        }

        public GridViewGameOverview(DesktopAppViewModel mainModel) : base(DesktopView.Grid, mainModel)
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (ScrollViewHost != null)
            {
                ScrollViewerBehaviours.SetCustomScrollEnabled(ScrollViewHost, true);
                BindingTools.SetBinding(ScrollViewHost,
                    ScrollViewerBehaviours.SensitivityProperty,
                    mainModel.AppSettings,
                    $"{nameof(PlayniteSettings.GridViewDetailsScrollOptions)}.{nameof(ScrollBehaviorOptions.Sensitivity)}");
                BindingTools.SetBinding(ScrollViewHost,
                    ScrollViewerBehaviours.SpeedProperty,
                    mainModel.AppSettings,
                    $"{nameof(PlayniteSettings.GridViewDetailsScrollOptions)}.{nameof(ScrollBehaviorOptions.Speed)}",
                    converter: new Converters.TicksToTimeSpanConverter());
                BindingTools.SetBinding(ScrollViewHost,
                    ScrollViewerBehaviours.SmoothScrollEnabledProperty,
                    mainModel.AppSettings,
                    $"{nameof(PlayniteSettings.GridViewDetailsScrollOptions)}.{nameof(ScrollBehaviorOptions.SmoothEnabled)}");
            }
        }
    }
}

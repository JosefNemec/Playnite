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
    public class DetailsViewGameOverview : GameOverview
    {
        static DetailsViewGameOverview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DetailsViewGameOverview), new FrameworkPropertyMetadata(typeof(DetailsViewGameOverview)));
        }

        public DetailsViewGameOverview() : base(DesktopView.Details)
        {
        }

        public DetailsViewGameOverview(DesktopAppViewModel mainModel) : base(DesktopView.Details, mainModel)
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
                    $"{nameof(PlayniteSettings.DetailsViewDetailsScrollOptions)}.{nameof(ScrollBehaviorOptions.Sensitivity)}");
                BindingTools.SetBinding(ScrollViewHost,
                    ScrollViewerBehaviours.SpeedProperty,
                    mainModel.AppSettings,
                    $"{nameof(PlayniteSettings.DetailsViewDetailsScrollOptions)}.{nameof(ScrollBehaviorOptions.Speed)}",
                    converter: new Converters.TicksToTimeSpanConverter());
                BindingTools.SetBinding(ScrollViewHost,
                    ScrollViewerBehaviours.SmoothScrollEnabledProperty,
                    mainModel.AppSettings,
                    $"{nameof(PlayniteSettings.DetailsViewDetailsScrollOptions)}.{nameof(ScrollBehaviorOptions.SmoothEnabled)}");
            }
        }
    }
}

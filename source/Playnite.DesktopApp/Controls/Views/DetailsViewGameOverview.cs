using Playnite.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.DesktopApp.Controls.Views
{
    public class DetailsViewGameOverview : GameOverview
    {
        static DetailsViewGameOverview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(DetailsViewGameOverview), new FrameworkPropertyMetadata(typeof(DetailsViewGameOverview)));
        }

        public DetailsViewGameOverview() : base(ViewType.Details)
        {
        }

        public DetailsViewGameOverview(DesktopAppViewModel mainModel) : base(ViewType.Details, mainModel)
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}

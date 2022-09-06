using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Controls;
using Playnite.Converters;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
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

        public LibraryDetailsView() : base(DesktopView.Details)
        {
        }

        public LibraryDetailsView(DesktopAppViewModel mainModel) : base (DesktopView.Details, mainModel)
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ListGames != null)
            {
                ScrollViewerBehaviours.SetCustomScrollEnabled(ListGames, true);
                BindingTools.SetBinding(ListGames,
                    ScrollViewerBehaviours.SensitivityProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.DetailsViewScrollSensitivity));
                BindingTools.SetBinding(ListGames,
                    ScrollViewerBehaviours.SpeedProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.DetailsViewScrollSpeed),
                    converter: new TicksToTimeSpanConverter());
                BindingTools.SetBinding(ListGames,
                    ScrollViewerBehaviours.SmoothScrollEnabledProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.DetailsViewSmoothScrollEnabled));
            }
        }
    }
}
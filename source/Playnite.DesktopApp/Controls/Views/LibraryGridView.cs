using Playnite.Common;
using Playnite.Controls;
using Playnite.Converters;
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
using System.Xml.Linq;
using BooleanToVisibilityConverter = Playnite.Converters.BooleanToVisibilityConverter;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_SliderZoom", Type = typeof(SliderWithPopup))]
    public class LibraryGridView : BaseGamesView
    {
        private SliderWithPopup SliderZoom;

        static LibraryGridView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LibraryGridView), new FrameworkPropertyMetadata(typeof(LibraryGridView)));
        }

        public LibraryGridView() : base(ViewType.Grid)
        {
        }

        public LibraryGridView(DesktopAppViewModel mainModel) : base(ViewType.Grid, mainModel)
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ListGames != null)
            {
                ListGames.ItemsPanel = GetItemsPanelTemplate();
            }

            if (ControlGameView != null)
            {
                BindingTools.SetBinding(ControlGameView,
                    Control.VisibilityProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.GridViewSideBarVisible),
                    converter: new BooleanToVisibilityConverter());
            }

            SliderZoom = Template.FindName("PART_SliderZoom", this) as SliderWithPopup;
            if (SliderZoom != null)
            {
                SliderZoom.SliderMaximumValue = ViewSettings.MaxGridItemWidth;
                SliderZoom.SliderMinimumValue = ViewSettings.MinGridItemWidth;
                BindingTools.SetBinding(SliderZoom,
                    SliderWithPopup.SliderValueProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.GridItemWidth),
                    BindingMode.TwoWay);
                BindingTools.SetBinding(SliderZoom,
                    SliderWithPopup.PopupLabelProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.GridItemWidth),
                    converter: new CoversZoomToPercentageConverter(),
                    stringFormat: "{0}%");
            }
        }

        private ItemsPanelTemplate GetItemsPanelTemplate()
        {
            XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XNamespace pctrls = "clr-namespace:Playnite.Controls;assembly=Playnite";
            var templateDoc = new XDocument(
                new XElement(pns + nameof(ItemsPanelTemplate), 
                    new XElement(pctrls + nameof(VirtualizingUniformPanel)))
            );

            return Xaml.FromString<ItemsPanelTemplate>(templateDoc.ToString());
        }
    }
}

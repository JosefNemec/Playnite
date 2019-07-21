using Playnite.Commands;
using Playnite.Common;
using Playnite.Controls;
using Playnite.Converters;
using Playnite.DesktopApp.ViewModels;
using Playnite.Extensions;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_ImageBackground", Type = typeof(FadeImage))]
    [TemplatePart(Name = "PART_ViewDetails", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ViewGrid", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ViewList", Type = typeof(Control))]
    public class Library : Control
    {
        private readonly DesktopAppViewModel mainModel;
        private FadeImage ImageBackground;
        private Control ViewDetails;
        private Control ViewGrid;
        private Control ViewList;

        static Library()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Library), new FrameworkPropertyMetadata(typeof(Library)));
        }

        public Library() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public Library(DesktopAppViewModel mainModel)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }

            this.mainModel.AppSettings.PropertyChanged += AppSettings_PropertyChanged;
        }

        private void AppSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayniteSettings.ShowBackgroundImage) ||
                e.PropertyName == nameof(PlayniteSettings.StrechBackgroundImage))
            {
                if (ImageBackground != null)
                {
                    SetBackgroundBinding();
                }
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ImageBackground = Template.FindName("PART_ImageBackground", this) as FadeImage;
            if (ImageBackground != null)
            {
                SetBackgroundBinding();
            }

            SetViewBinding(ref ViewDetails, "PART_ViewDetails", ViewType.Details);
            SetViewBinding(ref ViewGrid, "PART_ViewGrid", ViewType.Grid);
            SetViewBinding(ref ViewList, "PART_ViewList", ViewType.List);
        }

        private void SetViewBinding(ref Control elem, string partId, ViewType type)
        {
            elem = Template.FindName(partId, this) as Control;
            if (elem != null)
            {
                BindingTools.SetBinding(elem,
                    Control.VisibilityProperty,
                    mainModel.AppSettings.ViewSettings,
                    nameof(ViewSettings.GamesViewType),
                    converter: new EnumToVisibilityConverter(),
                    converterParameter: type);
                BindingTools.SetBinding(elem,
                    Control.IsEnabledProperty,
                    mainModel.AppSettings.ViewSettings,
                    nameof(ViewSettings.GamesViewType),
                    converter: new EnumToBooleanConverter(),
                    converterParameter: type);
            }
        }

        private void SetBackgroundBinding()
        {
            if (mainModel.AppSettings.ShowBackgroundImage && mainModel.AppSettings.StrechBackgroundImage)
            {
                BindingTools.SetBinding(ImageBackground,
                    FadeImage.SourceProperty,
                    mainModel,
                    $"{nameof(mainModel.SelectedGame)}.{nameof(GamesCollectionViewEntry.BackgroundImageObject)}",
                    isAsync: true);
            }
            else
            {
                ImageBackground.Source = null;
            }
        }
    }
}
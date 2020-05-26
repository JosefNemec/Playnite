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
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_ImageBackground", Type = typeof(FadeImage))]
    [TemplatePart(Name = "PART_ViewDetails", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ViewGrid", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ViewList", Type = typeof(Control))]
    [TemplatePart(Name = "PART_ElemNoGamesNotif", Type = typeof(FrameworkElement))]
    public class Library : Control
    {
        private readonly DesktopAppViewModel mainModel;
        private FadeImage ImageBackground;
        private Control ViewDetails;
        private Control ViewGrid;
        private Control ViewList;
        private FrameworkElement ElemNoGamesNotif;

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
            this.mainModel.AppSettings.ViewSettings.PropertyChanged += ViewSettings_PropertyChanged;
        }

        private void ViewSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ViewSettings.GamesViewType))
            {
                SetBackgroundBinding();
            }
        }

        private void AppSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayniteSettings.ShowBackgroundImageOnWindow) ||
                e.PropertyName == nameof(PlayniteSettings.ShowBackImageOnGridView))
            {
                SetBackgroundBinding();
            }
            else if (e.PropertyName == nameof(PlayniteSettings.DarkenWindowBackgroundImage) ||
                     e.PropertyName == nameof(PlayniteSettings.BackgroundImageDarkAmount))
            {
                SetBackgroundEffect();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ImageBackground = Template.FindName("PART_ImageBackground", this) as FadeImage;
            if (ImageBackground != null)
            {
                BindingTools.SetBinding(ImageBackground,
                    FadeImage.IsBlurEnabledProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.BlurWindowBackgroundImage),
                    mode: BindingMode.OneWay);
                BindingTools.SetBinding(ImageBackground,
                    FadeImage.BlurAmountProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.BackgroundImageBlurAmount),
                    mode: BindingMode.OneWay);
                BindingTools.SetBinding(ImageBackground,
                    FadeImage.HighQualityBlurProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.HighQualityBackgroundBlur),
                    mode: BindingMode.OneWay);
                BindingTools.SetBinding(ImageBackground,
                    FadeImage.AnimationEnabledProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.BackgroundImageAnimation),
                    mode: BindingMode.OneWay);
            }

            ElemNoGamesNotif = Template.FindName("PART_ElemNoGamesNotif", this) as FrameworkElement;
            if (ElemNoGamesNotif != null)
            {
                BindingTools.SetBinding(ElemNoGamesNotif,
                    Control.VisibilityProperty,
                    mainModel,
                    $"{nameof(mainModel.GamesView)}.{nameof(mainModel.GamesView.CollectionView)}.{nameof(mainModel.GamesView.Items.Count)}",
                    converter: new IntToVisibilityConverter(),
                    converterParameter: 0);
            }

            SetBackgroundBinding();
            SetBackgroundEffect();

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
            if (ImageBackground == null)
            {
                return;
            }

            if (mainModel.AppSettings.ShowBackgroundImageOnWindow &&
                ((mainModel.AppSettings.ShowBackImageOnGridView && mainModel.AppSettings.ViewSettings.GamesViewType == ViewType.Grid) ||
                mainModel.AppSettings.ViewSettings.GamesViewType == ViewType.Details))
            {
                BindingTools.SetBinding(ImageBackground,
                    FadeImage.SourceProperty,
                    mainModel,
                    $"{nameof(mainModel.SelectedGame)}.{nameof(GamesCollectionViewEntry.DisplayBackgroundImageObject)}");
            }
            else
            {
                ImageBackground.Source = null;
            }
        }

        private void SetBackgroundEffect()
        {
            if (ImageBackground != null)
            {
                if (mainModel.AppSettings.DarkenWindowBackgroundImage)
                {
                    ImageBackground.ImageDarkeningBrush = null;
                    ImageBackground.ImageDarkeningBrush = new SolidColorBrush(new Color()
                    {
                        ScA = mainModel.AppSettings.BackgroundImageDarkAmount,
                        ScR = 0,
                        ScG = 0,
                        ScB = 0
                    });
                }
                else
                {
                    ImageBackground.ImageDarkeningBrush = null;
                }
            }
        }
    }
}
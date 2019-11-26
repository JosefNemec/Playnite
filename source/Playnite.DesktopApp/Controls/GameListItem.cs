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
using System.Windows.Input;
using BooleanToVisibilityConverter = Playnite.Converters.BooleanToVisibilityConverter;

namespace Playnite.DesktopApp.Controls
{
    [TemplatePart(Name = "PART_PanelHost", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ImageIcon", Type = typeof(Image))]
    [TemplatePart(Name = "PART_ImageCover", Type = typeof(Image))]
    [TemplatePart(Name = "PART_ButtonPlay", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonInfo", Type = typeof(Button))]
    public class GameListItem : Control
    {
        private readonly DesktopAppViewModel mainModel;
        private FrameworkElement PanelHost;
        private Image ImageIcon;
        private Image ImageCover;
        private Button ButtonPlay;
        private Button ButtonInfo;

        static GameListItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameListItem), new FrameworkPropertyMetadata(typeof(GameListItem)));
        }

        public GameListItem() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public GameListItem(DesktopAppViewModel mainModel)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PanelHost = Template.FindName("PART_PanelHost", this) as FrameworkElement;
            if (PanelHost != null)
            {
                var mBinding = new MouseBinding(mainModel.StartGameCommand, new MouseGesture(MouseAction.LeftDoubleClick));
                BindingTools.SetBinding(mBinding,
                    MouseBinding.CommandParameterProperty,
                    nameof(GamesCollectionViewEntry.Game));
                PanelHost.InputBindings.Add(mBinding);

                if (!DesignerProperties.GetIsInDesignMode(this))
                {
                    PanelHost.ContextMenu = new GameMenu(mainModel);
                    BindingTools.SetBinding(PanelHost.ContextMenu,
                        Button.DataContextProperty,
                        mainModel,
                        nameof(DesktopAppViewModel.SelectedGames));
                }
            }

            ImageIcon = Template.FindName("PART_ImageIcon", this) as Image;
            if (ImageIcon != null)
            {
                BindingTools.SetBinding(ImageIcon,
                    Image.VisibilityProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.ShowIconsOnList),
                    converter: new BooleanToVisibilityConverter());

                var sourceBinding = new PriorityBinding();
                sourceBinding.Bindings.Add(new Binding()
                {
                    Path = new PropertyPath(nameof(GamesCollectionViewEntry.DetailsListIconObjectCached)),
                    IsAsync = mainModel.AppSettings.AsyncImageLoading,
                    Converter = new NullToDependencyPropertyUnsetConverter(),
                    Mode = BindingMode.OneWay
                });
                sourceBinding.Bindings.Add(new Binding()
                {
                    Path = new PropertyPath(nameof(GamesCollectionViewEntry.DefaultDetailsListIconObjectCached)),
                    Converter = new NullToDependencyPropertyUnsetConverter(),
                    Mode = BindingMode.OneWay
                });

                BindingOperations.SetBinding(ImageIcon, Image.SourceProperty, sourceBinding);
            }

            ImageCover = Template.FindName("PART_ImageCover", this) as Image;
            if (ImageCover != null)
            {
                var sourceBinding = new PriorityBinding();
                sourceBinding.Bindings.Add(new Binding()
                {
                    Path = new PropertyPath(nameof(GamesCollectionViewEntry.GridViewCoverObjectCached)),
                    IsAsync = mainModel.AppSettings.AsyncImageLoading,
                    Converter = new NullToDependencyPropertyUnsetConverter(),
                    Mode = BindingMode.OneWay
                });
                sourceBinding.Bindings.Add(new Binding()
                {
                    Path = new PropertyPath(nameof(GamesCollectionViewEntry.DefaultGridViewCoverObjectCached)),
                    Converter = new NullToDependencyPropertyUnsetConverter(),
                    Mode = BindingMode.OneWay
                });

                BindingOperations.SetBinding(ImageCover, Image.SourceProperty, sourceBinding);
            }

            ButtonPlay = Template.FindName("PART_ButtonPlay", this) as Button;
            if (ButtonPlay != null)
            {
                ButtonPlay.Command = mainModel.StartGameCommand;
                BindingTools.SetBinding(ButtonPlay,
                    Button.CommandParameterProperty,
                    nameof(GamesCollectionViewEntry.Game));
            }

            ButtonInfo = Template.FindName("PART_ButtonInfo", this) as Button;
            if (ButtonInfo != null)
            {
                ButtonInfo.Command = mainModel.ShowGameSideBarCommand;
                BindingTools.SetBinding(ButtonInfo,
                    Button.CommandParameterProperty,
                    string.Empty);
            }
        }
    }
}
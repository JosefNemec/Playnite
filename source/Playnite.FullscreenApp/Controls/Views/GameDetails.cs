using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Controls;
using Playnite.Converters;
using Playnite.FullscreenApp.ViewModels;
using Playnite.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Playnite.FullscreenApp.Controls.Views
{
    [TemplatePart(Name = "PART_ViewHost", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ButtonContext", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonOptions", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ImageCover", Type = typeof(Image))]
    [TemplatePart(Name = "PART_ImageBackground", Type = typeof(FadeImage))]
    public class GameDetails : Control
    {
        private FullscreenAppViewModel mainModel;
        private FrameworkElement ViewHost;
        private ButtonBase ButtonContext;
        private ButtonBase ButtonOptions;
        private Image ImageCover;
        private FadeImage ImageBackground;

        static GameDetails()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameDetails), new FrameworkPropertyMetadata(typeof(GameDetails)));
        }

        public GameDetails() : this(FullscreenApplication.Current?.MainModel)
        {
        }

        public GameDetails(FullscreenAppViewModel mainModel) : base()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                var designModel = DesignMainViewModel.DesignIntance;
                this.mainModel = designModel;
                DataContext = designModel.GamesView.Items[0];
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (Template != null)
            {
                ViewHost = Template.FindName("PART_ViewHost", this) as FrameworkElement;
                if (ViewHost != null)
                {
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.ToggleGameDetailsCommand, Key = Key.Back });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.ToggleGameDetailsCommand, Key = Key.Escape });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.SelectPrevGameCommand, Key = Key.F2 });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.SelectNextGameCommand, Key = Key.F3 });
                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.ToggleGameDetailsCommand, XInputButton.B));
                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.SelectPrevGameCommand, XInputButton.LeftShoulder));
                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.SelectNextGameCommand, XInputButton.RightShoulder));

                    BindingTools.SetBinding(ViewHost,
                         FocusBahaviors.FocusBindingProperty,
                         mainModel,
                         nameof(mainModel.GameDetailsFocused));
                }

                ButtonContext = Template.FindName("PART_ButtonContext", this) as ButtonBase;
                if (ButtonContext != null)
                {
                    BindingTools.SetBinding(
                        ButtonContext,
                        ButtonBase.CommandProperty,
                        mainModel,
                        $"{nameof(mainModel.SelectedGameDetails)}.{nameof(mainModel.SelectedGameDetails.ContextActionCommand)}");
                    BindingTools.SetBinding(
                        ButtonContext,
                        ButtonBase.ContentProperty,
                        mainModel,
                        $"{nameof(mainModel.SelectedGameDetails)}.{nameof(mainModel.SelectedGameDetails.ContextActionDescription)}");
                }

                ButtonOptions = Template.FindName("PART_ButtonOptions", this) as ButtonBase;
                if (ButtonOptions != null)
                {
                    ButtonOptions.Command = mainModel.ToggleGameOptionsCommand;
                }

                ImageCover = Template.FindName("PART_ImageCover", this) as Image;
                if (ImageCover != null)
                {
                    var sourceBinding = new PriorityBinding();
                    sourceBinding.Bindings.Add(new Binding()
                    {
                        Path = new PropertyPath(nameof(GamesCollectionViewEntry.CoverImageObject)),
                        Converter = new NullToDependencyPropertyUnsetConverter()
                    });
                    sourceBinding.Bindings.Add(new Binding()
                    {
                        Path = new PropertyPath(nameof(GamesCollectionViewEntry.DefaultCoverImageObject)),
                        Converter = new NullToDependencyPropertyUnsetConverter()
                    });

                    BindingOperations.SetBinding(ImageCover, Image.SourceProperty, sourceBinding);
                }

                ImageBackground = Template.FindName("PART_ImageBackground", this) as FadeImage;
                if (ImageBackground != null)
                {
                    BindingTools.SetBinding(ImageBackground,
                        FadeImage.SourceProperty,
                        nameof(GamesCollectionViewEntry.DisplayBackgroundImageObject));
                }
            }
        }
    }
}

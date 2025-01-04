using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Controls;
using Playnite.Converters;
using Playnite.FullscreenApp.Markup;
using Playnite.FullscreenApp.ViewModels;
using Playnite.Input;
using Playnite.SDK;
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
    [TemplatePart(Name = "PART_HtmlDescription", Type = typeof(HtmlTextView))]
    [TemplatePart(Name = "PART_ScrollHtmlDescription", Type = typeof(ScrollViewer))]
    public class GameDetails : Control
    {
        private FullscreenAppViewModel mainModel;
        private FrameworkElement ViewHost;
        private ButtonBase ButtonContext;
        private ButtonBase ButtonOptions;
        private Image ImageCover;
        private FadeImage ImageBackground;
        private HtmlTextView HtmlDescription;
        private ScrollViewer ScrollHtmlDescription;

        static GameDetails()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameDetails), new FrameworkPropertyMetadata(typeof(GameDetails)));
        }

        public GameDetails() : this(FullscreenApplication.Current?.MainModel)
        {
            DataContextChanged += GameDetails_DataContextChanged;
        }

        private void GameDetails_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            // Scrollview not resseting causes some incidental issues elswhere, see #2663
            ScrollHtmlDescription?.ScrollToTop();
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
                this.mainModel.PropertyChanged += MainModel_PropertyChanged;
            }
        }

        private void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(mainModel.SelectedGame) &&
                mainModel.GameDetailsVisible &&
                mainModel.SelectedGame == null)
            {
                // This takes care of case where game is modified in a way that would remove it from current list.
                // Changing favorite status, removing it etc. It would result in empty game details #2458
                // TODO handle properly in future via TODO from SelectedGame property
                if (mainModel.GamesView.CollectionView.Count > 0)
                {
                    if (mainModel.LastValidSelectedGameIndex + 1 > mainModel.GamesView.CollectionView.Count)
                    {
                        mainModel.SelectedGame = mainModel.GamesView.CollectionView.GetItemAt(mainModel.LastValidSelectedGameIndex - 1) as GamesCollectionViewEntry;
                    }
                    else
                    {
                        mainModel.SelectedGame = mainModel.GamesView.CollectionView.GetItemAt(mainModel.LastValidSelectedGameIndex) as GamesCollectionViewEntry;
                    }
                }
                else
                {
                    mainModel.ToggleGameDetailsCommand.Execute(null);
                }
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
                    ViewHost.InputBindings.Add(new GameControllerInputBinding(mainModel.SelectPrevGameCommand, ControllerInput.LeftShoulder));
                    ViewHost.InputBindings.Add(new GameControllerInputBinding(mainModel.SelectNextGameCommand, ControllerInput.RightShoulder));

                    var backInput = new GameControllerInputBinding { Command = mainModel.ToggleGameDetailsCommand };
                    BindingTools.SetBinding(backInput,
                        GameControllerInputBinding.ButtonProperty,
                        null,
                        typeof(GameControllerGesture).GetProperty(nameof(GameControllerGesture.CancellationBinding)));
                    ViewHost.InputBindings.Add(backInput);

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
                    ButtonOptions.Command = mainModel.OpenGameMenuCommand;
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
                    ImageBackground.SourceUpdateDelay = 100;
                    BindingTools.SetBinding(ImageBackground,
                        FadeImage.SourceProperty,
                        nameof(GamesCollectionViewEntry.DisplayBackgroundImageObject));
                }

                HtmlDescription = Template.FindName("PART_HtmlDescription", this) as HtmlTextView;
                if (HtmlDescription != null)
                {
                    BindingTools.SetBinding(HtmlDescription,
                        HtmlTextView.HtmlTextProperty,
                        nameof(GamesCollectionViewEntry.Description));
                    HtmlDescription.TemplatePath = ThemeFile.GetFilePath("DescriptionView.html");
                }

                ControlTemplateTools.InitializePluginControls(
                    mainModel.Extensions,
                    Template,
                    this,
                    ApplicationMode.Fullscreen,
                    mainModel,
                    $"{nameof(FullscreenAppViewModel.SelectedGameDetails)}.{nameof(GameDetailsViewModel.Game)}.{nameof(GameDetailsViewModel.Game.Game)}");

                ScrollHtmlDescription = Template.FindName("PART_ScrollHtmlDescription", this) as ScrollViewer;
            }
        }
    }
}

using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Converters;
using Playnite.FullscreenApp.ViewModels;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Playnite.FullscreenApp.Controls.Views
{
    [TemplatePart(Name = "PART_ViewHost", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_PanelActionButtons", Type = typeof(Panel))]
    [TemplatePart(Name = "PART_TextStatus", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ImageCover", Type = typeof(Image))]
    public class GameStatus : Control
    {
        private FullscreenAppViewModel mainModel;
        private FrameworkElement ViewHost;
        private Panel PanelActionButtons;
        private TextBlock TextStatus;
        private Image ImageCover;

        static GameStatus()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameStatus), new FrameworkPropertyMetadata(typeof(GameStatus)));
        }

        public GameStatus() : this(FullscreenApplication.Current?.MainModel)
        {
        }

        public GameStatus(FullscreenAppViewModel mainModel) : base()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (Template == null)
            {
                return;
            }

            ViewHost = Template.FindName("PART_ViewHost", this) as FrameworkElement;
            if (ViewHost != null)
            {
                BindingTools.SetBinding(ViewHost,
                     FocusBahaviors.FocusBindingProperty,
                     mainModel,
                     nameof(mainModel.GameStatusVisible));
            }

            PanelActionButtons = Template.FindName("PART_PanelActionButtons", this) as Panel;
            if (PanelActionButtons != null)
            {
                var buttonClose = new ButtonEx();
                buttonClose.Content = ResourceProvider.GetString(LOC.CloseLabel);
                buttonClose.SetResourceReference(ButtonEx.StyleProperty, "ButtonGameStatusAction");
                buttonClose.Command = mainModel.CloseGameStatusCommand;
                PanelActionButtons.Children.Add(buttonClose);
            }

            TextStatus = Template.FindName("PART_TextStatus", this) as TextBlock;
            if (TextStatus != null)
            {
                BindingTools.SetBinding(TextStatus,
                     TextBlock.TextProperty,
                     nameof(GameStatusViewModel.GameStatusText));
            }

            ImageCover = Template.FindName("PART_ImageCover", this) as Image;
            if (ImageCover != null)
            {
                var sourceBinding = new PriorityBinding();
                sourceBinding.Bindings.Add(new Binding()
                {
                    Path = new PropertyPath($"{nameof(GameStatusViewModel.Game)}.{nameof(GamesCollectionViewEntry.CoverImageObject)}"),
                    Converter = new NullToDependencyPropertyUnsetConverter()
                });
                sourceBinding.Bindings.Add(new Binding()
                {
                    Path = new PropertyPath($"{nameof(GameStatusViewModel.Game)}.{nameof(GamesCollectionViewEntry.DefaultCoverImageObject)}"),
                    Converter = new NullToDependencyPropertyUnsetConverter()
                });

                BindingOperations.SetBinding(ImageCover, Image.SourceProperty, sourceBinding);
            }
        }
    }
}

using Playnite.Behaviors;
using Playnite.Common;
using Playnite.FullscreenApp.ViewModels;
using Playnite.Input;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Playnite.FullscreenApp.Controls.Views
{
    [TemplatePart(Name = "PART_MenuHost", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ButtonSettings", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonExitPlaynite", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonSwitchToDesktop", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtoFeedback", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonPatreon", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonRestartSystem", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonShutdownSystem", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonSleepSystem", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonHibernateSystem", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonPickRandomGame", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonUpdateLibrary", Type = typeof(ButtonBase))]
    public class MainMenu : Control
    {
        private FullscreenAppViewModel mainModel;
        private FrameworkElement MenuHost;
        private ButtonBase ButtonSettings;
        private ButtonBase ButtonExitPlaynite;
        private ButtonBase ButtonSwitchToDesktop;
        private ButtonBase ButtonPatreon;
        private ButtonBase ButtoFeedback;
        private ButtonBase ButtonRestartSystem;
        private ButtonBase ButtonShutdownSystem;
        private ButtonBase ButtonSleepSystem;
        private ButtonBase ButtonHibernateSystem;
        private ButtonBase ButtonPickRandomGame;
        private ButtonBase ButtonUpdateLibrary;

        static MainMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainMenu), new FrameworkPropertyMetadata(typeof(MainMenu)));
        }

        public MainMenu() : this(FullscreenApplication.Current?.MainModel)
        {
        }

        public MainMenu(FullscreenAppViewModel mainModel) : base()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }

            Focusable = false;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template != null)
            {
                MenuHost = Template.FindName("PART_MenuHost", this) as FrameworkElement;
                if (MenuHost != null)
                {
                    MenuHost.InputBindings.Add(new KeyBinding(mainModel.ToggleMainMenuCommand, new KeyGesture(Key.Back)));
                    MenuHost.InputBindings.Add(new KeyBinding(mainModel.ToggleMainMenuCommand, new KeyGesture(Key.Escape)));
                    MenuHost.InputBindings.Add(new XInputBinding(mainModel.ToggleMainMenuCommand, XInputButton.Back));
                    MenuHost.InputBindings.Add(new XInputBinding(mainModel.ToggleMainMenuCommand, XInputButton.B));
                }

                ButtonSettings = Template.FindName("PART_ButtonSettings", this) as ButtonBase;
                if (ButtonSettings != null)
                {
                    ButtonSettings.Command = mainModel.ToggleSettingsMenuCommand;
                    BindingTools.SetBinding(ButtonSettings,
                         FocusBahaviors.FocusBindingProperty,
                         mainModel,
                         nameof(mainModel.MainMenuFocused));
                }

                ButtonExitPlaynite = Template.FindName("PART_ButtonExitPlaynite", this) as ButtonBase;
                if (ButtonExitPlaynite != null)
                {
                    ButtonExitPlaynite.Command = mainModel.ExitCommand;
                }

                ButtonSwitchToDesktop = Template.FindName("PART_ButtonSwitchToDesktop", this) as ButtonBase;
                if (ButtonSwitchToDesktop != null)
                {
                    ButtonSwitchToDesktop.Command = mainModel.SwitchToDesktopCommand;
                }

                ButtonPickRandomGame = Template.FindName("PART_ButtonPickRandomGame", this) as ButtonBase;
                if (ButtonPickRandomGame != null)
                {
                    ButtonPickRandomGame.Command = mainModel.SelectRandomGameCommand;
                }

                ButtonPatreon = Template.FindName("PART_ButtonPatreon", this) as ButtonBase;
                if (ButtonPatreon != null)
                {
                    ButtonPatreon.Command = new NavigateUrlCommand();
                    ButtonPatreon.CommandParameter = UrlConstants.Patreon;
                }

                ButtoFeedback = Template.FindName("PART_ButtoFeedback", this) as ButtonBase;
                if (ButtoFeedback != null)
                {
                    ButtoFeedback.Command = new NavigateUrlCommand();
                    if (PlayniteEnvironment.ReleaseChannel == ReleaseChannel.Beta)
                    {
                        ButtoFeedback.CommandParameter = UrlConstants.IssuesTesting;
                    }
                    else
                    {
                        ButtoFeedback.CommandParameter = UrlConstants.Issues;
                    }
                }

                ButtonRestartSystem = Template.FindName("PART_ButtonRestartSystem", this) as ButtonBase;
                if (ButtonRestartSystem != null)
                {
                    ButtonRestartSystem.Command = mainModel.RestartSystemCommand;
                }

                ButtonShutdownSystem = Template.FindName("PART_ButtonShutdownSystem", this) as ButtonBase;
                if (ButtonShutdownSystem != null)
                {
                    ButtonShutdownSystem.Command = mainModel.ShutdownSystemCommand;
                }

                ButtonSleepSystem = Template.FindName("PART_ButtonSleepSystem", this) as ButtonBase;
                if (ButtonSleepSystem != null)
                {
                    ButtonSleepSystem.Command = mainModel.SleepSystemCommand;
                }

                ButtonHibernateSystem = Template.FindName("PART_ButtonHibernateSystem", this) as ButtonBase;
                if (ButtonHibernateSystem != null)
                {
                    ButtonHibernateSystem.Command = mainModel.HibernateSystemCommand;
                }

                ButtonUpdateLibrary = Template.FindName("PART_ButtonUpdateLibrary", this) as ButtonBase;
                if (ButtonUpdateLibrary != null)
                {
                    ButtonUpdateLibrary.Command = mainModel.UpdateGamesCommand;
                }
            }
        }
    }
}

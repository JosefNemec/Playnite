using Playnite.Behaviors;
using Playnite.Common;
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
using System.Windows.Data;
using System.Windows.Input;

namespace Playnite.FullscreenApp.Controls.Views
{
    [TemplatePart(Name = "PART_MenuHost", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ItemsHost", Type = typeof(ItemsControl))]
    public class GameMenu : Control
    {
        private FullscreenAppViewModel mainModel;
        private FrameworkElement MenuHost;
        private ItemsControl ItemsHost;

        static GameMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GameMenu), new FrameworkPropertyMetadata(typeof(GameMenu)));
        }

        public GameMenu() : this(FullscreenApplication.Current?.MainModel)
        {
        }

        public GameMenu(FullscreenAppViewModel mainModel) : base()
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

            if (Template != null)
            {
                MenuHost = Template.FindName("PART_MenuHost", this) as FrameworkElement;
                if (MenuHost != null)
                {
                    MenuHost.InputBindings.Add(new KeyBinding(mainModel.ToggleGameOptionsCommand, new KeyGesture(Key.Back)));
                    MenuHost.InputBindings.Add(new KeyBinding(mainModel.ToggleGameOptionsCommand, new KeyGesture(Key.Escape)));
                    MenuHost.InputBindings.Add(new XInputBinding(mainModel.ToggleGameOptionsCommand, XInputButton.B));
                    MenuHost.InputBindings.Add(new XInputBinding(mainModel.ToggleGameOptionsCommand, XInputButton.Start));
                    BindingTools.SetBinding(
                        MenuHost,
                        Control.DataContextProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.SelectedGameDetails));
                }

                ItemsHost = Template.FindName("PART_ItemsHost", this) as ItemsControl;
                if (ItemsHost != null)
                {
                    BindingTools.SetBinding(
                        ItemsHost,
                        ItemsControl.ItemsSourceProperty,
                        mainModel,
                        $"{nameof(FullscreenAppViewModel.SelectedGameDetails)}.{nameof(FullscreenAppViewModel.SelectedGameDetails.GameItems)}");
                    BindingTools.SetBinding(ItemsHost,
                         FocusBahaviors.FocusBindingProperty,
                         mainModel,
                         nameof(mainModel.GameMenuVisible));
                }
            }
        }
    }
}

using Playnite.Common;
using Playnite.FullscreenApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Playnite.Input.GameControllerManager;

namespace Playnite.FullscreenApp.Controls.SettingsSections
{
    /// <summary>
    /// Interaction logic for Visuals.xaml
    /// </summary>
    public partial class Input : SettingsSectionControl
    {
        private FullscreenApplication app;

        public Input()
        {
            InitializeComponent();
        }

        public Input(FullscreenAppViewModel mainModel)
        {
            InitializeComponent();

            BindingTools.SetBinding(
                ToggleMouseCursor,
                ToggleButton.IsCheckedProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.HideMouserCursor),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

            BindingTools.SetBinding(
                ToggleControllerInput,
                ToggleButton.IsCheckedProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.EnableGameControllerSupport),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

            BindingTools.SetBinding(
                ToggleSwapXA,
                ToggleButton.IsCheckedProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.SwapStartDetailsAction),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

            BindingTools.SetBinding(
                ToggleSwapConfirmCancel,
                ToggleButton.IsCheckedProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.SwapConfirmCancelButtons),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

            BindingTools.SetBinding(
                ToggleGuideFocus,
                ToggleButton.IsCheckedProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.GuideButtonFocus),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

            app = mainModel.App as FullscreenApplication;
            if (app.GameController != null)
            {
                LoadControllers();
                app.GameController.ControllersChanged += GameController_ControllersChanged;
            }
        }

        private void LoadControllers()
        {
            StackControllers.Children.Clear();
            if (app.GameController.Controllers.Count == 0)
            {
                StackControllers.Children.Add(new TextBlock
                {
                    Text = LOC.NoControllersDetected.GetLocalized(),
                    Style = FindResource("TextBlockBaseStyle") as Style
                });
                return;
            }

            foreach (var controller in app.GameController.Controllers)
            {
                var check = new CheckBoxEx()
                {
                    Content = controller.Name
                };

                BindingTools.SetBinding(
                    check,
                    CheckBox.IsCheckedProperty,
                    controller,
                    nameof(LoadedGameController.Enabled),
                    BindingMode.TwoWay,
                    UpdateSourceTrigger.PropertyChanged);

                StackControllers.Children.Add(check);
            }
        }

        private void GameController_ControllersChanged(object sender, EventArgs e)
        {
            LoadControllers();
            ToggleControllerInput.Focus();
        }

        public override void Dispose()
        {
            if (app.GameController != null)
            {
                app.GameController.ControllersChanged -= GameController_ControllersChanged;
                app.AppSettings.Fullscreen.DisabledGameControllers = app.GameController.Controllers.
                    Where(a => !a.Enabled).
                    Select(a => a.Path).ToList();
            }
        }
    }
}

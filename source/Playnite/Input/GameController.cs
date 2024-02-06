using Playnite.Native;
using Playnite.SDK;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using static SDL2.SDL;

namespace Playnite.Input
{
    public enum ControllerInputState
    {
        Pressed,
        Released
    }

    public enum ControllerInput
    {
        None,
        Start,
        Back,
        LeftStick,
        RightStick,
        LeftShoulder,
        RightShoulder,
        Guide,
        A,
        B,
        X,
        Y,
        DPadLeft,
        DPadRight,
        DPadUp,
        DPadDown,
        TriggerLeft,
        TriggerRight,
        LeftStickLeft,
        LeftStickRight,
        LeftStickUp,
        LeftStickDown,
        RightStickLeft,
        RightStickRight,
        RightStickUp,
        RightStickDown
    }

    public class GameControllerGesture : InputGesture
    {
        public static event EventHandler ConfirmationBindingChanged;
        public static event EventHandler CancellationBindingChanged;

        private static ControllerInput confirmationBinding = ControllerInput.A;
        public static ControllerInput ConfirmationBinding
        {
            get => confirmationBinding;
            set
            {
                confirmationBinding = value;
                ConfirmationBindingChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private static ControllerInput cancellationBinding = ControllerInput.B;
        public static ControllerInput CancellationBinding
        {
            get => cancellationBinding;
            set
            {
                cancellationBinding = value;
                CancellationBindingChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        private readonly ControllerInput button;

        public GameControllerGesture(ControllerInput button)
        {
            this.button = button;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (inputEventArgs is GameControllerInputEventArgs args)
            {
                return args.ButtonState == ControllerInputState.Pressed && args.Button == button;
            }
            else
            {
                return false;
            }
        }
    }

    [System.Runtime.InteropServices.Guid("36CB2F69-F227-4165-8CEE-6C10BC575524")]
    public class GameControllerManager : IDisposable
    {
        public class ButtonUpEventArgs
        {
            public ControllerInput Button { get; internal set; }
        }

        public class ButtonDownEventArgs
        {
            public ControllerInput Button { get; internal set; }
        }

        public class InputState
        {
            public Stopwatch Watch { get; set; } = new Stopwatch();
            public bool IsReSending { get; set; } = false;
        }

        private static readonly ILogger logger = LogManager.GetLogger();

        public bool SimulateNavigationKeys { get; set; } = true;
        public bool SimulateAllKeys { get; set; } = false;
        public bool StandardProcessingEnabled { get; set; } = true;

        private readonly ButtonUpEventArgs buttonUpEventArgs = new ButtonUpEventArgs();
        private readonly ButtonDownEventArgs buttonDownEventArgs = new ButtonDownEventArgs();

        public event EventHandler<ButtonUpEventArgs> ButtonUp;
        public event EventHandler<ButtonDownEventArgs> ButtonDown;
        public event EventHandler ControllersChanged;

        private readonly int resendDelay = 700;
        private readonly int resendRate = 80;

        private readonly Dictionary<ControllerInput, uint> keyboardMap = new Dictionary<ControllerInput, uint>()
        {
            {  ControllerInput.A, Winuser.VK_RETURN },
            {  ControllerInput.B, 0 },
            {  ControllerInput.Back, 0 },
            {  ControllerInput.DPadDown, Winuser.VK_DOWN },
            {  ControllerInput.DPadLeft, Winuser.VK_LEFT },
            {  ControllerInput.DPadRight, Winuser.VK_RIGHT },
            {  ControllerInput.DPadUp, Winuser.VK_UP },
            {  ControllerInput.Guide, 0 },
            {  ControllerInput.LeftShoulder, 0 },
            {  ControllerInput.LeftStick, 0 },
            {  ControllerInput.LeftStickDown, Winuser.VK_DOWN },
            {  ControllerInput.LeftStickLeft, Winuser.VK_LEFT },
            {  ControllerInput.LeftStickRight, Winuser.VK_RIGHT },
            {  ControllerInput.LeftStickUp, Winuser.VK_UP },
            {  ControllerInput.RightShoulder, 0 },
            {  ControllerInput.RightStick, 0 },
            {  ControllerInput.RightStickDown, 0 },
            {  ControllerInput.RightStickLeft, 0 },
            {  ControllerInput.RightStickRight, 0 },
            {  ControllerInput.RightStickUp, 0 },
            {  ControllerInput.Start, 0 },
            {  ControllerInput.TriggerLeft, Winuser.VK_PRIOR },
            {  ControllerInput.TriggerRight, Winuser.VK_NEXT },
            {  ControllerInput.X, 0 },
            {  ControllerInput.Y, 0 }
        };

        private readonly Dictionary<ControllerInput, InputState> keyWatches = new Dictionary<ControllerInput, InputState>()
        {
            {  ControllerInput.A, new InputState() },
            {  ControllerInput.B, new InputState() },
            {  ControllerInput.Back, new InputState() },
            {  ControllerInput.DPadDown, new InputState() },
            {  ControllerInput.DPadLeft, new InputState() },
            {  ControllerInput.DPadRight, new InputState() },
            {  ControllerInput.DPadUp, new InputState() },
            {  ControllerInput.Guide, new InputState() },
            {  ControllerInput.LeftShoulder, new InputState() },
            {  ControllerInput.LeftStick, new InputState() },
            {  ControllerInput.LeftStickDown, new InputState() },
            {  ControllerInput.LeftStickLeft, new InputState() },
            {  ControllerInput.LeftStickRight, new InputState() },
            {  ControllerInput.LeftStickUp, new InputState() },
            {  ControllerInput.RightShoulder, new InputState() },
            {  ControllerInput.RightStick, new InputState() },
            {  ControllerInput.RightStickDown, new InputState() },
            {  ControllerInput.RightStickLeft, new InputState() },
            {  ControllerInput.RightStickRight, new InputState() },
            {  ControllerInput.RightStickUp, new InputState() },
            {  ControllerInput.Start, new InputState() },
            {  ControllerInput.TriggerLeft, new InputState() },
            {  ControllerInput.TriggerRight, new InputState() },
            {  ControllerInput.X, new InputState() },
            {  ControllerInput.Y, new InputState() }
        };

        private readonly SynchronizationContext context;

        private readonly InputManager inputManager;
        private readonly PlayniteSettings settings;
        private bool isDisposed = false;

        public class LoadedGameController
        {
            public IntPtr Controller { get; }
            public IntPtr Joystic { get; }
            public int InstanceId { get; }
            public string Path { get; }
            public string Name { get; }
            public bool Enabled { get; set; } = true;

            public readonly Dictionary<ControllerInput, ControllerInputState> LastInputState = new Dictionary<ControllerInput, ControllerInputState>()
            {
                {  ControllerInput.A, ControllerInputState.Released },
                {  ControllerInput.B, ControllerInputState.Released },
                {  ControllerInput.Back, ControllerInputState.Released },
                {  ControllerInput.DPadDown, ControllerInputState.Released },
                {  ControllerInput.DPadLeft, ControllerInputState.Released },
                {  ControllerInput.DPadRight, ControllerInputState.Released },
                {  ControllerInput.DPadUp, ControllerInputState.Released },
                {  ControllerInput.Guide, ControllerInputState.Released },
                {  ControllerInput.LeftShoulder, ControllerInputState.Released },
                {  ControllerInput.LeftStick, ControllerInputState.Released },
                {  ControllerInput.LeftStickDown, ControllerInputState.Released },
                {  ControllerInput.LeftStickLeft, ControllerInputState.Released },
                {  ControllerInput.LeftStickRight, ControllerInputState.Released },
                {  ControllerInput.LeftStickUp, ControllerInputState.Released },
                {  ControllerInput.RightShoulder, ControllerInputState.Released },
                {  ControllerInput.RightStick, ControllerInputState.Released },
                {  ControllerInput.RightStickDown, ControllerInputState.Released },
                {  ControllerInput.RightStickLeft, ControllerInputState.Released },
                {  ControllerInput.RightStickRight, ControllerInputState.Released },
                {  ControllerInput.RightStickUp, ControllerInputState.Released },
                {  ControllerInput.Start, ControllerInputState.Released },
                {  ControllerInput.TriggerLeft, ControllerInputState.Released },
                {  ControllerInput.TriggerRight, ControllerInputState.Released },
                {  ControllerInput.X, ControllerInputState.Released },
                {  ControllerInput.Y, ControllerInputState.Released }
            };

            public LoadedGameController(IntPtr controller, IntPtr joystic, int instanceId, string path, string name)
            {
                Controller = controller;
                Joystic = joystic;
                InstanceId = instanceId;
                Path = path;
                Name = name;
            }
        }

        public readonly List<LoadedGameController> Controllers = new List<LoadedGameController>();

        public GameControllerManager(InputManager input, PlayniteSettings settings)
        {
            this.settings = settings;
            inputManager = input;
            context = SynchronizationContext.Current;

            LoadControllers();
        }

        public void ProcessInputs()
        {
            if (isDisposed)
            {
                return;
            }

            SDL_GameControllerUpdate();

            foreach (var controller in Controllers)
            {
                if (controller.Enabled)
                {
                    ProcessState(controller);
                }
            }
        }

        public void AddController(int joyIndex)
        {
            var controller = SDL_GameControllerOpen(joyIndex);
            var joystick = SDL_GameControllerGetJoystick(controller);
            var con = new LoadedGameController(controller, joystick, SDL_JoystickInstanceID(joystick), SDL_JoystickPath(joystick), SDL_JoystickName(joystick));

            if (settings.Fullscreen.UseOnlyLastAddedController)
            {
                con.Enabled = true;
                Controllers.ForEach(x => x.Enabled = false);
            }
            else
            {
                con.Enabled = !settings.Fullscreen.DisabledGameControllers.Contains(con.Path);
            }
     
            Controllers.Add(con);

            logger.Info($"added controller index {con.InstanceId}, {con.Name}");
            context.Send((a) => ControllersChanged?.Invoke(this, EventArgs.Empty), null);
        }

        public void LoadControllers()
        {
            for (int i = 0; i < SDL_NumJoysticks(); i++)
            {
                if (SDL_IsGameController(i) == SDL_bool.SDL_TRUE)
                {
                    AddController(i);
                }
            }
        }

        public void RemoveController(int instanceId)
        {
            var controller = Controllers.FirstOrDefault(a => a.InstanceId == instanceId);
            if (controller == null)
            {
                return;
            }

            SDL_GameControllerClose(controller.Controller);
            Controllers.Remove(controller);
            logger.Info($"removed controller {instanceId}, {controller.Name}");

            context.Send((a) => ControllersChanged?.Invoke(this, EventArgs.Empty), null);
        }

        public void RemoveAllControllers()
        {
            foreach (var controller in Controllers)
            {
                SDL_GameControllerClose(controller.Controller);
            }

            Controllers.Clear();
            context.Send((a) => ControllersChanged?.Invoke(this, EventArgs.Empty), null);
        }

        public void Dispose()
        {
            isDisposed = true;
            RemoveAllControllers();
        }
     

        private uint MapPadToKeyboard(ControllerInput input)
        {
            if (input == GameControllerGesture.ConfirmationBinding)
            {
                return Winuser.VK_RETURN;
            }
            else if (input == GameControllerGesture.CancellationBinding)
            {
                // I don't remember anymore why we don't map B to ESC, but it's probably because of some WPF FS mode hack BS
                return 0;
            }
            else
            {
                return keyboardMap[input];
            }
        }

        private void ProcessState(LoadedGameController controller)
        {
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A), ControllerInput.A, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B), ControllerInput.B, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK), ControllerInput.Back, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE), ControllerInput.Guide, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER), ControllerInput.LeftShoulder, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK), ControllerInput.LeftStick, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER), ControllerInput.RightShoulder, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK), ControllerInput.RightStick, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START), ControllerInput.Start, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X), ControllerInput.X, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y), ControllerInput.Y, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN), ControllerInput.DPadDown, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT), ControllerInput.DPadLeft, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT), ControllerInput.DPadRight, controller);
            ProcessButtonState(SDL_GameControllerGetButton(controller.Controller, SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP), ControllerInput.DPadUp, controller);

            ProcessAxisState(SDL_GameControllerGetAxis(controller.Controller, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT), ControllerInput.TriggerLeft, true, controller);
            ProcessAxisState(SDL_GameControllerGetAxis(controller.Controller, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT), ControllerInput.TriggerRight, true, controller);
            var state = SDL_GameControllerGetAxis(controller.Controller, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX);
            ProcessAxisState(state, ControllerInput.LeftStickLeft, false, controller);
            ProcessAxisState(state, ControllerInput.LeftStickRight, true, controller);
            state = SDL_GameControllerGetAxis(controller.Controller, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY);
            ProcessAxisState(state, ControllerInput.LeftStickUp, false, controller);
            ProcessAxisState(state, ControllerInput.LeftStickDown, true, controller);
            state = SDL_GameControllerGetAxis(controller.Controller, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX);
            ProcessAxisState(state, ControllerInput.RightStickLeft, false, controller);
            ProcessAxisState(state, ControllerInput.RightStickRight, true, controller);
            state = SDL_GameControllerGetAxis(controller.Controller, SDL_GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY);
            ProcessAxisState(state, ControllerInput.RightStickUp, false, controller);
            ProcessAxisState(state, ControllerInput.RightStickDown, true, controller);
        }

        private bool IsButtonNotNavigation(ControllerInput button)
        {
            return button == ControllerInput.A ||
                   button == ControllerInput.B ||
                   button == ControllerInput.Back ||
                   button == ControllerInput.Guide ||
                   button == ControllerInput.LeftShoulder ||
                   button == ControllerInput.LeftStick ||
                   button == ControllerInput.None ||
                   button == ControllerInput.RightShoulder ||
                   button == ControllerInput.RightStick ||
                   button == ControllerInput.Start ||
                   button == ControllerInput.X ||
                   button == ControllerInput.Y;
        }

        private bool ShouldResendKey(ControllerInput button)
        {
            var state = keyWatches[button];
            var sendInput = false;
            var elapsed = state.Watch.ElapsedMilliseconds;
            if (!state.Watch.IsRunning)
            {
                sendInput = true;
                state.Watch.Start();
            }
            else
            {
                if (IsButtonNotNavigation(button))
                {
                    sendInput = false;
                }
                else
                {
                    if (!state.IsReSending && elapsed < resendDelay)
                    {
                        sendInput = false;
                    }
                    else if (!state.IsReSending && elapsed > resendDelay)
                    {
                        state.IsReSending = true;
                        state.Watch.Restart();
                        sendInput = true;
                    }
                    else
                    {
                        if (state.IsReSending && elapsed > resendRate)
                        {
                            state.Watch.Restart();
                            sendInput = true;
                        }
                    }
                }
            }

            return sendInput;
        }

        private void ResetButtonResend(ControllerInput button)
        {
            var state = keyWatches[button];
            state.Watch.Reset();
            state.IsReSending = false;
        }

        private void ProcessButtonState(byte currentState, ControllerInput button, LoadedGameController controller)
        {
            var pressed = false;
            if (currentState == 1)
            {
                pressed = true;
            }
            else if (currentState < 0)
            {
                logger.Error($"Failed to get controller button state: {SDL_GetError()}");
            }

            if (IsButtonNotNavigation(button))
            {
                var lastState = controller.LastInputState[button];
                if (pressed && lastState == ControllerInputState.Released)
                {
                    SendControllerInput(button, true);
                    SimulateKeyInput(MapPadToKeyboard(button), true);
                }
                else if (!pressed && lastState == ControllerInputState.Pressed)
                {
                    SendControllerInput(button, false);
                    SimulateKeyInput(MapPadToKeyboard(button), false);
                }

                controller.LastInputState[button] = pressed ? ControllerInputState.Pressed : ControllerInputState.Released;
            }
            else
            {
                if (pressed && ShouldResendKey(button))
                {
                    SendControllerInput(button, true);
                    controller.LastInputState[button] = ControllerInputState.Pressed;
                    SimulateKeyInput(MapPadToKeyboard(button), true);
                }
                else if (!pressed && controller.LastInputState[button] == ControllerInputState.Pressed)
                {
                    ResetButtonResend(button);
                    SendControllerInput(button, false);
                    controller.LastInputState[button] = ControllerInputState.Released;
                    SimulateKeyInput(MapPadToKeyboard(button), false);
                }
            }
        }

        private void ProcessAxisState(short currentState, ControllerInput button, bool positive, LoadedGameController controller)
        {
            var pressed = false;
            // SDL2 docs:
            // The state is a value ranging from -32768 to 32767. Triggers, however, range from 0 to 32767 (they never return a negative value).
            if (button == ControllerInput.TriggerLeft || button == ControllerInput.TriggerRight)
            {
                pressed = currentState > 16383;
            }
            else
            {
                if (positive)
                {
                    pressed = currentState > 16383;
                }
                else
                {
                    pressed = currentState < -16383;
                }
            }

            if (pressed && ShouldResendKey(button))
            {
                SendControllerInput(button, true);
                controller.LastInputState[button] = ControllerInputState.Pressed;
                SimulateKeyInput(MapPadToKeyboard(button), true);
            }
            else if (!pressed && controller.LastInputState[button] == ControllerInputState.Pressed)
            {
                ResetButtonResend(button);
                SendControllerInput(button, false);
                controller.LastInputState[button] = ControllerInputState.Released;
                SimulateKeyInput(MapPadToKeyboard(button), false);
            }
        }

        private void SendControllerInput(ControllerInput button, bool pressed)
        {
            context.Post((a) =>
            {
                if (StandardProcessingEnabled)
                {
                    if (InputManager.Current.PrimaryKeyboardDevice?.ActiveSource == null)
                    {
                        return;
                    }

                    var args = new GameControllerInputEventArgs(Key.None, pressed ? ControllerInputState.Pressed : ControllerInputState.Released, button);
                    inputManager.ProcessInput(args);
                }

                if (pressed)
                {
                    buttonDownEventArgs.Button = button;
                    ButtonDown?.Invoke(null, buttonDownEventArgs);
                }
                else
                {
                    buttonUpEventArgs.Button = button;
                    ButtonUp?.Invoke(null, buttonUpEventArgs);
                }
            }, null);
        }

        private void SendKeyInput(uint key, bool pressed)
        {
            if (!StandardProcessingEnabled)
            {
                return;
            }

            if (key == 0)
            {
                return;
            }

            var windowHandle = IntPtr.Zero;
            context.Send((_) =>
            {
                var window = WindowManager.CurrentWindow;
                if (window == null)
                {
                    windowHandle = IntPtr.Zero;
                }
                else
                {
                    var helper = new WindowInteropHelper(window);
                    windowHandle = helper.Handle;
                }
            }, null);

            if (windowHandle == IntPtr.Zero)
            {
                return;
            }

            if (pressed)
            {
                User32.SendMessage(windowHandle, Winuser.WM_KEYDOWN, key, IntPtr.Zero);
            }
            else
            {
                User32.SendMessage(windowHandle, Winuser.WM_KEYUP, key, IntPtr.Zero);
            }
        }

        private void SimulateKeyInput(uint key, bool pressed)
        {
            if (!StandardProcessingEnabled)
            {
                return;
            }

            if (SimulateAllKeys || (SimulateNavigationKeys && IsKeysDirectionKey(key)))
            {
                SendKeyInput(key, pressed);
            }
        }

        private bool IsKeysDirectionKey(uint key)
        {
            switch (key)
            {
                case Winuser.VK_UP:
                case Winuser.VK_DOWN:
                case Winuser.VK_LEFT:
                case Winuser.VK_RIGHT:
                case Winuser.VK_PRIOR:
                case Winuser.VK_NEXT:
                    return true;
                default:
                    return false;
            }
        }
    }

    public class GameControllerInputEventArgs : KeyEventArgs
    {
        public ControllerInputState ButtonState
        {
            get; set;
        }

        public ControllerInput Button
        {
            get; set;
        }

        public GameControllerInputEventArgs(Key key, ControllerInputState state, ControllerInput button) :
            base(InputManager.Current.PrimaryKeyboardDevice, InputManager.Current.PrimaryKeyboardDevice.ActiveSource, Environment.TickCount, key)
        {
            ButtonState = state;
            Button = button;
            RoutedEvent = state == ControllerInputState.Pressed ? Keyboard.KeyDownEvent : Keyboard.KeyUpEvent;
        }
    }

    public class GameControllerInputBinding : InputBinding
    {
        public static readonly DependencyProperty ButtonProperty =
            DependencyProperty.Register(
                nameof(Button),
                typeof(ControllerInput),
                typeof(GameControllerInputBinding),
                new UIPropertyMetadata(ControllerInput.None, new PropertyChangedCallback(OnButtonPropertyChanged)));

        public ControllerInput Button
        {
            get
            {
                return (ControllerInput)GetValue(ButtonProperty);
            }
            set
            {
                SetValue(ButtonProperty, value);
            }
        }

        public override InputGesture Gesture
        {
            get
            {
                return base.Gesture as GameControllerGesture;
            }
            set
            {
                var gesture = value as GameControllerGesture;
                base.Gesture = gesture;
            }
        }

        public GameControllerInputBinding()
        {
        }

        public GameControllerInputBinding(ICommand command, ControllerInput button)
        {
            Command = command;
            Button = button;
        }

        private static void OnButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var binding = (GameControllerInputBinding)d;
            binding.Gesture = new GameControllerGesture((ControllerInput)e.NewValue);
        }
    }
}

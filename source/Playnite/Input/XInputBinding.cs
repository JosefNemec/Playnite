using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WindowsInput;
using WindowsInput.Native;
using XInputDotNetPure;

namespace Playnite.Input
{
    public enum XInputButtonState
    {
        Pressed,
        Released
    }

    public enum XInputButton
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

    public class XInputGesture : InputGesture
    {
        private XInputButton button;

        public XInputGesture(XInputButton button)
        {
            this.button = button;
        }

        public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
        {
            if (inputEventArgs is XInputEventArgs args)
            {
                return args.XButtonState == XInputButtonState.Pressed && args.XButton == button;
            }
            else
            {
                return false;
            }
        }
    }

    public class XInputDevice
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly InputSimulator inputSimulator = new InputSimulator();

        public class InputState
        {
            public Stopwatch Watch
            {
                get; set;
            } = new Stopwatch();

            public bool IsReSending
            {
                get; set;
            } = false;
        }

        public bool SimulateNavigationKeys
        {
            get; set;
        } = true;

        public bool SimulateAllKeys
        {
            get; set;
        } = false;

        private int pollingRate = 20;
        private int resendDelay = 700;
        private int resendRate = 80;

        private Dictionary<PlayerIndex, Dictionary<XInputButton, ButtonState>> prevStates =
            new Dictionary<PlayerIndex, Dictionary<XInputButton, ButtonState>>();

        private Dictionary<XInputButton, VirtualKeyCode> keyboardMap = new Dictionary<XInputButton, VirtualKeyCode>()
        {
            {  XInputButton.A, VirtualKeyCode.RETURN },
            {  XInputButton.B, 0 },
            {  XInputButton.Back, 0 },
            {  XInputButton.DPadDown, VirtualKeyCode.DOWN },
            {  XInputButton.DPadLeft, VirtualKeyCode.LEFT },
            {  XInputButton.DPadRight, VirtualKeyCode.RIGHT },
            {  XInputButton.DPadUp, VirtualKeyCode.UP },
            {  XInputButton.Guide, 0 },
            {  XInputButton.LeftShoulder, 0 },
            {  XInputButton.LeftStick, 0 },
            {  XInputButton.LeftStickDown, VirtualKeyCode.DOWN },
            {  XInputButton.LeftStickLeft, VirtualKeyCode.LEFT },
            {  XInputButton.LeftStickRight, VirtualKeyCode.RIGHT },
            {  XInputButton.LeftStickUp, VirtualKeyCode.UP },
            {  XInputButton.RightShoulder, 0 },
            {  XInputButton.RightStick, 0 },
            {  XInputButton.RightStickDown, 0 },
            {  XInputButton.RightStickLeft, 0 },
            {  XInputButton.RightStickRight, 0 },
            {  XInputButton.RightStickUp, 0 },
            {  XInputButton.Start, 0 },
            {  XInputButton.TriggerLeft, VirtualKeyCode.PRIOR },
            {  XInputButton.TriggerRight, VirtualKeyCode.NEXT },
            {  XInputButton.X, 0 },
            {  XInputButton.Y, 0 }
        };

        private Dictionary<XInputButton, InputState> keyWatches = new Dictionary<XInputButton, InputState>()
        {
            {  XInputButton.A, new InputState() },
            {  XInputButton.B, new InputState() },
            {  XInputButton.Back, new InputState() },
            {  XInputButton.DPadDown, new InputState() },
            {  XInputButton.DPadLeft, new InputState() },
            {  XInputButton.DPadRight, new InputState() },
            {  XInputButton.DPadUp, new InputState() },
            {  XInputButton.Guide, new InputState() },
            {  XInputButton.LeftShoulder, new InputState() },
            {  XInputButton.LeftStick, new InputState() },
            {  XInputButton.LeftStickDown, new InputState() },
            {  XInputButton.LeftStickLeft, new InputState() },
            {  XInputButton.LeftStickRight, new InputState() },
            {  XInputButton.LeftStickUp, new InputState() },
            {  XInputButton.RightShoulder, new InputState() },
            {  XInputButton.RightStick, new InputState() },
            {  XInputButton.RightStickDown, new InputState() },
            {  XInputButton.RightStickLeft, new InputState() },
            {  XInputButton.RightStickRight, new InputState() },
            {  XInputButton.RightStickUp, new InputState() },
            {  XInputButton.Start, new InputState() },
            {  XInputButton.TriggerLeft, new InputState() },
            {  XInputButton.TriggerRight, new InputState() },
            {  XInputButton.X, new InputState() },
            {  XInputButton.Y, new InputState() }
        };

        private readonly SynchronizationContext context;
        private readonly PlayniteApplication application;

        private InputManager inputManager;
        private uint lastState = 0;

        public XInputDevice(InputManager input, PlayniteApplication app)
        {
            foreach (PlayerIndex index in Enum.GetValues(typeof(PlayerIndex)))
            {
                prevStates.Add(index, new Dictionary<XInputButton, ButtonState>()
                {
                    {  XInputButton.A, ButtonState.Released },
                    {  XInputButton.B, ButtonState.Released },
                    {  XInputButton.Back, ButtonState.Released },
                    {  XInputButton.DPadDown, ButtonState.Released },
                    {  XInputButton.DPadLeft, ButtonState.Released },
                    {  XInputButton.DPadRight, ButtonState.Released },
                    {  XInputButton.DPadUp, ButtonState.Released },
                    {  XInputButton.Guide, ButtonState.Released },
                    {  XInputButton.LeftShoulder, ButtonState.Released },
                    {  XInputButton.LeftStick, ButtonState.Released },
                    {  XInputButton.LeftStickDown, ButtonState.Released },
                    {  XInputButton.LeftStickLeft, ButtonState.Released },
                    {  XInputButton.LeftStickRight, ButtonState.Released },
                    {  XInputButton.LeftStickUp, ButtonState.Released },
                    {  XInputButton.RightShoulder, ButtonState.Released },
                    {  XInputButton.RightStick, ButtonState.Released },
                    {  XInputButton.RightStickDown, ButtonState.Released },
                    {  XInputButton.RightStickLeft, ButtonState.Released },
                    {  XInputButton.RightStickRight, ButtonState.Released },
                    {  XInputButton.RightStickUp, ButtonState.Released },
                    {  XInputButton.Start, ButtonState.Released },
                    {  XInputButton.TriggerLeft, ButtonState.Released },
                    {  XInputButton.TriggerRight, ButtonState.Released },
                    {  XInputButton.X, ButtonState.Released },
                    {  XInputButton.Y, ButtonState.Released }
                });
            }

            inputManager = input;
            application = app;
            context = SynchronizationContext.Current;

            Task.Run(async () =>
            {
                while (true)
                {
                    if (app.IsActive != true)
                    {
                        await Task.Delay(pollingRate);
                        continue;
                    }

                    var state = GamePad.GetState(PlayerIndex.One);
                    if (state.IsConnected)
                    {
                        ProcessState(state, PlayerIndex.One);
                        await Task.Delay(pollingRate);
                    }

                    state = GamePad.GetState(PlayerIndex.Two);
                    if (state.IsConnected)
                    {
                        ProcessState(state, PlayerIndex.Two);
                        await Task.Delay(pollingRate);
                    }

                    state = GamePad.GetState(PlayerIndex.Three);
                    if (state.IsConnected)
                    {
                        ProcessState(state, PlayerIndex.Three);
                        await Task.Delay(pollingRate);
                    }

                    state = GamePad.GetState(PlayerIndex.Four);
                    if (state.IsConnected)
                    {
                        ProcessState(state, PlayerIndex.Four);
                        await Task.Delay(pollingRate);
                    }

                    await Task.Delay(pollingRate);
                }
            });
        }

        private void ProcessState(GamePadState state, PlayerIndex playniteIndex)
        {
            lastState = state.PacketNumber;

            ProcessButtonState(state.Buttons.A, XInputButton.A, playniteIndex);
            ProcessButtonState(state.Buttons.B, XInputButton.B, playniteIndex);
            ProcessButtonState(state.Buttons.Back, XInputButton.Back, playniteIndex);
            ProcessButtonState(state.Buttons.Guide, XInputButton.Guide, playniteIndex);
            ProcessButtonState(state.Buttons.LeftShoulder, XInputButton.LeftShoulder, playniteIndex);
            ProcessButtonState(state.Buttons.LeftStick, XInputButton.LeftStick, playniteIndex);
            ProcessButtonState(state.Buttons.RightShoulder, XInputButton.RightShoulder, playniteIndex);
            ProcessButtonState(state.Buttons.RightStick, XInputButton.RightStick, playniteIndex);
            ProcessButtonState(state.Buttons.Start, XInputButton.Start, playniteIndex);
            ProcessButtonState(state.Buttons.X, XInputButton.X, playniteIndex);
            ProcessButtonState(state.Buttons.Y, XInputButton.Y, playniteIndex);
            ProcessButtonState(state.DPad.Down, XInputButton.DPadDown, playniteIndex);
            ProcessButtonState(state.DPad.Left, XInputButton.DPadLeft, playniteIndex);
            ProcessButtonState(state.DPad.Right, XInputButton.DPadRight, playniteIndex);
            ProcessButtonState(state.DPad.Up, XInputButton.DPadUp, playniteIndex);
            ProcessAxisState(state.Triggers.Left, XInputButton.TriggerLeft, true, playniteIndex);
            ProcessAxisState(state.Triggers.Right, XInputButton.TriggerRight, true, playniteIndex);
            ProcessAxisState(state.ThumbSticks.Left.X, XInputButton.LeftStickLeft, false, playniteIndex);
            ProcessAxisState(state.ThumbSticks.Left.X, XInputButton.LeftStickRight, true, playniteIndex);
            ProcessAxisState(state.ThumbSticks.Left.Y, XInputButton.LeftStickUp, true, playniteIndex);
            ProcessAxisState(state.ThumbSticks.Left.Y, XInputButton.LeftStickDown, false, playniteIndex);
            ProcessAxisState(state.ThumbSticks.Right.X, XInputButton.RightStickLeft, false, playniteIndex);
            ProcessAxisState(state.ThumbSticks.Right.X, XInputButton.RightStickRight, true, playniteIndex);
            ProcessAxisState(state.ThumbSticks.Right.Y, XInputButton.RightStickUp, true, playniteIndex);
            ProcessAxisState(state.ThumbSticks.Right.Y, XInputButton.RightStickDown, false, playniteIndex);
        }

        private bool IsButtonNotNavigation(XInputButton button)
        {
            return button == XInputButton.A ||
                   button == XInputButton.B ||
                   button == XInputButton.Back ||
                   button == XInputButton.Guide ||
                   button == XInputButton.LeftShoulder ||
                   button == XInputButton.LeftStick ||
                   button == XInputButton.None ||
                   button == XInputButton.RightShoulder ||
                   button == XInputButton.RightStick ||
                   button == XInputButton.Start ||
                   button == XInputButton.X ||
                   button == XInputButton.Y;
        }

        private bool ShouldResendKey(XInputButton button)
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

        private void ResetButtonResend(XInputButton button)
        {
            var state = keyWatches[button];
            state.Watch.Reset();
            state.IsReSending = false;
        }

        private void ProcessButtonState(ButtonState currentState, XInputButton button, PlayerIndex playniteIndex)
        {
            if (currentState == ButtonState.Pressed && ShouldResendKey(button))
            {
                SendXInput(button, true);
                prevStates[playniteIndex][button] = ButtonState.Pressed;
                SimulateKeyInput(keyboardMap[button], true);
            }
            else if (currentState == ButtonState.Released && prevStates[playniteIndex][button] == ButtonState.Pressed)
            {
                ResetButtonResend(button);
                SendXInput(button, false);
                prevStates[playniteIndex][button] = ButtonState.Released;
                SimulateKeyInput(keyboardMap[button], false);
            }
        }

        private void ProcessAxisState(float currentState, XInputButton button, bool positive, PlayerIndex playniteIndex)
        {
            if (positive)
            {
                if (currentState > 0.5f && ShouldResendKey(button))
                {
                    SendXInput(button, true);
                    prevStates[playniteIndex][button] = ButtonState.Pressed;
                    SimulateKeyInput(keyboardMap[button], true);
                }
                else if (currentState < 0.5f && prevStates[playniteIndex][button] == ButtonState.Pressed)
                {
                    ResetButtonResend(button);
                    SendXInput(button, false);
                    prevStates[playniteIndex][button] = ButtonState.Released;
                    SimulateKeyInput(keyboardMap[button], false);
                }
            }
            else
            {
                if (currentState < -0.5f && ShouldResendKey(button))
                {
                    SendXInput(button, true);
                    prevStates[playniteIndex][button] = ButtonState.Pressed;
                    SimulateKeyInput(keyboardMap[button], true);
                }
                else if (currentState > -0.5f && prevStates[playniteIndex][button] == ButtonState.Pressed)
                {
                    ResetButtonResend(button);
                    SendXInput(button, false);
                    prevStates[playniteIndex][button] = ButtonState.Released;
                    SimulateKeyInput(keyboardMap[button], false);
                }
            }
        }

        private void SendXInput(XInputButton button, bool pressed)
        {
            context.Post((a) =>
            {
                if (InputManager.Current.PrimaryKeyboardDevice?.ActiveSource == null)
                {
                    return;
                }

                var args = new XInputEventArgs(Key.None, pressed ? XInputButtonState.Pressed : XInputButtonState.Released, button);
                inputManager.ProcessInput(args);
            }, null);
        }

        private void SendKeyInput(VirtualKeyCode key, bool pressed)
        {
            if (key == 0)
            {
                return;
            }

            if (pressed)
            {
                inputSimulator.Keyboard.KeyDown(key);
            }
            else
            {
                inputSimulator.Keyboard.KeyUp(key);
            }
        }

        private void SimulateKeyInput(VirtualKeyCode key, bool pressed)
        {
            if (SimulateAllKeys || (SimulateNavigationKeys && IsKeysDirectionKey(key)))
            {
                SendKeyInput(key, pressed);
            }
        }

        private bool IsKeysDirectionKey(VirtualKeyCode key)
        {
            switch (key)
            {
                case VirtualKeyCode.UP:
                case VirtualKeyCode.DOWN:
                case VirtualKeyCode.LEFT:
                case VirtualKeyCode.RIGHT:
                case VirtualKeyCode.PRIOR:
                case VirtualKeyCode.NEXT:
                    return true;
                default:
                    return false;
            }
        }
    }

    public class XInputEventArgs : KeyEventArgs
    {
        public XInputButtonState XButtonState
        {
            get; set;
        }

        public XInputButton XButton
        {
            get; set;
        }

        public XInputEventArgs(Key key, XInputButtonState state, XInputButton button) :
            base(InputManager.Current.PrimaryKeyboardDevice, InputManager.Current.PrimaryKeyboardDevice.ActiveSource, Environment.TickCount, key)
        {
            XButtonState = state;
            XButton = button;
            RoutedEvent = state == XInputButtonState.Pressed ? Keyboard.KeyDownEvent : Keyboard.KeyUpEvent;
        }
    }

    public class XInputBinding : InputBinding
    {
        public static readonly DependencyProperty ButtonProperty =
            DependencyProperty.Register(
                nameof(Button),
                typeof(XInputButton),
                typeof(XInputBinding),
                new UIPropertyMetadata(XInputButton.None, new PropertyChangedCallback(OnButtonPropertyChanged)));

        public XInputButton Button
        {
            get
            {
                return (XInputButton)GetValue(ButtonProperty);
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
                return base.Gesture as XInputGesture;
            }
            set
            {
                var gesture = value as XInputGesture;
                base.Gesture = gesture;
            }
        }

        public XInputBinding()
        {
        }

        public XInputBinding(ICommand command, XInputButton button)
        {
            Command = command;
            Button = button;
        }

        private static void OnButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var binding = (XInputBinding)d;
            binding.Gesture = new XInputGesture((XInputButton)e.NewValue);
        }
    }
}

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
using XInputDotNetPure;

namespace PlayniteUI
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

        private int pollingRate = 40;
        private int resendDelay = 700;
        private int resendRate = 80;

        private Dictionary<XInputButton, ButtonState> prevStates = new Dictionary<XInputButton, ButtonState>()
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
        };

        private Dictionary<XInputButton, Key> keyboardMap = new Dictionary<XInputButton, Key>()
        {
            {  XInputButton.A, Key.Enter },
            {  XInputButton.B, Key.None },
            {  XInputButton.Back, Key.None },
            {  XInputButton.DPadDown, Key.Down },
            {  XInputButton.DPadLeft, Key.Left },
            {  XInputButton.DPadRight, Key.Right },
            {  XInputButton.DPadUp, Key.Up },
            {  XInputButton.Guide, Key.None },
            {  XInputButton.LeftShoulder, Key.None },
            {  XInputButton.LeftStick, Key.None },
            {  XInputButton.LeftStickDown, Key.Down },
            {  XInputButton.LeftStickLeft, Key.Left },
            {  XInputButton.LeftStickRight, Key.Right },
            {  XInputButton.LeftStickUp, Key.Up },
            {  XInputButton.RightShoulder, Key.None },
            {  XInputButton.RightStick, Key.None },
            {  XInputButton.RightStickDown, Key.None },
            {  XInputButton.RightStickLeft, Key.None },
            {  XInputButton.RightStickRight, Key.None },
            {  XInputButton.RightStickUp, Key.None },
            {  XInputButton.Start, Key.None },
            {  XInputButton.TriggerLeft, Key.PageUp },
            {  XInputButton.TriggerRight, Key.PageDown },
            {  XInputButton.X, Key.None },
            {  XInputButton.Y, Key.None }
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
        private InputManager inputManager;
        private uint lastState = 0;

        public XInputDevice(InputManager input)
        {            
            inputManager = input;
            context = SynchronizationContext.Current;           
            
            Task.Run(async () =>
            {
                while (true)
                {
                    if (App.CurrentApp?.IsActive != true)
                    {
                        await Task.Delay(pollingRate);
                        continue;
                    }

                    var state = GamePad.GetState(PlayerIndex.One);
                    if (state.IsConnected)
                    {
                        ProcessState(state);
                        await Task.Delay(pollingRate);
                    }

                    state = GamePad.GetState(PlayerIndex.Two);
                    if (state.IsConnected)
                    {
                        ProcessState(state);
                        await Task.Delay(pollingRate);
                    }

                    state = GamePad.GetState(PlayerIndex.Three);
                    if (state.IsConnected)
                    {
                        ProcessState(state);
                        await Task.Delay(pollingRate);
                    }

                    state = GamePad.GetState(PlayerIndex.Four);
                    if (state.IsConnected)
                    {
                        ProcessState(state);
                        await Task.Delay(pollingRate);
                    }

                    await Task.Delay(pollingRate);
                }
            });
        }

        private void ProcessState(GamePadState state)
        {
            lastState = state.PacketNumber;

            ProcessButtonState(state.Buttons.A, XInputButton.A);
            ProcessButtonState(state.Buttons.B, XInputButton.B);
            ProcessButtonState(state.Buttons.Back, XInputButton.Back);
            ProcessButtonState(state.Buttons.Guide, XInputButton.Guide);
            ProcessButtonState(state.Buttons.LeftShoulder, XInputButton.LeftShoulder);
            ProcessButtonState(state.Buttons.LeftStick, XInputButton.LeftStick);
            ProcessButtonState(state.Buttons.RightShoulder, XInputButton.RightShoulder);
            ProcessButtonState(state.Buttons.RightStick, XInputButton.RightStick);
            ProcessButtonState(state.Buttons.Start, XInputButton.Start);
            ProcessButtonState(state.Buttons.X, XInputButton.X);
            ProcessButtonState(state.Buttons.Y, XInputButton.Y);
            ProcessButtonState(state.DPad.Down, XInputButton.DPadDown);
            ProcessButtonState(state.DPad.Left, XInputButton.DPadLeft);
            ProcessButtonState(state.DPad.Right, XInputButton.DPadRight);
            ProcessButtonState(state.DPad.Up, XInputButton.DPadUp);
            ProcessAxisState(state.Triggers.Left, XInputButton.TriggerLeft, true);
            ProcessAxisState(state.Triggers.Right, XInputButton.TriggerRight, true);
            ProcessAxisState(state.ThumbSticks.Left.X, XInputButton.LeftStickLeft, false);
            ProcessAxisState(state.ThumbSticks.Left.X, XInputButton.LeftStickRight, true);
            ProcessAxisState(state.ThumbSticks.Left.Y, XInputButton.LeftStickUp, true);
            ProcessAxisState(state.ThumbSticks.Left.Y, XInputButton.LeftStickDown, false);
            ProcessAxisState(state.ThumbSticks.Right.X, XInputButton.RightStickLeft, false);
            ProcessAxisState(state.ThumbSticks.Right.X, XInputButton.RightStickRight, true);
            ProcessAxisState(state.ThumbSticks.Right.Y, XInputButton.RightStickUp, true);
            ProcessAxisState(state.ThumbSticks.Right.Y, XInputButton.RightStickDown, false);
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
            
            return sendInput;
        }

        private void ResetButtonResend(XInputButton button)
        {
            var state = keyWatches[button];
            state.Watch.Reset();
            state.IsReSending = false;
        }

        private void ProcessButtonState(ButtonState currentState, XInputButton button)
        {            
            if (currentState == ButtonState.Pressed && ShouldResendKey(button))
            {
                SendXInput(button, true);
                prevStates[button] = ButtonState.Pressed;
                SimulateKeyInput(keyboardMap[button], true);
            }
            else if (currentState == ButtonState.Released && prevStates[button] == ButtonState.Pressed)
            {
                ResetButtonResend(button);
                SendXInput(button, false);
                prevStates[button] = ButtonState.Released;
                SimulateKeyInput(keyboardMap[button], false);
            }
        }

        private void ProcessAxisState(float currentState, XInputButton button, bool positive)
        {
            if (positive)
            {
                if (currentState > 0.5f && ShouldResendKey(button))
                {
                    SendXInput(button, true);
                    prevStates[button] = ButtonState.Pressed;
                    SimulateKeyInput(keyboardMap[button], true);
                }
                else if (currentState < 0.5f && prevStates[button] == ButtonState.Pressed)
                {
                    ResetButtonResend(button);
                    SendXInput(button, false);
                    prevStates[button] = ButtonState.Released;
                    SimulateKeyInput(keyboardMap[button], false);
                }
            }
            else
            {
                if (currentState < -0.5f && ShouldResendKey(button))
                {
                    SendXInput(button, true);
                    prevStates[button] = ButtonState.Pressed;
                    SimulateKeyInput(keyboardMap[button], true);
                }
                else if (currentState > -0.5f && prevStates[button] == ButtonState.Pressed)
                {
                    ResetButtonResend(button);
                    SendXInput(button, false);
                    prevStates[button] = ButtonState.Released;
                    SimulateKeyInput(keyboardMap[button], false);
                }
            }
        }

        private void SendXInput(XInputButton button, bool pressed)
        {
            context.Post((a) =>
            {
                if (InputManager.Current.PrimaryKeyboardDevice.ActiveSource == null)
                {
                    return;
                }

                var args = new XInputEventArgs(Key.None, pressed ? XInputButtonState.Pressed : XInputButtonState.Released, button);
                inputManager.ProcessInput(args);
            }, null);
        }

        private void SendKeyInput(Key key, bool pressed)
        {
            context.Post((a) =>
            {
                if (InputManager.Current.PrimaryKeyboardDevice.ActiveSource == null)
                {
                    return;
                }

                if (InputManager.Current == null)
                {
                    // Should happen only in very rare cases
                    logger.Warn("Can't send key input, no input manager exits right now.");
                }
                else
                {
                    var args = new KeyEventArgs(InputManager.Current.PrimaryKeyboardDevice, InputManager.Current.PrimaryKeyboardDevice.ActiveSource, Environment.TickCount, key)
                    {
                        RoutedEvent = pressed ? Keyboard.KeyDownEvent : Keyboard.KeyUpEvent
                    };

                    inputManager.ProcessInput(args);
                }
            }, null);
        }

        private void SimulateKeyInput(Key key, bool pressed)
        {
            if (SimulateAllKeys || (SimulateNavigationKeys && IsKeysDirectionKey(key)))
            {                
                SendKeyInput(key, pressed);
            }
        }

        private bool IsKeysDirectionKey(Key key)
        {
            switch (key)
            {
                case Key.Up:
                case Key.Down:
                case Key.Left:
                case Key.Right:
                case Key.PageDown:
                case Key.PageUp:
                case Key.Return:
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
            DependencyProperty.Register("Button", typeof(XInputButton), typeof(XInputBinding), new UIPropertyMetadata(XInputButton.None, new PropertyChangedCallback(OnButtonPropertyChanged)));

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

        private static void OnButtonPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var binding = (XInputBinding)d;
            binding.Gesture = new XInputGesture((XInputButton)e.NewValue);
        }
    }
}

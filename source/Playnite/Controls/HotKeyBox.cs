using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Playnite.Controls
{
    public class HotKeyBox : TextBox
    {
        public static readonly DependencyProperty HotkeyProperty = DependencyProperty.Register(
            nameof(Hotkey),
            typeof(HotKey),
            typeof(HotKeyBox),
            new FrameworkPropertyMetadata(default(HotKey), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public HotKey Hotkey
        {
            get => (HotKey)GetValue(HotkeyProperty);
            set => SetValue(HotkeyProperty, value);
        }

        public bool ClearWithDeleteKeys { get; set; } = true;

        static HotKeyBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HotKeyBox), new FrameworkPropertyMetadata(typeof(HotKeyBox)));
        }

        public HotKeyBox() : base()
        {
            PreviewKeyDown += HotKeyBox_PreviewKeyDown;
            IsReadOnly = true;
            IsReadOnlyCaretVisible = false;
            IsUndoEnabled = false;

            BindingTools.SetBinding(
                this,
                TextProperty,
                this,
                nameof(Hotkey),
                System.Windows.Data.BindingMode.OneWay,
                targetNullValue: ResourceProvider.GetResource(LOC.None));
        }

        private void HotKeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            var modifiers = Keyboard.Modifiers;
            var key = e.Key;
            if (key == Key.System)
            {
                key = e.SystemKey;
            }

            if (modifiers == ModifierKeys.None &&
                (key == Key.Delete || key == Key.Back || key == Key.Escape) &&
                ClearWithDeleteKeys)
            {
                Hotkey = null;
                return;
            }

            if (key == Key.LeftCtrl ||
                key == Key.RightCtrl ||
                key == Key.LeftAlt ||
                key == Key.RightAlt ||
                key == Key.LeftShift ||
                key == Key.RightShift ||
                key == Key.LWin ||
                key == Key.RWin ||
                key == Key.Clear ||
                key == Key.OemClear ||
                key == Key.Apps)
            {
                return;
            }

            Hotkey = new HotKey(key, modifiers);
        }
    }
}

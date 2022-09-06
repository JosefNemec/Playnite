using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Playnite
{
    public class HotKey
    {
        public Key Key { get; }
        public ModifierKeys Modifiers { get; }

        public HotKey(Key key, ModifierKeys modifiers)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public override string ToString()
        {
            var str = "";
            if (Modifiers.HasFlag(ModifierKeys.Control))
            {
                str += "Ctrl + ";
            }

            if (Modifiers.HasFlag(ModifierKeys.Shift))
            {
                str += "Shift + ";
            }

            if (Modifiers.HasFlag(ModifierKeys.Alt))
            {
                str += "Alt + ";
            }

            if (Modifiers.HasFlag(ModifierKeys.Windows))
            {
                str += "Win + ";
            }

            return str += Key.ToString();
        }
    }
}

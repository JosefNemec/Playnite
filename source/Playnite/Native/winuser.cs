using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Native
{
    public static class Winuser
    {
        public static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        public static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        public static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        public static readonly IntPtr HWND_TOP = new IntPtr(0);
        public static readonly IntPtr HWND_BOTTOM = new IntPtr(1);
        public const uint WM_QUERYENDSESSION = 0x11;
        public const uint WM_ENDSESSION = 0x16;
        public const uint ENDSESSION_CLOSEAPP = 0x1;

        // http://msdn.microsoft.com/en-us/library/windows/desktop/ms646244(v=vs.85).aspx
        public const uint WM_MOUSEACTIVATE = 0x0021;
        public const uint WM_MOUSEMOVE = 0x0200;
        public const uint WM_LBUTTONDOWN = 0x0201;
        public const uint WM_LBUTTONUP = 0x0202;
        public const uint WM_LBUTTONDBLCLK = 0x0203;
        public const uint WM_RBUTTONDOWN = 0x0204;
        public const uint WM_RBUTTONUP = 0x0205;
        public const uint WM_RBUTTONDBLCLK = 0x0206;
        public const uint WM_MBUTTONDOWN = 0x0207;
        public const uint WM_MBUTTONUP = 0x0208;
        public const uint WM_MBUTTONDBLCLK = 0x0209;

        public const uint MK_CONTROL = 0x0008;    // The CTRL key is down.
        public const uint MK_LBUTTON = 0x0001;    // The left mouse button is down.
        public const uint MK_MBUTTON = 0x0010;    // The middle mouse button is down.
        public const uint MK_RBUTTON = 0x0002;    // The right mouse button is down.
        public const uint MK_SHIFT = 0x0004;    // The SHIFT key is down.

        public const uint WM_KEYDOWN = 0x0100;
        public const uint WM_KEYUP = 0x0101;
        public const uint WM_SYSKEYDOWN = 0x0104;
        public const uint WM_SYSKEYUP = 0x0105;

        // http://msdn.microsoft.com/en-us/library/windows/desktop/dd375731(v=vs.85).aspx
        public const uint VK_LBUTTON = 0x01; // Left mouse button
        public const uint VK_RBUTTON = 0x02; // Right mouse button
        public const uint VK_CANCEL = 0x03; // Control-break processing
        public const uint VK_MBUTTON = 0x04; // Middle mouse button (three-button mouse)
        public const uint VK_XBUTTON1 = 0x05; // X1 mouse button
        public const uint VK_XBUTTON2 = 0x06; // X2 mouse button
        public const uint VK_0x07 = 0x07; // Undefined
        public const uint VK_BACK = 0x08; // BACKSPACE key
        public const uint VK_TAB = 0x09; // TAB key
        public const uint VK_0x0A = 0x0A; // Reserved
        public const uint VK_0x0B = 0x0B; // Reserved
        public const uint VK_CLEAR = 0x0C; // CLEAR key
        public const uint VK_RETURN = 0x0D; // ENTER key
        public const uint VK_0x0E = 0x0E; // Undefined
        public const uint VK_0x0F = 0x0F; // Undefined
        public const uint VK_SHIFT = 0x10; // SHIFT key
        public const uint VK_CONTROL = 0x11; // CTRL key
        public const uint VK_MENU = 0x12; // ALT key
        public const uint VK_PAUSE = 0x13; // PAUSE key
        public const uint VK_CAPITAL = 0x14; // CAPS LOCK key
        public const uint VK_KANA = 0x15; // IME Kana mode
        public const uint VK_HANGUEL = 0x15; // IME Hanguel mode (maintained for compatibility; use VK_HANGUL)
        public const uint VK_HANGUL = 0x15; // IME Hangul mode
        public const uint VK_0x16 = 0x16; // Undefined
        public const uint VK_JUNJA = 0x17; // IME Junja mode
        public const uint VK_FINAL = 0x18; // IME final mode
        public const uint VK_HANJA = 0x19; // IME Hanja mode
        public const uint VK_KANJI = 0x19; // IME Kanji mode
        public const uint VK_0x1A = 0x1A; // Undefined
        public const uint VK_ESCAPE = 0x1B; // ESC key
        public const uint VK_CONVERT = 0x1C; // IME convert
        public const uint VK_NONCONVERT = 0x1D; // IME nonconvert
        public const uint VK_ACCEPT = 0x1E; // IME accept
        public const uint VK_MODECHANGE = 0x1F; // IME mode change request
        public const uint VK_SPACE = 0x20; // SPACEBAR
        public const uint VK_PRIOR = 0x21; // PAGE UP key
        public const uint VK_NEXT = 0x22; // PAGE DOWN key
        public const uint VK_END = 0x23; // END key
        public const uint VK_HOME = 0x24; // HOME key
        public const uint VK_LEFT = 0x25; // LEFT ARROW key
        public const uint VK_UP = 0x26; // UP ARROW key
        public const uint VK_RIGHT = 0x27; // RIGHT ARROW key
        public const uint VK_DOWN = 0x28; // DOWN ARROW key
        public const uint VK_SELECT = 0x29; // SELECT key
        public const uint VK_PRINT = 0x2A; // PRINT key
        public const uint VK_EXECUTE = 0x2B; // EXECUTE key
        public const uint VK_SNAPSHOT = 0x2C; // PRINT SCREEN key
        public const uint VK_INSERT = 0x2D; // INS key
        public const uint VK_DELETE = 0x2E; // DEL key
        public const uint VK_HELP = 0x2F; // HELP key
        public const uint VK_0x30 = 0x30; // 0 key
        public const uint VK_0x31 = 0x31; // 1 key
        public const uint VK_0x32 = 0x32; // 2 key
        public const uint VK_0x33 = 0x33; // 3 key
        public const uint VK_0x34 = 0x34; // 4 key
        public const uint VK_0x35 = 0x35; // 5 key
        public const uint VK_0x36 = 0x36; // 6 key
        public const uint VK_0x37 = 0x37; // 7 key
        public const uint VK_0x38 = 0x38; // 8 key
        public const uint VK_0x39 = 0x39; // 9 key
        public const uint VK_0x3A = 0x3A; // Undefined
        public const uint VK_0x3B = 0x3B; // Undefined
        public const uint VK_0x3C = 0x3C; // Undefined
        public const uint VK_0x3D = 0x3D; // Undefined
        public const uint VK_0x3E = 0x3E; // Undefined
        public const uint VK_0x3F = 0x3F; // Undefined
        public const uint VK_0x40 = 0x40; // Undefined
        public const uint VK_0x41 = 0x41; // A key
        public const uint VK_0x42 = 0x42; // B key
        public const uint VK_0x43 = 0x43; // C key
        public const uint VK_0x44 = 0x44; // D key
        public const uint VK_0x45 = 0x45; // E key
        public const uint VK_0x46 = 0x46; // F key
        public const uint VK_0x47 = 0x47; // G key
        public const uint VK_0x48 = 0x48; // H key
        public const uint VK_0x49 = 0x49; // I key
        public const uint VK_0x4A = 0x4A; // J key
        public const uint VK_0x4B = 0x4B; // K key
        public const uint VK_0x4C = 0x4C; // L key
        public const uint VK_0x4D = 0x4D; // M key
        public const uint VK_0x4E = 0x4E; // N key
        public const uint VK_0x4F = 0x4F; // O key
        public const uint VK_0x50 = 0x50; // P key
        public const uint VK_0x51 = 0x51; // Q key
        public const uint VK_0x52 = 0x52; // R key
        public const uint VK_0x53 = 0x53; // S key
        public const uint VK_0x54 = 0x54; // T key
        public const uint VK_0x55 = 0x55; // U key
        public const uint VK_0x56 = 0x56; // V key
        public const uint VK_0x57 = 0x57; // W key
        public const uint VK_0x58 = 0x58; // X key
        public const uint VK_0x59 = 0x59; // Y key
        public const uint VK_0x5A = 0x5A; // Z key
        public const uint VK_LWIN = 0x5B; // Left Windows key (Natural keyboard)
        public const uint VK_RWIN = 0x5C; // Right Windows key (Natural keyboard)
        public const uint VK_APPS = 0x5D; // Applications key (Natural keyboard)
        public const uint VK_0x5E = 0x5E; // Reserved
        public const uint VK_SLEEP = 0x5F; // Computer Sleep key
        public const uint VK_NUMPAD0 = 0x60; // Numeric keypad 0 key
        public const uint VK_NUMPAD1 = 0x61; // Numeric keypad 1 key
        public const uint VK_NUMPAD2 = 0x62; // Numeric keypad 2 key
        public const uint VK_NUMPAD3 = 0x63; // Numeric keypad 3 key
        public const uint VK_NUMPAD4 = 0x64; // Numeric keypad 4 key
        public const uint VK_NUMPAD5 = 0x65; // Numeric keypad 5 key
        public const uint VK_NUMPAD6 = 0x66; // Numeric keypad 6 key
        public const uint VK_NUMPAD7 = 0x67; // Numeric keypad 7 key
        public const uint VK_NUMPAD8 = 0x68; // Numeric keypad 8 key
        public const uint VK_NUMPAD9 = 0x69; // Numeric keypad 9 key
        public const uint VK_MULTIPLY = 0x6A; // Multiply key
        public const uint VK_ADD = 0x6B; // Add key
        public const uint VK_SEPARATOR = 0x6C; // Separator key
        public const uint VK_SUBTRACT = 0x6D; // Subtract key
        public const uint VK_DECIMAL = 0x6E; // Decimal key
        public const uint VK_DIVIDE = 0x6F; // Divide key
        public const uint VK_F1 = 0x70; // F1 key
        public const uint VK_F2 = 0x71; // F2 key
        public const uint VK_F3 = 0x72; // F3 key
        public const uint VK_F4 = 0x73; // F4 key
        public const uint VK_F5 = 0x74; // F5 key
        public const uint VK_F6 = 0x75; // F6 key
        public const uint VK_F7 = 0x76; // F7 key
        public const uint VK_F8 = 0x77; // F8 key
        public const uint VK_F9 = 0x78; // F9 key
        public const uint VK_F10 = 0x79; // F10 key
        public const uint VK_F11 = 0x7A; // F11 key
        public const uint VK_F12 = 0x7B; // F12 key
        public const uint VK_F13 = 0x7C; // F13 key
        public const uint VK_F14 = 0x7D; // F14 key
        public const uint VK_F15 = 0x7E; // F15 key
        public const uint VK_F16 = 0x7F; // F16 key
        public const uint VK_F17 = 0x80; // F17 key
        public const uint VK_F18 = 0x81; // F18 key
        public const uint VK_F19 = 0x82; // F19 key
        public const uint VK_F20 = 0x83; // F20 key
        public const uint VK_F21 = 0x84; // F21 key
        public const uint VK_F22 = 0x85; // F22 key
        public const uint VK_F23 = 0x86; // F23 key
        public const uint VK_F24 = 0x87; // F24 key
        public const uint VK_0x88 = 0x88; // Unassigned
        public const uint VK_0x89 = 0x89; // Unassigned
        public const uint VK_0x8A = 0x8A; // Unassigned
        public const uint VK_0x8B = 0x8B; // Unassigned
        public const uint VK_0x8C = 0x8C; // Unassigned
        public const uint VK_0x8D = 0x8D; // Unassigned
        public const uint VK_0x8E = 0x8E; // Unassigned
        public const uint VK_0x8F = 0x8F; // Unassigned
        public const uint VK_NUMLOCK = 0x90; // NUM LOCK key
        public const uint VK_SCROLL = 0x91; // SCROLL LOCK key
        public const uint VK_0x92 = 0x92; // OEM specific
        public const uint VK_0x93 = 0x93; // OEM specific
        public const uint VK_0x94 = 0x94; // OEM specific
        public const uint VK_0x95 = 0x95; // OEM specific
        public const uint VK_0x96 = 0x96; // OEM specific
        public const uint VK_0x97 = 0x97; // Unassigned
        public const uint VK_0x98 = 0x98; // Unassigned
        public const uint VK_0x99 = 0x99; // Unassigned
        public const uint VK_0x9A = 0x9A; // Unassigned
        public const uint VK_0x9B = 0x9B; // Unassigned
        public const uint VK_0x9C = 0x9C; // Unassigned
        public const uint VK_0x9D = 0x9D; // Unassigned
        public const uint VK_0x9E = 0x9E; // Unassigned
        public const uint VK_0x9F = 0x9F; // Unassigned
        public const uint VK_LSHIFT = 0xA0; // Left SHIFT key
        public const uint VK_RSHIFT = 0xA1; // Right SHIFT key
        public const uint VK_LCONTROL = 0xA2; // Left CONTROL key
        public const uint VK_RCONTROL = 0xA3; // Right CONTROL key
        public const uint VK_LMENU = 0xA4; // Left MENU key
        public const uint VK_RMENU = 0xA5; // Right MENU key
        public const uint VK_BROWSER_BACK = 0xA6; // Browser Back key
        public const uint VK_BROWSER_FORWARD = 0xA7; // Browser Forward key
        public const uint VK_BROWSER_REFRESH = 0xA8; // Browser Refresh key
        public const uint VK_BROWSER_STOP = 0xA9; // Browser Stop key
        public const uint VK_BROWSER_SEARCH = 0xAA; // Browser Search key
        public const uint VK_BROWSER_FAVORITES = 0xAB; // Browser Favorites key
        public const uint VK_BROWSER_HOME = 0xAC; // Browser Start and Home key
        public const uint VK_VOLUME_MUTE = 0xAD; // Volume Mute key
        public const uint VK_VOLUME_DOWN = 0xAE; // Volume Down key
        public const uint VK_VOLUME_UP = 0xAF; // Volume Up key
        public const uint VK_MEDIA_NEXT_TRACK = 0xB0; // Next Track key
        public const uint VK_MEDIA_PREV_TRACK = 0xB1; // Previous Track key
        public const uint VK_MEDIA_STOP = 0xB2; // Stop Media key
        public const uint VK_MEDIA_PLAY_PAUSE = 0xB3; // Play/Pause Media key
        public const uint VK_LAUNCH_MAIL = 0xB4; // Start Mail key
        public const uint VK_LAUNCH_MEDIA_SELECT = 0xB5; // Select Media key
        public const uint VK_LAUNCH_APP1 = 0xB6; // Start Application 1 key
        public const uint VK_LAUNCH_APP2 = 0xB7; // Start Application 2 key
        public const uint VK_0xB8 = 0xB8; // Reserved
        public const uint VK_0xB9 = 0xB9; // Reserved
        public const uint VK_OEM_1 = 0xBA; // Used for miscellaneous characters; it can vary by keyboard. For the US standard keyboard, the ';:' key
        public const uint VK_OEM_PLUS = 0xBB; // For any country/region, the '+' key
        public const uint VK_OEM_COMMA = 0xBC; // For any country/region, the ',' key
        public const uint VK_OEM_MINUS = 0xBD; // For any country/region, the '-' key
        public const uint VK_OEM_PERIOD = 0xBE; // For any country/region, the '.' key
        public const uint VK_OEM_2 = 0xBF; // Used for miscellaneous characters; it can vary by keyboard.
        public const uint VK_OEM_3 = 0xC0; // Used for miscellaneous characters; it can vary by keyboard.
        public const uint VK_0xC1 = 0xC1; // Reserved
        public const uint VK_0xC2 = 0xC2; // Reserved
        public const uint VK_0xC3 = 0xC3; // Reserved
        public const uint VK_0xC4 = 0xC4; // Reserved
        public const uint VK_0xC5 = 0xC5; // Reserved
        public const uint VK_0xC6 = 0xC6; // Reserved
        public const uint VK_0xC7 = 0xC7; // Reserved
        public const uint VK_0xC8 = 0xC8; // Reserved
        public const uint VK_0xC9 = 0xC9; // Reserved
        public const uint VK_0xCA = 0xCA; // Reserved
        public const uint VK_0xCB = 0xCB; // Reserved
        public const uint VK_0xCC = 0xCC; // Reserved
        public const uint VK_0xCD = 0xCD; // Reserved
        public const uint VK_0xCE = 0xCE; // Reserved
        public const uint VK_0xCF = 0xCF; // Reserved
        public const uint VK_0xD0 = 0xD0; // Reserved
        public const uint VK_0xD1 = 0xD1; // Reserved
        public const uint VK_0xD2 = 0xD2; // Reserved
        public const uint VK_0xD3 = 0xD3; // Reserved
        public const uint VK_0xD4 = 0xD4; // Reserved
        public const uint VK_0xD5 = 0xD5; // Reserved
        public const uint VK_0xD6 = 0xD6; // Reserved
        public const uint VK_0xD7 = 0xD7; // Reserved
        public const uint VK_0xD8 = 0xD8; // Unassigned
        public const uint VK_0xD9 = 0xD9; // Unassigned
        public const uint VK_0xDA = 0xDA; // Unassigned
        public const uint VK_OEM_4 = 0xDB; // Used for miscellaneous characters; it can vary by keyboard.
        public const uint VK_OEM_5 = 0xDC; // Used for miscellaneous characters; it can vary by keyboard.
        public const uint VK_OEM_6 = 0xDD; // Used for miscellaneous characters; it can vary by keyboard.
        public const uint VK_OEM_7 = 0xDE; // Used for miscellaneous characters; it can vary by keyboard.
        public const uint VK_OEM_8 = 0xDF; // Used for miscellaneous characters; it can vary by keyboard.
        public const uint VK_0xE0 = 0xE0; // Reserved
        public const uint VK_0xE1 = 0xE1; // OEM specific
        public const uint VK_OEM_102 = 0xE2; // Either the angle bracket key or the backslash key on the RT 102-key keyboard
        public const uint VK_0xE3 = 0xE3; // OEM specific
        public const uint VK_0xE4 = 0xE4; // OEM specific
        public const uint VK_PROCESSKEY = 0xE5; // IME PROCESS key
        public const uint VK_0xE6 = 0xE6; // OEM specific
        public const uint VK_PACKET = 0xE7; // Used to pass Unicode characters as if they were keystrokes. The VK_PACKET key is the low word of a 32-bit Virtual Key value used for non-keyboard input methods. For more information, see Remark in KEYBDINPUT,SendInput, WM_KEYDOWN, and WM_KEYUP
        public const uint VK_0xE8 = 0xE8; // Unassigned
        public const uint VK_0xE9 = 0xE9; // OEM specific
        public const uint VK_0xEA = 0xEA; // OEM specific
        public const uint VK_0xEB = 0xEB; // OEM specific
        public const uint VK_0xEC = 0xEC; // OEM specific
        public const uint VK_0xED = 0xED; // OEM specific
        public const uint VK_0xEE = 0xEE; // OEM specific
        public const uint VK_0xEF = 0xEF; // OEM specific
        public const uint VK_0xF0 = 0xF0; // OEM specific
        public const uint VK_0xF1 = 0xF1; // OEM specific
        public const uint VK_0xF2 = 0xF2; // OEM specific
        public const uint VK_0xF3 = 0xF3; // OEM specific
        public const uint VK_0xF4 = 0xF4; // OEM specific
        public const uint VK_0xF5 = 0xF5; // OEM specific
        public const uint VK_ATTN = 0xF6; // Attn key
        public const uint VK_CRSEL = 0xF7; // CrSel key
        public const uint VK_EXSEL = 0xF8; // ExSel key
        public const uint VK_EREOF = 0xF9; // Erase EOF key
        public const uint VK_PLAY = 0xFA; // Play key
        public const uint VK_ZOOM = 0xFB; // Zoom key
        public const uint VK_NONAME = 0xFC; // Reserved
        public const uint VK_PA1 = 0xFD; // PA1 key
        public const uint VK_OEM_CLEAR = 0xFE; // Clear key
    }

    public enum QUERY_DEVICE_CONFIG_FLAGS : uint
    {
        QDC_ALL_PATHS = 0x00000001,
        QDC_ONLY_ACTIVE_PATHS = 0x00000002,
        QDC_DATABASE_CURRENT = 0x00000004
    }

    [Flags]
    public enum SWP
    {
        ASYNCWINDOWPOS = 0x4000,
        DEFERERASE = 0x2000,
        DRAWFRAME = 0x0020,
        FRAMECHANGED = 0x0020,
        HIDEWINDOW = 0x0080,
        NOACTIVATE = 0x0010,
        NOCOPYBITS = 0x0100,
        NOMOVE = 0x0002,
        NOOWNERZORDER = 0x0200,
        NOREDRAW = 0x0008,
        NOREPOSITION = 0x0200,
        NOSENDCHANGING = 0x0400,
        NOSIZE = 0x0001,
        NOZORDER = 0x0004,
        SHOWWINDOW = 0x0040,
        TOPMOST = NOACTIVATE | NOOWNERZORDER | NOSIZE | NOMOVE | NOREDRAW | NOSENDCHANGING
    }

    public enum MonitorOptions : uint
    {
        MONITOR_DEFAULTTONULL = 0x00000000,
        MONITOR_DEFAULTTOPRIMARY = 0x00000001,
        MONITOR_DEFAULTTONEAREST = 0x00000002
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class MONITORINFO
    {
        public int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
        public RECT rcMonitor = new RECT();
        public RECT rcWork = new RECT();
        public int dwFlags = 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }
}

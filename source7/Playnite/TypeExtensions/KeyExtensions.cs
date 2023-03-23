namespace System.Windows.Input;

public static class KeyExtensions
{
    public static bool IsNumericKey(this Key key)
    {
        return (
            key == Key.D0 ||
            key == Key.D1 ||
            key == Key.D2 ||
            key == Key.D3 ||
            key == Key.D4 ||
            key == Key.D5 ||
            key == Key.D6 ||
            key == Key.D7 ||
            key == Key.D8 ||
            key == Key.D9 ||
            key == Key.NumPad0 ||
            key == Key.NumPad1 ||
            key == Key.NumPad2 ||
            key == Key.NumPad3 ||
            key == Key.NumPad4 ||
            key == Key.NumPad5 ||
            key == Key.NumPad6 ||
            key == Key.NumPad7 ||
            key == Key.NumPad8 ||
            key == Key.NumPad9);
    }
}

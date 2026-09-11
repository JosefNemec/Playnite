namespace Playnite.BrowserProcess;

public class Program
{
    public static int Main(string[] args)
    {
        return CefSharp.BrowserSubprocess.SelfHost.Main(args);
    }
}

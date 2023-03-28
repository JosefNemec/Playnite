using System.Diagnostics;
using System.IO;

namespace Playnite.Tests;

public class TempDirectory : IDisposable
{
    private readonly bool autoDelete;

    public string TempPath { get; private set; }

    public static TempDirectory Create(bool autoDelete = true, string? dirName = null)
    {
        if (dirName.IsNullOrEmpty())
        {
            var stack = new StackTrace(1);
            var method = stack.GetFrame(0)!.GetMethod();
            dirName = Paths.GetSafePathName($"{method!.DeclaringType!.Name}_{method.Name}");
        }

        return new TempDirectory(dirName, autoDelete);
    }

    public TempDirectory(string dirName, bool autoDelete = true)
    {
        TempPath = Path.Combine(Path.GetTempPath(), "Playnite", dirName);
        FileSystem.CreateDirectory(TempPath, true);
        this.autoDelete = autoDelete;
    }

    public void Dispose()
    {
        if (autoDelete)
        {
            FileSystem.DeleteDirectory(TempPath);
        }
    }

    public override string ToString()
    {
        return TempPath;
    }
}

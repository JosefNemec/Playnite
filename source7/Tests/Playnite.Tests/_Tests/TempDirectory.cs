using System.Diagnostics;
using System.IO;

namespace Playnite.Tests;

public class TempDirectory : IDisposable
{
    private readonly bool autoDelete;

    public string TempDir { get; private set; }

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
        TempDir = Path.Combine(Path.GetTempPath(), "Playnite", dirName);
        FileSystem.CreateDirectory(TempDir, true);
        this.autoDelete = autoDelete;
    }

    public void Dispose()
    {
        if (autoDelete)
        {
            FileSystem.DeleteDirectory(TempDir);
        }
    }

    public override string ToString()
    {
        return TempDir;
    }

    public static implicit operator string(TempDirectory dir) => dir.TempDir;
}

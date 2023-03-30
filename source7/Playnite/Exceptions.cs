using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Playnite;

/// <summary>
/// Represents exception from scripting runtime.
/// </summary>
public class ScriptRuntimeException : Exception
{
    /// <summary>
    /// Gets script runtime stack trace.
    /// </summary>
    public string? ScriptStackTrace { get; private set; }

    /// <summary>
    /// Creates new instance of <see cref="ScriptRuntimeException"/>.
    /// </summary>
    public ScriptRuntimeException() : base()
    {
    }

    /// <summary>
    /// Creates new instance of <see cref="ScriptRuntimeException"/>.
    /// </summary>
    /// <param name="message"></param>
    public ScriptRuntimeException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates new instance of <see cref="ScriptRuntimeException"/>.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="stackTrace"></param>
    public ScriptRuntimeException(string message, string stackTrace) : base(message)
    {
        ScriptStackTrace = stackTrace;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return base.ToString() + "\n--- script trace ---\n" + ScriptStackTrace;
    }
}

public class NotSupportedInFullscreenException : Exception
{
    public NotSupportedInFullscreenException() : base("Not supported in Fullscreen mode.")
    {
    }

    public NotSupportedInFullscreenException(string message) : base(message)
    {
    }
}

//public class ExceptionInfo
//{
//    public bool IsLiteDbCorruptionCrash;
//    public bool IsExtensionCrash;
//    //public ExtensionManifest CrashExtension;
//}

//public class Exceptions
//{
//    private static readonly ILogger logger = LogManager.GetLogger();

//    public static ExceptionInfo GetExceptionInfo(Exception exception)
//    {
//        try
//        {
//            var playniteStackCalls = 0;
//            var stack = new StackTrace(exception);
//            var crashModules = new List<Module>();
//            foreach (var frame in stack.GetFrames())
//            {
//                var module = frame.GetMethod()?.Module;
//                if (module == null)
//                {
//                    continue;
//                }

//                if (module.Name.StartsWith("Playnite", StringComparison.Ordinal))
//                {
//                    playniteStackCalls++;
//                }

//                crashModules.AddMissing(module);
//            }

//            if (exception.InnerException != null)
//            {
//                stack = new StackTrace(exception.InnerException);
//                foreach (var frame in stack.GetFrames())
//                {
//                    var module = frame.GetMethod()?.Module;
//                    if (module == null)
//                    {
//                        continue;
//                    }

//                    if (module.Name.StartsWith("Playnite", StringComparison.Ordinal))
//                    {
//                        playniteStackCalls++;
//                    }

//                    crashModules.AddMissing(module);
//                }
//            }

//            LoadedPlugin extDesc = null;
//            foreach (var module in crashModules)
//            {
//                extDesc = extensions?.Plugins?.FirstOrDefault(a =>
//                    module.Name == a.Value.Description.Module ||
//                    Paths.AreEqual(a.Value.Description.DirectoryPath, Path.GetDirectoryName(module.Assembly.Location))).Value;
//                if (extDesc != null)
//                {
//                    break;
//                }
//            }

//            var liteDbCrash = Regex.IsMatch(exception.Message, @"'LiteDB\..+?Page'");
//            if (extDesc != null)
//            {
//                return new ExceptionInfo
//                {
//                    IsExtensionCrash = true,
//                    CrashExtension = extDesc.Description,
//                    IsLiteDbCorruptionCrash = liteDbCrash
//                };
//            }
//            else
//            {
//                // This usually happens if an exception occurs in XAML because of faulty custom theme.
//                // The only stack entry would be Playnite's entry point or no entry at all.
//                if (playniteStackCalls == 0 || playniteStackCalls == 1)
//                {
//                    return new ExceptionInfo
//                    {
//                        IsExtensionCrash = true,
//                        IsLiteDbCorruptionCrash = liteDbCrash
//                    };
//                }
//                else
//                {
//                    return new ExceptionInfo
//                    {
//                        IsLiteDbCorruptionCrash = liteDbCrash
//                    };
//                }
//            }
//        }
//        catch (Exception e) when (!AppConfig.ThrowAllErrors)
//        {
//            logger.Error(e, "Failed check crash stack trace.");
//            return new ExceptionInfo();
//        }
//    }
//}

using Playnite.API;
using Playnite.Common;
using Playnite.Plugins;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class NotSupportedInFullscreenException : Exception
    {
        public NotSupportedInFullscreenException() : base("Not supported in Fullscreen mode.")
        {
        }
        public NotSupportedInFullscreenException(string message) : base(message)
        {
        }
    }

    public class ExceptionInfo
    {
        public bool IsExtensionCrash;
        public ExtensionManifest CrashExtension;
    }

    public class Exceptions
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public static ExceptionInfo GetExceptionInfo(Exception exception, ExtensionFactory extensions)
        {
            try
            {
                var playniteStackCalls = 0;
                var stack = new StackTrace(exception);
                var crashModules = new List<Module>();
                foreach (var frame in stack.GetFrames())
                {
                    var module = frame.GetMethod()?.Module;
                    if (module == null)
                    {
                        continue;
                    }

                    if (module.Name.StartsWith("Playnite"))
                    {
                        playniteStackCalls++;
                    }

                    crashModules.AddMissing(module);
                }

                if (exception.InnerException != null)
                {
                    stack = new StackTrace(exception.InnerException);
                    foreach (var frame in stack.GetFrames())
                    {
                        var module = frame.GetMethod()?.Module;
                        if (module == null)
                        {
                            continue;
                        }

                        if (module.Name.StartsWith("Playnite"))
                        {
                            playniteStackCalls++;
                        }

                        crashModules.AddMissing(module);
                    }
                }

                var extDesc = extensions?.Plugins?.FirstOrDefault(a =>
                    crashModules.FirstOrDefault(m => m.Name ==
                        a.Value.Description.Module ||
                        Paths.AreEqual(a.Value.Description.DirectoryPath, Path.GetDirectoryName(m.Assembly.Location))) != null).Value;
                if (extDesc != null)
                {
                    return new ExceptionInfo
                    {
                        IsExtensionCrash = true,
                        CrashExtension = extDesc.Description
                    };
                }
                else
                {
                    // This usually happens if an exception occurs in XAML because of faulty custom theme.
                    // The only stack entry would be Playnite's entry point or no entry at all.
                    if (playniteStackCalls == 0 || playniteStackCalls == 1)
                    {
                        return new ExceptionInfo { IsExtensionCrash = true };
                    }
                    else
                    {
                        return new ExceptionInfo();
                    }
                }
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed check crash stack trace.");
                return new ExceptionInfo();
            }
        }
    }
}

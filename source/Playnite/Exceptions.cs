using Playnite.API;
using Playnite.Plugins;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
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
                var stack = new StackTrace(exception);
                var crashModules = new List<string>();
                foreach (var frame in stack.GetFrames())
                {
                    crashModules.AddMissing(frame.GetMethod().Module.Name);
                }

                if (exception.InnerException != null)
                {
                    stack = new StackTrace(exception.InnerException);
                    foreach (var frame in stack.GetFrames())
                    {
                        crashModules.AddMissing(frame.GetMethod().Module.Name);
                    }
                }

                var extDesc = extensions.Plugins.FirstOrDefault(a => crashModules.ContainsString(a.Value.Description.Module, StringComparison.OrdinalIgnoreCase)).Value;
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
                    return new ExceptionInfo();
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

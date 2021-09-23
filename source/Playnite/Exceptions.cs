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
                var crashModules = new List<Module>();
                foreach (var frame in stack.GetFrames())
                {
                    crashModules.AddMissing(frame.GetMethod().Module);
                }

                if (exception.InnerException != null)
                {
                    stack = new StackTrace(exception.InnerException);
                    foreach (var frame in stack.GetFrames())
                    {
                        crashModules.AddMissing(frame.GetMethod().Module);
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

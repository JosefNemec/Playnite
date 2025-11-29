using Playnite.API;
using Playnite.Common;
using Playnite.Plugins;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;

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

    public class NotSupportedInDesktopException : Exception
    {
        public NotSupportedInDesktopException() : base("Not supported in Desktop mode.")
        {
        }
        public NotSupportedInDesktopException(string message) : base(message)
        {
        }
    }

    public class ExceptionInfo
    {
        public bool IsLiteDbCorruptionCrash;
        public bool IsExtensionCrash;
        public ExtensionManifest CrashExtension;
        public int PlayniteStackCalls;
    }

    public class Exceptions
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public static ExceptionInfo GetExceptionInfo(Exception exception, ExtensionFactory extensions)
        {
            ExceptionInfo innerCrash = null;
            if (exception.InnerException != null)
            {
                innerCrash = GetExceptionInfoImpl(exception.InnerException, extensions);
                if (innerCrash.IsExtensionCrash || innerCrash.IsLiteDbCorruptionCrash)
                    return innerCrash;
            }

            var crashInfo = GetExceptionInfoImpl(exception.InnerException, extensions);
            // This usually happens if an exception occurs in XAML because of faulty custom theme.
            // The only stack entry would be Playnite's entry point or no entry at all.
            if ((innerCrash?.PlayniteStackCalls ?? 0 + crashInfo.PlayniteStackCalls) <= 1)
                crashInfo.IsExtensionCrash = true;

            return crashInfo;
        }

        private static ExceptionInfo GetExceptionInfoImpl(Exception exception, ExtensionFactory extensions)
        {
            var crashInfo = new ExceptionInfo();

            try
            {
                if (// Seems to happen with extensions that use reflection that fails at runtime
                    exception is RuntimeBinderException ||
                    // This happens with systems that use extensions/themes with integrated media player
                    // but the actual system player used by media player is broken somehow.
                    exception.StackTrace?.Contains("MediaPlayerState") == true ||
                    // Seems to happen with script extensions somehow calling PowerShell, or from PS, via blocking ProgressDialog
                    exception is PSInvalidOperationException)
                {
                    crashInfo.IsExtensionCrash = true;
                    return crashInfo;
                }

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
                        crashInfo.PlayniteStackCalls++;
                    }

                    crashModules.AddMissing(module);
                }

                LoadedPlugin extDesc = null;
                foreach (var module in crashModules)
                {
                    extDesc = extensions?.Plugins?.FirstOrDefault(a =>
                        module.Name == a.Value.Description.Module ||
                        Paths.AreEqual(a.Value.Description.DirectoryPath, Path.GetDirectoryName(module.Assembly.Location))).Value;
                    if (extDesc != null)
                    {
                        break;
                    }
                }

                var liteDbCrash = exception is LiteDB.LiteException || exception.Message.Contains("LiteDB.");
                crashInfo.IsLiteDbCorruptionCrash = liteDbCrash;
                if (extDesc != null)
                {
                    crashInfo.IsExtensionCrash = true;
                    crashInfo.CrashExtension = extDesc.Description;
                }

                return crashInfo;
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed check crash stack trace.");
                return crashInfo;
            }
        }
    }
}

using Playnite.Scripting.PowerShell;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Scripting
{
    public enum SupportedMenuMethods
    {
        GameMenu,
        MainMenu
    }

    public class ScriptFunctionExport : ExtensionFunction
    {
        public string FunctionName
        {
            get; set;
        }

        public PlayniteScript Script
        {
            get; set;
        }

        public ScriptFunctionExport(string name, string functionName, PlayniteScript script) : base (name)
        {
            Name = name;
            FunctionName = functionName;
            Script = script;
        }

        public override string ToString()
        {
            return Name;
        }

        public override void Invoke()
        {
            Script.InvokeExportedFunction(this);
        }
    }

    public abstract class PlayniteScript: IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();
        public List<ApplicationEvent> SupportedEvents { get; internal set; }
        public List<SupportedMenuMethods> SupportedMenus { get; internal set; }

        public string Path
        {
            get; private set;
        }

        private string name;
        public string Name
        {
            get => name ?? System.IO.Path.GetFileName(Path);
        }

        public PlayniteScript(string path, string name = null)
        {
            Path = path;
            this.name = name;
        }

        public virtual List<ScriptGameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            return new List<ScriptGameMenuItem>();
        }

        public virtual List<ScriptMainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            return new List<ScriptMainMenuItem>();
        }

        public static PlayniteScript FromFile(string path, string name)
        {
            var extension = System.IO.Path.GetExtension(path).ToLower();
            if (extension == ".psm1" || extension == ".psd1")
            {
                if (PowerShellRuntime.IsInstalled)
                {
                    return new PowerShellScript(path, name);
                }
                else
                {
                    logger.Warn("Cannot load PowerShell script, PowerShell 3+ not installed.");
                    return null;
                }
            }
            else
            {
                throw new Exception("Cannot load script file, uknown format.");
            }
        }

        public override string ToString()
        {
            return System.IO.Path.GetFileName(Path);
        }

        public virtual void Dispose()
        {
        }

        public abstract object InvokeFunction(string functionName);
        public abstract object InvokeFunction(string functionName, List<object> arguments);
        public abstract void InvokeExportedFunction(ScriptFunctionExport function);
        public abstract void SetVariable(string name, object value);
        public abstract void OnApplicationStarted();
        public abstract void OnApplicationStopped();
        public abstract void OnLibraryUpdated();
        public abstract void OnGameStarting(OnGameStartingEventArgs args);
        public abstract void OnGameStarted(OnGameStartedEventArgs args);
        public abstract void OnGameStopped(OnGameStoppedEventArgs args);
        public abstract void OnGameInstalled(OnGameInstalledEventArgs args);
        public abstract void OnGameUninstalled(OnGameUninstalledEventArgs args);
        public abstract void OnGameSelected(OnGameSelectedEventArgs args);
        public abstract void OnGameStartupCancelled(OnGameStartupCancelledEventArgs args);
    }
}

using Playnite.Scripting.IronPython;
using Playnite.Scripting.PowerShell;
using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Scripting
{
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

        public List<ScriptFunctionExport> FunctionExports
        {
            get; set;
        }

        public string Path
        {
            get; private set;
        }

        public string Name
        {
            get => System.IO.Path.GetFileName(Path);
        }

        public PlayniteScript(string path)
        {
            Path = path;
        }

        public static PlayniteScript FromFile(string path)
        {
            var extension = System.IO.Path.GetExtension(path).ToLower();
            if (extension == ".py")
            {
                return new IronPythonScript(path);
            }
            else if (extension == ".ps1")
            {
                if (PowerShellRuntime.IsInstalled)
                {
                    return new PowerShellScript(path);
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

        public abstract void InvokeExportedFunction(ScriptFunctionExport function);
        public abstract void SetVariable(string name, object value);
        public abstract void OnApplicationStarted();
        public abstract void OnApplicationStopped();
        public abstract void OnLibraryUpdated();
        public abstract void OnGameStarting(Game game);
        public abstract void OnGameStarted(Game game);
        public abstract void OnGameStopped(Game game, long ellapsedSeconds);
        public abstract void OnGameInstalled(Game game);
        public abstract void OnGameUninstalled(Game game);
        public abstract void OnGameSelected(GameSelectionEventArgs args);
    }
}

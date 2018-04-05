using Playnite.Models;
using Playnite.Scripting.IronPython;
using Playnite.Scripting.PowerShell;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Scripting
{
    public enum ScriptLanguage
    {
        PowerShell,
        IronPython
    }

    public class ScriptFunctionExport : IExtensionFunction
    {
        public string Name
        {
            get; set;
        }

        public string FunctionName
        {
            get; set;
        }

        public PlayniteScript Script
        {
            get; set;
        }

        public ScriptFunctionExport(string name, string functionName, PlayniteScript script)
        {
            Name = name;
            FunctionName = functionName;
            Script = script;
        }

        public override string ToString()
        {
            return Name;
        }

        public void Invoke()
        {
            Script.InvokeExportedFunction(this);
        }
    }

    public abstract class PlayniteScript: IDisposable
    {
        public Dictionary<string, string> Attributes
        {
            get; set;
        }

        public List<ScriptFunctionExport> FunctionExports
        {
            get; set;
        }

        public ScriptLanguage Language
        {
            get; private set;
        }

        public string Path
        {
            get; private set;
        }

        public string Name
        {
            get => System.IO.Path.GetFileName(Path);
        }

        public PlayniteScript(string path, ScriptLanguage language)
        {
            Path = path;
            Language = language;
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
                return new PowerShellScript(path);
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
        public abstract void OnScriptLoaded();
        public abstract void OnGameStarted(IGame game);
        public abstract void OnGameStopped(IGame game, long ellapsedSeconds);
        public abstract void OnGameInstalled(IGame game);
        public abstract void OnGameUninstalled(IGame game);
    }
}

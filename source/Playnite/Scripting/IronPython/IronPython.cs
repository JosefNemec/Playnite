using IronPython.Hosting;
using IronPython.Modules;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Playnite.API;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Scripting.IronPython
{
    public class IronPythonRuntime : IScriptRuntime
    {
        /// <summary>
        /// Ultra stupid solution to force VS to copy IronPython.Modules.dll to output dir when debugging.
        /// More at https://stackoverflow.com/questions/15816769/dependent-dll-is-not-getting-copied-to-the-build-output-folder-in-visual-studio/24828522
        /// </summary>
        private object z = new ArrayModule.array("c");

        private static NLog.Logger logger = NLog.LogManager.GetLogger("Python");
        private ScriptEngine engine;
        private ScriptScope scope;
        private ScriptScope pythonBuiltins;

        public IronPythonRuntime()
        {
            Dictionary<string, object> options = new Dictionary<string, object>();
            if (Debugger.IsAttached)
            {
                options["Debug"] = true;
            }
            engine = Python.CreateEngine(options);
            var paths = engine.GetSearchPaths();
            paths.Add(PlaynitePaths.ProgramPath);
            var stdLibPath = Path.Combine(PlaynitePaths.ProgramPath, "IronPythonStdLib.zip");
            if (File.Exists(stdLibPath))
            {
                paths.Add(stdLibPath);
            }
            engine.SetSearchPaths(paths);
            pythonBuiltins = engine.GetBuiltinModule();
            engine.Runtime.LoadAssembly(typeof(Playnite.SDK.Models.Game).Assembly);
            engine.Execute("from Playnite.SDK.Models import *", pythonBuiltins);
            scope = engine.CreateScope();
            SetVariable("__logger", new Logger("Python"));
        }

        public void Dispose()
        {
            engine.Runtime.Shutdown();
        }

        public object Execute(string script)
        {
            return Execute(script, null);
        }

        public object Execute(string script, Dictionary<string, object> variables)
        {
            if (variables != null)
            {
                foreach (var key in variables.Keys)
                {
                    // Set the variable inside the scope (current module), not globally in __builtins__
                    scope.SetVariable(key, variables[key]);
                }
            }

            var source = engine.CreateScriptSourceFromString(script, SourceCodeKind.AutoDetect);            
            var result = source.Execute<object>(scope);
            return result;
        }

        public void ExecuteFile(string path)
        {
            // Python search paths contain the current directory
            // Change to the directory of the script to allow importing other modules
            FileInfo fileInfo = new FileInfo(path);
            Directory.SetCurrentDirectory(fileInfo.DirectoryName);

            // Replace the current scope with the new module
            scope = engine.ImportModule(Path.GetFileNameWithoutExtension(fileInfo.Name));
        }

        public object GetVariable(string name)
        {
            try
            {
                return scope.GetVariable<object>(name);
            }
            catch (MissingMemberException)
            {
                try
                {
                    return pythonBuiltins.GetVariable<object>(name);
                }
                catch (MissingMemberException)
                {
                    return null;
                }

            }
        }

        public void SetVariable(string name, object value)
        {
            // Set the variable inside Python's __builtins__ module
            // This is a set of global variables accessible in all Python modules
            pythonBuiltins.SetVariable(name, value);
        }

        public bool GetFunctionExits(string name)
        {
            return scope.GetVariableNames().Contains(name);
        }
    }
}

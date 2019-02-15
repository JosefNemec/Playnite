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

        public IronPythonRuntime(string name)
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
            engine.Runtime.LoadAssembly(typeof(Playnite.SDK.Models.Game).Assembly);
            scope = engine.CreateScope();
            SetVariable("__logger", new Logger("Python"));
        }

        public void Dispose()
        {
            engine.Runtime.Shutdown();
        }

        public void ImportModule(string path)
        {
            // Python search paths contain the current directory
            // Change to the directory of the script to allow importing other modules
            FileInfo fileInfo = new FileInfo(path);
            Directory.SetCurrentDirectory(fileInfo.DirectoryName);

            // Replace the current scope with the new module
            scope = engine.ImportModule(Path.GetFileNameWithoutExtension(fileInfo.Name));
        }

        public void SetVariable(string name, object value)
        {
            // Set the variable inside IronPython's Globals
            // These variables may be imported in Python scripts
            engine.Runtime.Globals.SetVariable(name, value);
        }

        public bool GetFunctionExits(string name)
        {
            try
            {
                return engine.Operations.IsCallable(scope.GetVariable(name));
            }
            catch (MissingMemberException)
            {
                return false;
            }
        }

        public object CallFunction(string name, params object[] args)
        {
            return engine.Operations.Invoke(scope.GetVariable(name), args);
        }
    }
}

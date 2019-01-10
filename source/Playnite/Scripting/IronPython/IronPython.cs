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
            scope = engine.CreateScope();
            engine.Execute(string.Format(@"
import clr
import os
os.chdir('{0}')
clr.AddReferenceToFile(""Playnite.SDK.dll"")
from Playnite.SDK.Models import *
", PlaynitePaths.ProgramPath.Replace(Path.DirectorySeparatorChar, '/')), scope);

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
                    scope.SetVariable(key, variables[key]);
                }
            }

            var source = engine.CreateScriptSourceFromString(script, SourceCodeKind.AutoDetect);            
            var result = source.Execute<object>(scope);
            return result;
        }

        public void ExecuteFile(string path)
        {
            engine.ExecuteFile(path, scope);
        }

        public object GetVariable(string name)
        {
            try
            {
                return scope.GetVariable<object>(name);
            }
            catch (MissingMemberException)
            {
                return null;
            }
        }

        public void SetVariable(string name, object value)
        {
            scope.SetVariable(name, value);
        }

        public bool GetFunctionExits(string name)
        {
            return engine.Execute<bool>($"'{name}' in globals()", scope);
        }
    }
}

using IronPython.Hosting;
using IronPython.Modules;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using Playnite.API;
using Playnite.SDK.Exceptions;
using Playnite.Settings;
using System;
using System.Collections.Generic;
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
            engine = Python.CreateEngine();
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
clr.AddReference('PresentationFramework')
from Playnite.SDK.Models import *
", PlaynitePaths.ProgramPath.Replace(Path.DirectorySeparatorChar, '/')), scope);

            SetVariable("__logger", new Logger("Python"));
        }

        public void Dispose()
        {
            engine.Runtime.Shutdown();
        }

        public object Execute(string script, string workDir = null)
        {
            return Execute(script, null, workDir);
        }

        public object Execute(string script, Dictionary<string, object> variables, string workDir = null)
        {
            if (variables != null)
            {
                foreach (var key in variables.Keys)
                {
                    scope.SetVariable(key, variables[key]);
                }
            }

            var source = engine.CreateScriptSourceFromString(script, SourceCodeKind.AutoDetect);
            var currentDir = string.Empty;
            if (!workDir.IsNullOrEmpty())
            {
                currentDir = engine.Execute<string>("os.getcwd()", scope);
                var dir = workDir.Replace(Path.DirectorySeparatorChar, '/');
                engine.Execute($"os.chdir('{dir}')", scope);
            }

            try
            {
                object result = null;
                try
                {
                    result = source.Execute<object>(scope);
                }
                catch (Exception e)
                {
                    var ext = engine.GetService<ExceptionOperations>().FormatException(e);
                    throw new ScriptRuntimeException(e.Message, ext);
                }

                return result;
            }
            finally
            {
                if (!workDir.IsNullOrEmpty())
                {
                    currentDir = currentDir.Replace(Path.DirectorySeparatorChar, '/');
                    engine.Execute($"os.chdir('{currentDir}')", scope);
                }
            }
        }

        public object ExecuteFile(string path, string workDir = null)
        {
            return engine.ExecuteFile(path, scope);
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

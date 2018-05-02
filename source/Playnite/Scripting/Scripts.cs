using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Scripting
{
    public class Scripts
    {
        private const string powerShellFolder = "PowerShell";
        private const string pythonFolder = "IronPython";

        public static void CreateScriptFolders()
        {
            FileSystem.CreateDirectory(Path.Combine(Paths.ScriptsProgramPath, powerShellFolder));
            FileSystem.CreateDirectory(Path.Combine(Paths.ScriptsProgramPath, pythonFolder));
            if (!Settings.IsPortable)
            {
                FileSystem.CreateDirectory(Path.Combine(Paths.ScriptsUserDataPath, powerShellFolder));
                FileSystem.CreateDirectory(Path.Combine(Paths.ScriptsUserDataPath, pythonFolder));
            }
        }

        public static List<string> GetScriptFiles()
        {
            var scripts = new List<string>();
            var psScripts = Path.Combine(Paths.ScriptsProgramPath, powerShellFolder);
            if (Directory.Exists(psScripts))
            {
                foreach (var file in Directory.GetFiles(psScripts, "*.ps1", SearchOption.TopDirectoryOnly))
                {
                    scripts.Add(file);
                }
            }

            var pyScripts = Path.Combine(Paths.ScriptsProgramPath, pythonFolder);
            if (Directory.Exists(pyScripts))
            {
                foreach (var file in Directory.GetFiles(pyScripts, "*.py", SearchOption.TopDirectoryOnly))
                {
                    scripts.Add(file);
                }
            }

            if (!Settings.IsPortable)
            {
                psScripts = Path.Combine(Paths.ScriptsUserDataPath, powerShellFolder);
                if (Directory.Exists(psScripts))
                {
                    foreach (var file in Directory.GetFiles(psScripts, "*.ps1", SearchOption.TopDirectoryOnly))
                    {
                        scripts.Add(file);
                    }
                }

                pyScripts = Path.Combine(Paths.ScriptsUserDataPath, pythonFolder);
                if (Directory.Exists(pyScripts))
                {
                    foreach (var file in Directory.GetFiles(pyScripts, "*.py", SearchOption.TopDirectoryOnly))
                    {
                        scripts.Add(file);
                    }
                }
            }

            return scripts;
        }

        public static List<PlayniteScript> GetScripts()
        {
            var scripts = new List<PlayniteScript>();
            foreach (var path in GetScriptFiles())
            {
                scripts.Add(PlayniteScript.FromFile(path));
            }

            return scripts;
        }
    }
}

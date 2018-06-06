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

        public static IEnumerable<string> GetScriptFilesFromFolder(string path, string pattern)
        {
            foreach (var file in Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly))
            {
                yield return file;
            }
        }

        public static List<string> GetScriptFiles()
        {
            var scripts = new List<string>();
            var psScripts = Path.Combine(Paths.ScriptsProgramPath, powerShellFolder);
            if (Directory.Exists(psScripts))
            {
                foreach (var dir in Directory.GetDirectories(psScripts))
                {
                    scripts.AddRange(GetScriptFilesFromFolder(dir, "*.ps1"));
                }
            }

            var pyScripts = Path.Combine(Paths.ScriptsProgramPath, pythonFolder);
            if (Directory.Exists(pyScripts))
            {
                foreach (var dir in Directory.GetDirectories(pyScripts))
                {
                    scripts.AddRange(GetScriptFilesFromFolder(dir, "*.py"));
                }
            }

            if (!Settings.IsPortable)
            {
                psScripts = Path.Combine(Paths.ScriptsUserDataPath, powerShellFolder);
                if (Directory.Exists(psScripts))
                {
                    foreach (var dir in Directory.GetDirectories(psScripts))
                    {
                        scripts.AddRange(GetScriptFilesFromFolder(dir, "*.ps1"));
                    }
                }

                pyScripts = Path.Combine(Paths.ScriptsUserDataPath, pythonFolder);
                if (Directory.Exists(pyScripts))
                {
                    foreach (var dir in Directory.GetDirectories(pyScripts))
                    {
                        scripts.AddRange(GetScriptFilesFromFolder(dir, "*.py"));
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

        public static void InstallScript(string scriptPath)
        {
            var extension = Path.GetExtension(scriptPath);
            var scriptFolder = Settings.IsPortable ? Paths.ScriptsProgramPath : Paths.ScriptsUserDataPath;

            if (extension.Equals(".ps1", StringComparison.InvariantCultureIgnoreCase))
            {
                scriptFolder = Path.Combine(scriptFolder, powerShellFolder, Path.GetFileNameWithoutExtension(scriptPath));
            }
            else if (extension.Equals(".py", StringComparison.InvariantCultureIgnoreCase))
            {
                scriptFolder = Path.Combine(scriptFolder, pythonFolder, Path.GetFileNameWithoutExtension(scriptPath));
            }
            else
            {
                throw new Exception("Uknown script file specified.");
            }

            FileSystem.CreateDirectory(scriptFolder);
            File.Copy(scriptPath, Path.Combine(scriptFolder, Path.GetFileName(scriptPath)), true);
        }
    }
}

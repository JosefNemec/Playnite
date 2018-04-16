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
        public static List<string> GetScriptFiles()
        {
            var scripts = new List<string>();
            var psScripts = Path.Combine(Paths.ScriptsProgramPath, "PowerShell");
            if (Directory.Exists(psScripts))
            {
                foreach (var file in Directory.GetFiles(psScripts, "*.ps1", SearchOption.TopDirectoryOnly))
                {
                    scripts.Add(file);
                }
            }

            var pyScripts = Path.Combine(Paths.ScriptsProgramPath, "IronPython");
            if (Directory.Exists(pyScripts))
            {
                foreach (var file in Directory.GetFiles(pyScripts, "*.py", SearchOption.TopDirectoryOnly))
                {
                    scripts.Add(file);
                }
            }

            if (!Paths.AreEqual(Paths.ScriptsProgramPath, Paths.ScriptsUserDataPath))
            {
                psScripts = Path.Combine(Paths.ScriptsUserDataPath, "PowerShell");
                if (Directory.Exists(psScripts))
                {
                    foreach (var file in Directory.GetFiles(psScripts, "*.ps1", SearchOption.TopDirectoryOnly))
                    {
                        scripts.Add(file);
                    }
                }

                pyScripts = Path.Combine(Paths.ScriptsUserDataPath, "IronPython");
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

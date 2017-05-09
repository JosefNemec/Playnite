using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IWshRuntimeLibrary;

namespace Playnite
{
    public class Program
    {
        public string Path
        {
            get; set;
        }

        public string Icon
        {
            get; set;
        }

        public string WorkDir
        {
            get; set;
        }

        public string Name
        {
            get; set;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Programs
    {

        public static List<Program> GetExecutablesFromFolder(string path, SearchOption searchOption)
        {
            var execs = new List<Program>();
            var files = Directory.GetFiles(path, "*.exe", searchOption);

            foreach (var file in files)
            {
                execs.Add(new Program()
                {
                    Path = file,
                    Icon = file,
                    WorkDir = Path.GetDirectoryName(file),
                    Name = new DirectoryInfo(Path.GetDirectoryName(file)).Name
                });
            }

            return execs;
        }

        public static List<Program> GetShortcutProgramsFromFolder(string path)
        {
            var folderExceptions = new string[]
            {
                @"\Accessibility\",
                @"\Accessories\",
                @"\Administrative Tools\",
                @"\Maintenance\",
                @"\StartUp\",
                @"\Windows ",
                @"\Microsoft ",
            };

            var nameExceptions = new string[]
            {
                "uninstall",
                "setup"
            };

            var pathExceptions = new string[]
            {
                @"\system32\",
                @"\windows\",
            };

            var shell = new WshShell();
            var apps = new List<Program>();
            var shortucts = Directory.GetFiles(path, "*.lnk", SearchOption.AllDirectories);

            foreach (var shortcut in shortucts)
            {
                var fileName = Path.GetFileName(shortcut);
                var Directory = Path.GetDirectoryName(shortcut);

                if (folderExceptions.FirstOrDefault(a => shortcut.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0) != null)
                {
                    continue;
                }

                if (nameExceptions.FirstOrDefault(a => shortcut.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0) != null)
                {
                    continue;
                }

                var link = (IWshShortcut)shell.CreateShortcut(shortcut);               
                var target = link.TargetPath;
                
                if (pathExceptions.FirstOrDefault(a => target.IndexOf(a, StringComparison.OrdinalIgnoreCase) >= 0) != null)
                {
                    continue;
                }

                // Ignore duplicates
                if (apps.FirstOrDefault(a => a.Path == target) != null)
                {
                    continue;
                }

                // Ignore non-application links
                if (Path.GetExtension(target) != ".exe")
                {
                    continue;
                }

                var app = new Program()
                {
                    Path = target,
                    Icon = link.IconLocation,
                    Name = Path.GetFileNameWithoutExtension(shortcut),
                    WorkDir = link.WorkingDirectory
                };

                apps.Add(app);
            }

            return apps;
        }

        public static List<Program> GetInstalledPrograms()
        {
            var apps = new List<Program>();

            // Get apps from All Users
            var allPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs");
            apps.AddRange(GetShortcutProgramsFromFolder(allPath));

            // Get current user apps
            var userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");
            apps.AddRange(GetShortcutProgramsFromFolder(userPath));

            return apps;
        }
    }
}

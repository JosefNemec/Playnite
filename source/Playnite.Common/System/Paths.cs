using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Playnite.Common.System
{
    public class Paths
    {
        public static bool GetValidFilePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                return false;
            }

            string drive = Path.GetPathRoot(path);
            if (!string.IsNullOrEmpty(drive) && !Directory.Exists(drive))
            {
                return false;
            }

            return true;
        }

        public static string FixSeparators(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            var newPath = path.Replace('\\', Path.DirectorySeparatorChar);
            newPath = newPath.Replace('/', Path.DirectorySeparatorChar);
            return Regex.Replace(newPath, string.Format(@"\{0}+", Path.DirectorySeparatorChar), Path.DirectorySeparatorChar.ToString());
        }

        private static string Normalize(string path)
        {
            var formatted = path;
            try
            {
                formatted = new Uri(path).LocalPath;
            }
            catch
            {
                // this shound't happen 
            }

            return Path.GetFullPath(formatted).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).ToUpperInvariant();
        }

        public static bool AreEqual(string path1, string path2)
        {
            if (string.IsNullOrEmpty(path1) && !string.IsNullOrEmpty(path2))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(path1) && string.IsNullOrEmpty(path2))
            {
                return false;
            }

            // Empty string is not valid path, return false even when both are null
            if (string.IsNullOrEmpty(path1) && string.IsNullOrEmpty(path2))
            {
                return false;
            }

            try
            {
                return Normalize(path1) == Normalize(path2);
            }
            catch
            {
                return false;
            }
        }

        public static string GetSafeFilename(string filename)
        {
            return string.Join(" ", filename.Split(Path.GetInvalidFileNameChars()));
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Playnite.Native;

namespace Playnite.Common
{
    public class Paths
    {
        public static string GetFinalPathName(string path)
        {
            var h = Kernel32.CreateFile(path,
                Winnt.FILE_READ_EA,
                FileShare.ReadWrite | FileShare.Delete,
                IntPtr.Zero,
                FileMode.Open,
                Fileapi.FILE_FLAG_BACKUP_SEMANTICS,
                IntPtr.Zero);

            if (path.StartsWith(@"\\"))
            {
                return path;
            }

            if (h == Winuser.INVALID_HANDLE_VALUE)
            {
                throw new Win32Exception();
            }

            try
            {
                var sb = new StringBuilder(1024);
                var res = Kernel32.GetFinalPathNameByHandle(h, sb, 1024, 0);
                if (res == 0)
                {
                    throw new Win32Exception();
                }

                var targetPath = sb.ToString();
                if (targetPath.StartsWith(@"\\?\UNC\"))
                {
                    return targetPath.Replace(@"\\?\UNC\", @"\\");
                }
                else
                {
                    return targetPath.Replace(@"\\?\", string.Empty);
                }
            }
            finally
            {
                Kernel32.CloseHandle(h);
            }
        }

        public static bool IsValidFilePath(string path)
        {
            try
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
            catch
            {
                // Any of Path methods can throw exception in case that path is some weird string
                return false;
            }
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

        public static string GetSafePathName(string filename)
        {
            var path = string.Join(" ", filename.Split(Path.GetInvalidFileNameChars()));
            return Regex.Replace(path, @"\s+", " ").Trim();
        }

        public static bool IsFullPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return false;
            }

            // Don't use Path.IsPathRooted because it fails on paths starting with one backslash.
            return Regex.IsMatch(path, @"^([a-zA-Z]:\\|\\\\)");
        }

        public static string GetCommonDirectory(string[] paths)
        {
            int k = paths[0].Length;
            for (int i = 1; i < paths.Length; i++)
            {
                k = Math.Min(k, paths[i].Length);
                for (int j = 0; j < k; j++)
                {
                    if (paths[i][j] != paths[0][j])
                    {
                        k = j;
                        break;
                    }
                }
            }

            var common = paths[0].Substring(0, k);
            if (common.Length == 0)
            {
                return string.Empty;
            }

            if (common[common.Length -1] == Path.DirectorySeparatorChar)
            {
                return common;
            }
            else
            {
                return common.Substring(0, common.LastIndexOf(Path.DirectorySeparatorChar) + 1);
            }
        }

        public static bool MathcesFilePattern(string filePath, string pattern)
        {
            if (filePath.IsNullOrEmpty() || pattern.IsNullOrEmpty())
            {
                return false;
            }

            if (pattern.Contains(';'))
            {
                return Shlwapi.PathMatchSpecExW(filePath, pattern, MatchPatternFlags.Multiple) == 0;
            }
            else
            {
                return Shlwapi.PathMatchSpecExW(filePath, pattern, MatchPatternFlags.Normal) == 0;
            }
        }
    }
}

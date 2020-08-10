using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO.Compression;
using Playnite.SDK;

namespace Playnite.Common
{
    public static class FileSystem
    {
        private static ILogger logger = LogManager.GetLogger();

        public static void CreateDirectory(string path)
        {
            CreateDirectory(path, false);
        }

        public static void CreateDirectory(string path, bool clean)
        {
            var directory = path;
            if (string.IsNullOrEmpty(directory))
            {
                return;
            }

            if (Directory.Exists(directory))
            {
                if (clean)
                {
                    Directory.Delete(directory, true);
                }
                else
                {
                    return;
                }
            }

            Directory.CreateDirectory(directory);
        }

        public static void PrepareSaveFile(string path)
        {
            CreateDirectory(Path.GetDirectoryName(path));
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static bool IsDirectoryEmpty(string path)
        {
            if (Directory.Exists(path))
            {
                return !Directory.EnumerateFileSystemEntries(path).Any();
            }
            else
            {
                return true;
            }
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static void CopyFile(string sourcePath, string targetPath, bool overwrite = true)
        {
            logger.Debug($"Copying file {sourcePath} to {targetPath}");
            PrepareSaveFile(targetPath);
            File.Copy(sourcePath, targetPath, overwrite);
        }

        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public static string GetMD5(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");
            }
        }

        public static string GetMD5(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return GetMD5(stream);
            }
        }

        public static void AddFolderToZip(ZipArchive archive, string zipRoot, string path, string filter, SearchOption searchOption)
        {
            IEnumerable<string> files;

            if (filter.Contains('|'))
            {
                var filters = filter.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                files = Directory.EnumerateFiles(path, "*.*", searchOption).Where(a =>
                {
                    return filters.Contains(Path.GetExtension(a));
                });
            }
            else
            {
                files = Directory.EnumerateFiles(path, filter, searchOption);
            }

            foreach (var file in files)
            {
                var archiveName = zipRoot + file.Replace(path, "").Replace(@"\", @"/");
                archive.CreateEntryFromFile(file, archiveName);
            }
        }

        public static bool CanWriteToFolder(string folder)
        {
            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                using (var stream = File.Create(Path.Combine(folder, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose))
                {
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ReadFileAsStringSafe(string path, int retryAttempts = 5)
        {
            IOException ioException = null;
            for (int i = 0; i < retryAttempts; i++)
            {
                try
                {
                    return File.ReadAllText(path);
                }
                catch (IOException exc)
                {
                    logger.Debug($"Can't read from file, trying again. {path}");
                    ioException = exc;
                    Task.Delay(500).Wait();
                }
            }

            throw new IOException($"Failed to read {path}", ioException);
        }

        public static byte[] ReadFileAsBytesSafe(string path, int retryAttempts = 5)
        {
            IOException ioException = null;
            for (int i = 0; i < retryAttempts; i++)
            {
                try
                {
                    return File.ReadAllBytes(path);
                }
                catch (IOException exc)
                {
                    logger.Debug($"Can't read from file, trying again. {path}");
                    ioException = exc;
                    Task.Delay(500).Wait();
                }
            }

            throw new IOException($"Failed to read {path}", ioException);
        }

        public static Stream OpenReadFileStreamSafe(string path, int retryAttempts = 5)
        {
            IOException ioException = null;
            for (int i = 0; i < retryAttempts; i++)
            {
                try
                {
                    return new FileStream(path, FileMode.Open, FileAccess.Read);
                }
                catch (IOException exc)
                {
                    logger.Debug($"Can't open file stream, trying again. {path}");
                    ioException = exc;
                    Task.Delay(500).Wait();
                }
            }

            throw new IOException($"Failed to read {path}", ioException);
        }

        public static void WriteStringToFile(string path, string content)
        {
            PrepareSaveFile(path);
            File.WriteAllText(path, content);
        }

        public static void WriteStringToFileSafe(string path, string content, int retryAttempts = 5)
        {
            IOException ioException = null;
            for (int i = 0; i < retryAttempts; i++)
            {
                try
                {
                    PrepareSaveFile(path);
                    File.WriteAllText(path, content);
                    return;
                }
                catch (IOException exc)
                {
                    logger.Debug($"Can't write to a file, trying again. {path}");
                    ioException = exc;
                    Task.Delay(500).Wait();
                }
            }

            throw new IOException($"Failed to write to {path}", ioException);
        }

        public static void DeleteFileSafe(string path, int retryAttempts = 5)
        {
            if (!File.Exists(path))
            {
                return;
            }

            IOException ioException = null;
            for (int i = 0; i < retryAttempts; i++)
            {
                try
                {
                    File.Delete(path);
                    return;
                }
                catch (IOException exc)
                {
                    logger.Debug($"Can't detele file, trying again. {path}");
                    ioException = exc;
                    Task.Delay(500).Wait();
                }
                catch (UnauthorizedAccessException exc)
                {
                    logger.Error(exc, $"Can't detele file, UnauthorizedAccessException. {path}");
                    return;
                }
            }

            throw new IOException($"Failed to delete {path}", ioException);
        }

        public static long GetFreeSpace(string drivePath)
        {
            var root = Path.GetPathRoot(drivePath);
            var drive = DriveInfo.GetDrives().FirstOrDefault(a => a.RootDirectory.FullName.Equals(root, StringComparison.OrdinalIgnoreCase)); ;
            if (drive != null)
            {
                return drive.AvailableFreeSpace;
            }
            else
            {
                return 0;
            }
        }

        public static long GetFileSize(string path)
        {
            return new FileInfo(path).Length;
        }

        public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs = true, bool overwrite = true)
        {
            var dir = new DirectoryInfo(sourceDirName);
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            var dirs = dir.GetDirectories();
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            var files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, overwrite);
            }

            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}

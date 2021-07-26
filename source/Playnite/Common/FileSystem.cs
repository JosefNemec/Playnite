using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Playnite.SDK;
using System.Diagnostics;

namespace Playnite.Common
{
    public enum FileSystemItem
    {
        File,
        Directory
    }

    public static partial class FileSystem
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

        public static void DeleteDirectory(string path, bool includeReadonly)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            if (includeReadonly)
            {
                foreach (var s in Directory.GetDirectories(path))
                {
                    DeleteDirectory(s, true);
                }

                foreach (var f in Directory.GetFiles(path))
                {
                    var attr = File.GetAttributes(f);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(f, attr ^ FileAttributes.ReadOnly);
                    }

                    File.Delete(f);
                }

                Directory.Delete(path, false);
            }
            else
            {
                DeleteDirectory(path);
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

        public static Stream CreateWriteFileStreamSafe(string path, int retryAttempts = 5)
        {
            IOException ioException = null;
            for (int i = 0; i < retryAttempts; i++)
            {
                try
                {
                    return new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                }
                catch (IOException exc)
                {
                    logger.Debug($"Can't open write file stream, trying again. {path}");
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
                    logger.Debug($"Can't open read file stream, trying again. {path}");
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

        public static bool FileExistsOnAnyDrive(string filePath, out string existringPath)
        {
            return PathExistsOnAnyDrive(filePath, path => File.Exists(path), out existringPath);
        }

        public static bool DirectoryExistsOnAnyDrive(string directoryPath, out string existringPath)
        {
            return PathExistsOnAnyDrive(directoryPath, path => Directory.Exists(path), out existringPath);
        }

        private static bool PathExistsOnAnyDrive(string originalPath, Predicate<string> predicate, out string existringPath)
        {
            existringPath = null;
            try
            {
                if (predicate(originalPath))
                {
                    existringPath = originalPath;
                    return true;
                }

                if (!Paths.IsFullPath(originalPath))
                {
                    return false;
                }

                var rootPath = Path.GetPathRoot(originalPath);
                var availableDrives = DriveInfo.GetDrives().Where(d => d.IsReady);
                foreach (var drive in availableDrives)
                {
                    var pathWithoutDrive = originalPath.Substring(drive.Name.Length);
                    var newPath = Path.Combine(drive.Name, pathWithoutDrive);
                    if (predicate(newPath))
                    {
                        existringPath = newPath;
                        return true;
                    }
                }
            }
            catch (Exception ex) when (!Debugger.IsAttached)
            {
                logger.Error(ex, $"Error checking if path exists on different drive \"{originalPath}\"");
            }

            return false;
        }
    }
}

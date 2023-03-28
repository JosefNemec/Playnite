using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;
using Vanara.PInvoke;

namespace Playnite
{
    public enum FileSystemItem
    {
        File,
        Directory
    }

    public static partial class FileSystem
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public static void CreateDirectory(string path)
        {
            CreateDirectory(path, false);
        }

        public static void CreateDirectory(string path, bool clean)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (Directory.Exists(path))
            {
                if (clean)
                {
                    DeleteDirectory(path, true);
                }
                else
                {
                    return;
                }
            }

            Directory.CreateDirectory(path);
        }

        public static void PrepareSaveFile(string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (!dir.IsNullOrEmpty())
            {
                CreateDirectory(dir);
            }

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

        public static void CreateFile(string path)
        {
            FileSystem.PrepareSaveFile(path);
            File.Create(path).Dispose();
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

                foreach (var file in Directory.GetFiles(path))
                {
                    var attr = File.GetAttributes(file);
                    if ((attr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(file, attr ^ FileAttributes.ReadOnly);
                    }

                    File.Delete(file);
                }

                var dirAttr = File.GetAttributes(path);
                if ((dirAttr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                {
                    File.SetAttributes(path, dirAttr ^ FileAttributes.ReadOnly);
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

                using var stream = File.Create(Path.Combine(folder, Path.GetRandomFileName()), 1, FileOptions.DeleteOnClose);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ReadFileAsStringSafe(string path, int retryAttempts = 5)
        {
            IOException? ioException = null;
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
            IOException? ioException = null;
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
            IOException? ioException = null;
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
            IOException? ioException = null;
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

        public static string ReadStringFromFile(string path)
        {
            return File.ReadAllText(path);
        }

        public static void WriteStringToFileSafe(string path, string content, int retryAttempts = 5)
        {
            IOException? ioException = null;
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

            IOException? ioException = null;
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
            return GetFileSize(new FileInfo(path));
        }

        public static long GetFileSize(FileInfo fi)
        {
            return fi.Length;
        }

        public static long GetDirectorySize(string path, bool getSizeOnDisk)
        {
            return GetDirectorySize(new DirectoryInfo(path), getSizeOnDisk);
        }

        private static long GetDirectorySize(DirectoryInfo dirInfo, bool getSizeOnDisk)
        {
            long size = 0;
            try
            {
                foreach (FileInfo fileInfo in dirInfo.GetFiles())
                {
                    size += getSizeOnDisk ? GetFileSizeOnDisk(fileInfo) : GetFileSize(fileInfo);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // Directory not being found here means that directory is a symlink
                // with an invalid target path.
                // TODO Rework with proper symlinks handling with FileSystemInfo.ResolveLinkTarget
                // method after Net runtime upgrade
                return size;
            }

            foreach (DirectoryInfo subdirInfo in dirInfo.GetDirectories())
            {
                if (!IsDirectorySubdirSafeToRecurse(subdirInfo))
                {
                    continue;
                }

                size += GetDirectorySize(subdirInfo.FullName, getSizeOnDisk);
            }

            return size;
        }

        public static long GetFileSizeOnDisk(string path)
        {
            return GetFileSizeOnDisk(new FileInfo(path));
        }

        public static long GetFileSizeOnDisk(FileInfo fileInfo)
        {
            // Method will fail if file is a symlink that has a target
            // that does not exist. To avoid, we can check its lenght before continuing
            if (fileInfo.Length == 0)
            {
                return 0;
            }

            // Method will fail when checking a file that's not valid on Windows,
            // for example files used by Proton containing a colon (:).
            // 'Directory' will be null when encountering such a file.
            if (fileInfo.Directory is null)
            {
                return 0;
            }

            // From https://stackoverflow.com/a/3751135
            if (!Kernel32.GetDiskFreeSpace(fileInfo.Directory!.Root.FullName, out var sectorsPerCluster, out var bytesPerSector, out var _, out var _))
            {
                Win32Error.GetLastError().ThrowIfFailed();
            }

            uint clusterSize = sectorsPerCluster * bytesPerSector;
            uint losize = Kernel32.GetCompressedFileSize(Paths.FormatLongPath(fileInfo.FullName), out uint hosize);
            int error = Marshal.GetLastWin32Error();
            if (losize == 0xFFFFFFFF && error != 0)
            {
                throw new System.ComponentModel.Win32Exception(error);
            }

            var size = (long)hosize << 32 | losize;
            return ((size + clusterSize - 1) / clusterSize) * clusterSize;
        }

        private static bool IsDirectorySubdirSafeToRecurse(DirectoryInfo childDirectory)
        {
            // Whitespace characters can cause confusion in methods, causing them to process
            // the parent directory instead and causing an infinite loop
            if (childDirectory.Name.IsNullOrWhiteSpace())
            {
                return false;
            }

            return true;
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

        public static bool FileExistsOnAnyDrive(string filePath, [NotNullWhen(true)] out string? existringPath)
        {
            return PathExistsOnAnyDrive(filePath, path => File.Exists(path), out existringPath);
        }

        public static bool DirectoryExistsOnAnyDrive(string directoryPath, [NotNullWhen(true)] out string? existringPath)
        {
            return PathExistsOnAnyDrive(directoryPath, path => Directory.Exists(path), out existringPath);
        }

        private static bool PathExistsOnAnyDrive(string originalPath, Predicate<string> predicate, [NotNullWhen(true)] out string? existringPath)
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

        public static bool DirectoryExists(string path)
        {
            return Directory.Exists(path);
        }

        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static DateTime DirectoryGetLastWriteTime(string path)
        {
            return Directory.GetLastWriteTime(path);
        }

        public static DateTime FileGetLastWriteTime(string path)
        {
            return File.GetLastWriteTime(path);
        }

        public static void ReplaceStringInFile(string path, string oldValue, string newValue, Encoding? encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var fileContent = File.ReadAllText(path, encoding);
            if (fileContent.IsNullOrEmpty())
            {
                return;
            }

            File.WriteAllText(path, fileContent.Replace(oldValue, newValue, StringComparison.Ordinal), encoding);
        }
    }
}

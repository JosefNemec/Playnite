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

namespace Playnite
{
    public static class FileSystem
    {
        public static void CreateDirectory(string path)
        {
            CreateDirectory(path, false);
        }

        public static void CreateDirectory(string path, bool clean)
        {
            var directory = path;
            if (!string.IsNullOrEmpty(Path.GetExtension(path)))
            {
                directory = Path.GetDirectoryName(path);
            }

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

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public static void DeleteDirectory(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public static string GetSafeFilename(string filename)
        {
            return string.Join(" ", filename.Split(Path.GetInvalidFileNameChars()));
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

        public static string FileReadAsString(string path)
        {
            // TODO: Add retry to file lock
            return File.ReadAllText(path);
        }

        public static void FileWriteString(string path, string content)
        {
            PrepareSaveFile(path);
            // TODO: Add retry to file lock
            File.WriteAllText(path, content);
        }

    }
}

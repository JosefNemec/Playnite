using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NLog;
using System.Security.Cryptography;
using System.IO.Compression;

namespace Playnite
{
    public static class FileSystem
    {
        public static void CreateFolder(string path)
        {
            CreateFolder(path, false);
        }

        public static void CreateFolder(string path, bool clean)
        {
            if (Directory.Exists(path))
            {
                if (clean)
                {
                    Directory.Delete(path, true);
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
            CreateFolder(Path.GetDirectoryName(path));
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

        public static void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }

        public static string GetSafeFilename(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        public static string GetMD5(Stream stream)
        {
            var md5 = MD5.Create();
            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");
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
    }
}

using System.IO;
using System.Security.Cryptography;

namespace Playnite;

public static partial class FileSystem
{
    public static string GetCRC32(Stream stream)
    {
        uint crc = 0;
        var buffer = new byte[4096];
        int bytesRead;
        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        {
            crc = Force.Crc32.Crc32Algorithm.Append(crc, buffer, 0, bytesRead);
        }

        return string.Format("{0:X8}", crc);
    }

    public static string GetCRC32(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return GetCRC32(stream);
    }

    public static string GetMD5(Stream stream)
    {
        using var md5 = MD5.Create();
        return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "", StringComparison.Ordinal);
    }

    public static string GetMD5(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return GetMD5(stream);
    }

    public static bool AreFileContentsEqual(string path1, string path2)
    {
        var info1 = new FileInfo(path1);
        var info2 = new FileInfo(path2);
        if (info1.Length != info2.Length)
        {
            return false;
        }
        else
        {
            return GetMD5(path1) == GetMD5(path2);
        }
    }
}

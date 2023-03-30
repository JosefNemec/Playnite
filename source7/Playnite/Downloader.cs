using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite;

public static class Downloader
{
    private static readonly ILogger logger = LogManager.GetLogger();
    private static readonly HttpClient httpClient = new HttpClient();

    static Downloader()
    {
        httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Playnite", "11"));
    }

    public static async Task<byte[]> DownloadBytesAsync(string url)
    {
        logger.Debug($"Downloading byte data from {url}.");
        var resp = await httpClient.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsByteArrayAsync();
    }

    public static async Task<byte[]> DownloadBytesAsync(string url, CancellationToken cancelToken)
    {
        logger.Debug($"Downloading byte data from {url}.");
        var resp = await httpClient.GetAsync(url, cancelToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsByteArrayAsync(cancelToken);
    }

    public static async Task<string> DownloadStringAsync(string url)
    {
        logger.Debug($"Downloading string from {url}.");
        var resp = await httpClient.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync();
    }

    public static async Task<string> DownloadStringAsync(string url, CancellationToken cancelToken)
    {
        logger.Debug($"Downloading string from {url}.");
        var resp = await httpClient.GetAsync(url, cancelToken);
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadAsStringAsync(cancelToken);
    }

    public static async Task<string> DownloadStringAsync(params string[] mirrors)
    {
        logger.Debug($"Downloading string content from multiple mirrors.");
        foreach (var mirror in mirrors)
        {
            try
            {
                return await DownloadStringAsync(mirror);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to download {mirror} string.");
            }
        }

        throw new Exception("Failed to download string from all mirrors.");
    }

    public static async Task<string> DownloadStringAsync(CancellationToken cancelToken, params string[] mirrors)
    {
        logger.Debug($"Downloading string content from multiple mirrors.");
        foreach (var mirror in mirrors)
        {
            if (cancelToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                return await DownloadStringAsync(mirror, cancelToken);
            }
            catch (Exception e)
            {
                logger.Error(e, $"Failed to download {mirror} string.");
            }
        }

        throw new Exception("Failed to download string from all mirrors.");
    }

    public static async Task DownloadFileAsync(string url, string path)
    {
        logger.Debug($"Downloading file from {url} to {path}.");
        FileSystem.PrepareSaveFile(path);
        var resp = await httpClient.GetAsync(url);
        resp.EnsureSuccessStatusCode();
        using var fs = new FileStream(path, FileMode.Create);
        await resp.Content.CopyToAsync(fs);
    }

    public static async Task DownloadFileAsync(string url, string path, CancellationToken cancelToken)
    {
        logger.Debug($"Downloading file from {url} to {path}.");
        FileSystem.PrepareSaveFile(path);
        var resp = await httpClient.GetAsync(url, cancelToken);
        resp.EnsureSuccessStatusCode();
        using var fs = new FileStream(path, FileMode.Create);
        await resp.Content.CopyToAsync(fs, cancelToken);
    }

    public static async Task DownloadFileAsync(string url, string path, Action<DownloadProgress> progressHandler, CancellationToken cancelToken)
    {
        logger.Debug($"Downloading file from {url} to {path}.");
        FileSystem.PrepareSaveFile(path);
        var resp = await httpClient.GetAsync(url, cancelToken);
        resp.EnsureSuccessStatusCode();

        var totalBytes = resp.Content.Headers.ContentLength;
        using var fs = new FileStream(path, FileMode.Create);
        if (totalBytes is null)
        {
            await resp.Content.CopyToAsync(fs, cancelToken);
        }
        else
        {
            var progress = new DownloadProgress(totalBytes.Value);
            var lastPercentage = 0;
            var buffer = new byte[8192];
            using var download = await resp.Content.ReadAsStreamAsync(cancelToken);
            while (true)
            {
                var currentRead = await download.ReadAsync(buffer.AsMemory(0, 8192), cancelToken);
                if (currentRead == 0 || cancelToken.IsCancellationRequested)
                {
                    break;
                }

                await fs.WriteAsync(buffer.AsMemory(0, currentRead), cancelToken);
                progress.Update(progress.ReadBytes + currentRead);
                if (lastPercentage != progress.PercentageDone)
                {
                    progressHandler(progress);
                }

                lastPercentage = progress.PercentageDone;
            }
        }
    }

    public static async Task<HttpStatusCode> GetResponseCode(string url)
    {
        try
        {
            var response = await httpClient.GetAsync(url);
            return response.StatusCode;
        }
        catch (Exception e)
        {
            logger.Error(e, $"Failed to get HTTP response for {url}.");
            return HttpStatusCode.ServiceUnavailable;
        }
    }

    public class DownloadProgress
    {
        public long TotalBytes { get; }
        public long ReadBytes { get; private set; }
        public int PercentageDone { get; private set; }

        public DownloadProgress(long totalBytes)
        {
            TotalBytes = totalBytes;
        }

        internal void Update(long readBytesSum)
        {
            ReadBytes = readBytesSum;
            PercentageDone = Convert.ToInt32(((double)ReadBytes / TotalBytes) * 100);
        }
    }
}

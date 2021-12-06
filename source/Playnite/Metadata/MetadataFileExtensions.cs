using Playnite.Common;
using Playnite.Common.Web;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite
{
    public static class MetadataFileExtensions
    {
        /// <summary>
        /// Returns local file path of file. If link, downloads file to Playnite temp. If raw content, saves file to Playnite temp.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="cancelToken"></param>
        /// <returns></returns>
        public static string GetLocalFile(this MetadataFile file, CancellationToken cancelToken)
        {
            if (!file.HasImageData)
            {
                return null;
            }

            if (file.HasContent)
            {
                var resultPath = Path.Combine(PlaynitePaths.TempPath, Guid.NewGuid() + Path.GetExtension(file.FileName));
                FileSystem.PrepareSaveFile(resultPath);
                File.WriteAllBytes(resultPath, file.Content);
                return resultPath;
            }
            else
            {
                if (file.Path.IsHttpUrl())
                {
                    var extension = Path.GetExtension(new Uri(file.Path).AbsolutePath);
                    var resultPath = Path.Combine(PlaynitePaths.TempPath, Guid.NewGuid() + extension);
                    FileSystem.PrepareSaveFile(resultPath);
                    HttpDownloader.DownloadFile(file.Path, resultPath, cancelToken);
                    if (cancelToken.IsCancellationRequested)
                    {
                        if (File.Exists(resultPath))
                        {
                            File.Delete(resultPath);
                        }

                        return null;
                    }
                    else
                    {
                        return resultPath;
                    }
                }
                else
                {
                    return file.Path;
                }
            }
        }
    }
}

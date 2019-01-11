using ItchioLibrary.Models;
using ItchioLibrary.Models.Butler;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ItchioLibrary
{
    public class Butler : IDisposable
    {
        public class Methods
        {
            /// <summary>
            /// When using TCP transport, must be the first message sent
            /// </summary>
            public const string Meta_Authenticate = "Meta.Authenticate";

            /// <summary>
            /// When called, gracefully shutdown the butler daemon.
            /// </summary>
            public const string Meta_Shutdown = "Meta.Shutdown";            

            /// <summary>
            /// Retrieve info for all caves.
            /// </summary>
            public const string Fetch_Caves = "Fetch.Caves";

            /// <summary>
            /// Lists remembered profiles
            /// </summary>            
            public const string Profile_List = "Profile.List";

            /// <summary>
            /// List owned keys
            /// </summary>
            public const string Fetch_ProfileOwnedKeys = "Fetch.ProfileOwnedKeys";            
        }

        public static string ExecutablePath
        {
            get
            {
                var userPath = Itch.UserPath;
                if (!Directory.Exists(userPath))
                {
                    return string.Empty;
                }

                var corePath = Path.Combine(userPath, "broth", "butler");
                var versionPath = Path.Combine(corePath, ".chosen-version");
                if (!File.Exists(versionPath))
                {
                    return string.Empty;
                }

                var currentVer = File.ReadAllText(versionPath);
                var exePath = Path.Combine(corePath, "versions", currentVer, "butler.exe");
                return File.Exists(exePath) ? exePath : string.Empty;
            }
        }

        public static string DatabasePath
        {
            get
            {
                var userPath = Itch.UserPath;
                if (!Directory.Exists(userPath))
                {
                    return string.Empty;
                }

                var dbPath = Path.Combine(userPath, "db", "butler.db");
                return File.Exists(dbPath) ? dbPath : string.Empty;
            }
        }

        private static ILogger logger = LogManager.GetLogger();
        private const string startupArgs = @"daemon --json --dbpath ""{0}"" --transport tcp --keep-alive";

        private Process proc;
        private string endpoint;
        private string secret;
        private AutoResetEvent startupEvent = new AutoResetEvent(false);
        private JsonRpcClient client;

        public Butler()
        {
            if (string.IsNullOrEmpty(ExecutablePath))
            {
                throw new FileNotFoundException("Butler executable not found.");
            }

            if (string.IsNullOrEmpty(DatabasePath))
            {
                throw new FileNotFoundException("Butler database not found.");
            }

            var butlerArgs = string.Format(startupArgs, DatabasePath);
            proc = new Process 
            {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo
                {
                    Arguments = butlerArgs,
                    FileName = ExecutablePath,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            proc.OutputDataReceived += Proc_OutputDataReceived;
            proc.ErrorDataReceived += Proc_ErrorDataReceived;
            proc.Start();
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
            if (!startupEvent.WaitOne(10000))
            {
                proc.Kill();
                throw new Exception("Butler failed to start properly.");
            }

            client = new JsonRpcClient(endpoint);
            client.SendRequest<Dictionary<string, object>>(Methods.Meta_Authenticate, new Dictionary<string, object>()
            {
                { "secret", secret }
            });
        }

        private void Proc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            LogButtlerOutput(e.Data);
        }

        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
            {
                return;
            }

            LogButtlerOutput(e.Data);
            if (e.Data.StartsWith("{") && Serialization.TryFromJson<ButlerOutput>(e.Data, out var data))
            {
                if (data.type == ListenNotification.MessageType)
                {
                    var listenData = Serialization.FromJson<ListenNotification>(e.Data);
                    endpoint = listenData.tcp.address;
                    secret = listenData.secret;
                    startupEvent.Set();
                }
            }
        }

        public void Dispose()
        {
            if (!proc.HasExited)
            {
                Shutdown();
                client.Dispose();
                proc.WaitForExit(5000);
                if (proc.HasExited == false)
                {
                    logger.Error("Butler didn't shutdown gracefully, going kill it.");
                    proc.Kill();
                }
            }
            else
            {
                client.Dispose();
            }
        }

        private void LogButtlerOutput(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return;
            }

            if (data.StartsWith("{") && Serialization.TryFromJson<ButlerOutput>(data, out var serialized))
            {
                if (serialized.type == Log.MessageType)
                {
                    var message = Serialization.FromJson<Log>(data);
                    if (message.level == "error")
                    {
                        logger.Error(message.message);
                    }
                    else
                    {
                        logger.Info(message.message);
                    }
                }
                else
                {
                    logger.Debug(data);
                }
            }
            else
            {
                logger.Debug("Butler: " + data);
            }
        }

        public List<Cave> GetCaves()
        {
            return client.SendRequest<FetchCaves>(Methods.Fetch_Caves).items;
        }

        public List<Profile> GetProfiles()
        {
            return client.SendRequest<ProfileList>(Methods.Profile_List).profiles;                
        }

        public void Shutdown()
        {
            client.SendRequest(Methods.Meta_Shutdown);
        }

        public List<DownloadKey> GetOwnedKeys(long profileId)
        {
            return client.SendRequest<FetchProfileOwnedKeys>(Methods.Fetch_ProfileOwnedKeys, new Dictionary<string, object>()
            {
                { "profileId", profileId },
                { "fresh", true }
            }).items;
        }
    }
}

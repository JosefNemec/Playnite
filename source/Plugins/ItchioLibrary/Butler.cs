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
            /// Fetches information for an itch.io game.
            /// </summary>
            public const string Fetch_Game = "Fetch.Game";

            /// <summary>
            /// Lists remembered profiles
            /// </summary>            
            public const string Profile_List = "Profile.List";

            /// <summary>
            /// List owned keys
            /// </summary>
            public const string Fetch_ProfileOwnedKeys = "Fetch.ProfileOwnedKeys";

            // <summary>
            /// Sent any time butler needs to send a log message
            /// </summary>
            public const string Log = "Log";

            // <summary>
            /// Attempt to launch an installed game.
            /// </summary>
            public const string Launch = "Launch";

            // <summary>
            /// Sent during Launch, when the game is configured, prerequisites are installed sandbox is set up (if enabled), and the game is actually running.
            /// </summary>
            public const string LaunchRunning = "LaunchRunning";

            // <summary>
            /// Sent during Launch, when the game has actually exited.
            /// </summary>
            public const string LaunchExited = "LaunchExited";

            // <summary>
            /// Sent during Launch, ask the user to pick a manifest action to launch.
            /// </summary>
            public const string PickManifestAction = "PickManifestAction";

            // <summary>
            /// Ask the client to perform a shell launch, ie. open an item with the operating system’s default handler (File explorer).
            /// </summary>
            public const string ShellLaunch = "ShellLaunch";

            // <summary>
            /// Ask the client to perform an URL launch, ie. open an address with the system browser or appropriate.
            /// </summary>
            public const string URLLaunch = "URLLaunch";

            // <summary>
            /// Ask the client to perform an HTML launch, ie. open an HTML5 game, ideally in an embedded browser.
            /// </summary>
            public const string HTMLLaunch = "HTMLLaunch";

            // <summary>
            /// Sent during Launch, when one or more prerequisites have failed to install.
            /// </summary>
            public const string PrereqsFailed = "PrereqsFailed";
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

        public event EventHandler<JsonRpcRequestEventArgs> RequestReceived;
        public event EventHandler<JsonRpcNotificationEventArgs> NotificationReceived;

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
            client.NotificationReceived += Client_NotificationReceived;
            client.RequestReceived += Client_RequestReceived;
            client.SendRequest<Dictionary<string, object>>(Methods.Meta_Authenticate, new Dictionary<string, object>()
            {
                { "secret", secret }
            });
        }

        private void Client_RequestReceived(object sender, JsonRpcRequestEventArgs e)
        {
            RequestReceived?.Invoke(this, e);
        }

        private void Client_NotificationReceived(object sender, JsonRpcNotificationEventArgs e)
        {
            if (e.Notification.Method == Methods.Log)
            {
                var logMessage = e.Notification.GetParams<LogMessage>();
                switch (logMessage.level)
                {
                    case LogLevel.debug:
                        logger.Debug("Butler: " + logMessage.message);
                        break;
                    case LogLevel.info:
                        logger.Info("Butler: " + logMessage.message);
                        break;
                    case LogLevel.warning:
                        logger.Warn("Butler: " + logMessage.message);
                        break;
                    case LogLevel.error:
                        logger.Error("Butler: " + logMessage.message);
                        break;
                }
            }

            NotificationReceived?.Invoke(this, e);
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
                    logger.Error("Butler didn't shutdown gracefully, going to kill it.");
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
            var allCaves = new List<Cave>();
            FetchCaves caveResult = null;

            do
            {
                var prms = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(caveResult?.nextCursor))
                {
                    prms.Add("cursor", caveResult.nextCursor);
                }

                caveResult = client.SendRequest<FetchCaves>(Methods.Fetch_Caves, prms);
                if (caveResult.items != null)
                {
                    allCaves.AddRange(caveResult.items);
                }
            }
            while (!string.IsNullOrEmpty(caveResult?.nextCursor));
            return allCaves;
        }

        public List<Profile> GetProfiles()
        {
            return client.SendRequest<ProfileList>(Methods.Profile_List).profiles;                
        }

        public ItchioGame GetGame(int gameId)
        {
            return client.SendRequest<FetchGame>(Methods.Fetch_Game, new Dictionary<string, object>
            {
                { "gameId", gameId },
                { "fresh", true }
            }).game;
        }

        public void Shutdown()
        {
            client.SendRequest(Methods.Meta_Shutdown);
        }

        public List<DownloadKey> GetOwnedKeys(long profileId)
        {
            return client.SendRequest<FetchProfileOwnedKeys>(Methods.Fetch_ProfileOwnedKeys, new Dictionary<string, object>
            {
                { "profileId", profileId },
                { "fresh", true }
            }).items;
        }

        public Task LaunchAsync(string caveId)
        {
            return Task.Run(() =>
                client.SendRequest(Methods.Launch, new Dictionary<string, object>
                {
                    { "caveId", caveId },
                    { "prereqsDir", Itch.PrereqsPaths }
                })
            );
        }

        public void SendResponse(JsonRpcRequest request, object response)
        {
            client.SendResponse(request, response);
        }

        public void SendResponse(JsonRpcRequest request)
        {
            client.SendResponse(request, new Dictionary<string, object>());
        }
    }
}

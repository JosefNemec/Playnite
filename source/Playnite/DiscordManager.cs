using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite
{
    public class DiscordManager : IDisposable
    {
        private static readonly ILogger logger = LogManager.GetLogger();
        private DiscordRPC.DiscordRpcClient discord;
        private bool isConnected = false;
        private bool isConnecting = false;

        public bool IsPresenceEnabled { get; set; } = false;

        public DiscordManager(bool presenceEnabled = false)
        {
            IsPresenceEnabled = presenceEnabled;
            if (IsPresenceEnabled)
            {
                InitializeDiscord();
            }
        }

        private void InitializeDiscord()
        {
            discord = new DiscordRPC.DiscordRpcClient("689105200262414377");
            discord.OnError += (a, s) =>
            {
                logger.Error($"{s.Code}, {s.Type}, {s.Message}");
            };

            discord.OnConnectionEstablished += (a, s) =>
            {
                isConnected = true;
                isConnecting = false;
            };

            discord.OnConnectionFailed += (a, s) =>
            {
                isConnecting = false;
                isConnected = false;
                discord.Deinitialize();
                discord.Dispose();
                logger.Error("Discord connection failed");
            };

            isConnecting = true;
            discord.Initialize();
        }

        public void SetPresence(string gameName)
        {
            if (!IsPresenceEnabled)
            {
                return;
            }

            try
            {
                if (!isConnected && !isConnecting)
                {
                    InitializeDiscord();
                }

                var pres = discord.CurrentPresence;

                discord.SetPresence(new DiscordRPC.RichPresence
                {
                    Details = gameName,
                    Assets = new DiscordRPC.Assets
                    {
                        LargeImageKey = "playnite-avatar",
                        LargeImageText = "Playnite game library manager."
                    }
                });
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to set Discord presence");
            }
        }

        public void ClearPresence()
        {
            if (!IsPresenceEnabled)
            {
                return;
            }

            try
            {
                if (!isConnected && !isConnecting)
                {
                    InitializeDiscord();
                }

                var pres = discord.CurrentPresence;
                discord.ClearPresence();
            }
            catch (Exception e) when (!PlayniteEnvironment.ThrowAllErrors)
            {
                logger.Error(e, "Failed to clear Discord presence");
            }
        }

        public void Dispose()
        {
            discord?.Dispose();
        }
    }
}

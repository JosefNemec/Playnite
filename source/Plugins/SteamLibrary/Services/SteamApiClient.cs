using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SteamKit2;

namespace SteamLibrary.Services
{
    public class SteamApiClient
    {
        SteamKit2.SteamClient steamClient;
        CallbackManager manager;
        SteamUser steamUser;
        SteamApps steamApps;

        private bool isRunning = false;

        private bool isConnected = false;
        public bool IsConnected
        {
            get
            {
                return isConnected;
            }
        }

        private bool isLoggedIn = false;
        public bool IsLoggedIn
        {
            get
            {
                return isLoggedIn;
            }
        }

        public SteamApiClient()
        {
            steamClient = new SteamKit2.SteamClient();
            manager = new CallbackManager(steamClient);
            steamUser = steamClient.GetHandler<SteamUser>();
            steamApps = steamClient.GetHandler<SteamApps>();
            manager.Subscribe<SteamKit2.SteamClient.ConnectedCallback>(onConnected);
            manager.Subscribe<SteamKit2.SteamClient.DisconnectedCallback>(onDisconnected);
            manager.Subscribe<SteamUser.LoggedOnCallback>(onLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(onLoggedOff);
        }

        private AutoResetEvent onConnectedEvent = new AutoResetEvent(false);
        private EResult onConnectedResult;
        private void onConnected(SteamKit2.SteamClient.ConnectedCallback callback)
        {
            onConnectedResult = callback.Result;
            onConnectedEvent.Set();
        }

        private AutoResetEvent onDisconnectedEvent = new AutoResetEvent(false);
        private void onDisconnected(SteamKit2.SteamClient.DisconnectedCallback callback)
        {
            onDisconnectedEvent.Set();
        }

        private AutoResetEvent onLoggedOnEvent = new AutoResetEvent(false);
        private EResult onLoggedOnResult;
#pragma warning disable CS1998
        private async void onLoggedOn(SteamUser.LoggedOnCallback callback)
#pragma warning restore CS1998
        {
            onLoggedOnResult = callback.Result;
            onLoggedOnEvent.Set();
        }

        private AutoResetEvent onLoggedOffEvent = new AutoResetEvent(false);
        private void onLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            onLoggedOffEvent.Set();
        }

        public async Task<EResult> Connect()
        {
            steamClient.Connect();
            isRunning = true;
            var result = EResult.OK;

#pragma warning disable CS4014
            Task.Run(() =>
            {
                while (isRunning)
                {
                    manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
                }
            });
#pragma warning restore CS4014

            await Task.Run(() =>
            {
                onConnectedEvent.WaitOne(10000);
                if (onConnectedResult != EResult.OK)
                {
                    isConnected = false;
                    result = onConnectedResult;
                }
                else
                {
                    isConnected = true;
                }
            });

            return result;
        }                

        public async Task<EResult>Login()
        {
            var result = EResult.OK;
            steamUser.LogOnAnonymous();

            await Task.Run(() =>
            {
                onLoggedOnEvent.WaitOne(10000);
                if (onLoggedOnResult != EResult.OK)
                {
                    isLoggedIn = false;
                    result = onLoggedOnResult;
                }
                else
                {
                    isLoggedIn = true;
                }
            });

            return result;
        }

        public void Logout()
        {
            steamClient.Disconnect();
            isConnected = false;
            isLoggedIn = false;
        }

        public async Task<KeyValue> GetProductInfo(uint id)
        {
            if (IsLoggedIn == false)
            {
                throw new Exception("Steam not loggin in.");
            }

            try
            {
                SteamApps.PICSProductInfoCallback productInfo;
                var productJob = steamApps.PICSGetProductInfo(id, package: null, onlyPublic: false);

                AsyncJobMultiple<SteamApps.PICSProductInfoCallback>.ResultSet resultSet = await productJob;

                if (resultSet.Complete)
                {
                    productInfo = resultSet.Results.First();
                }
                else
                {
                    productInfo = resultSet.Results.FirstOrDefault(prodCallback => prodCallback.Apps.ContainsKey(id));
                }

                if (productInfo == null)
                {
                    throw new Exception("Failed to get product info for app " + id);
                }

                return productInfo.Apps[id].KeyValues;
            }
            catch (Exception e)
            {
                throw new Exception("Failed to get product info for app " + id + ". " + e.Message);
            }
        }

        public async Task<KeyValue> GetProductInfo(int id)
        {
            if (!IsConnected)
            {
                var connect = await Connect();
                if (connect != EResult.OK)
                {
                    connect = await Connect();
                    if (connect != EResult.OK)
                    {
                        throw new Exception("Failed to connect to Steam " + connect);
                    }
                }
            }

            if (!IsLoggedIn)
            {
                var logon = await Login();
                if (logon != EResult.OK)
                {
                    throw new Exception("Failed to logon to Steam " + logon);
                }
            }

            return await GetProductInfo(Convert.ToUInt32(id));
        }
    }
}

﻿using EpicLibrary.Models;
using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media;

namespace EpicLibrary.Services
{
    public class TokenException : Exception
    {
        public TokenException(string message) : base(message)
        {
        }
    }

    public class ApiRedirectResponse
    {
        public string redirectUrl { get; set; }
        public string sid { get; set; }
    }

    public class EpicAccountClient
    {
        private ILogger logger = LogManager.GetLogger();
        private IPlayniteAPI api;
        private string tokensPath;
        private readonly string loginUrl = "https://www.epicgames.com/id/login?redirectUrl=https://www.epicgames.com/id/api/redirect";
        private readonly string oauthUrl = @"";
        private readonly string accountUrl = @"";
        private readonly string assetsUrl = @"";
        private readonly string catalogUrl = @"";
        private readonly string playtimeUrl = @"";
        private const string authEcodedString = "MzRhMDJjZjhmNDQxNGUyOWIxNTkyMTg3NmRhMzZmOWE6ZGFhZmJjY2M3Mzc3NDUwMzlkZmZlNTNkOTRmYzc2Y2Y=";

        public EpicAccountClient(IPlayniteAPI api, string tokensPath)
        {
            this.api = api;
            this.tokensPath = tokensPath;
            var oauthUrlMask = @"https://{0}/account/api/oauth/token";
            var accountUrlMask = @"https://{0}/account/api/public/account/";
            var assetsUrlMask = @"https://{0}/launcher/api/public/assets/Windows?label=Live";
            var catalogUrlMask = @"https://{0}/catalog/api/shared/namespace/";
            var playtimeUrlMask = @"https://{0}/library/api/public/playtime/account/{1}/all";

            var loadedFromConfig = false;
            if (!string.IsNullOrEmpty(EpicLauncher.PortalConfigPath) && File.Exists(EpicLauncher.PortalConfigPath))
            {
                try
                {
                    var config = IniParser.Parse(File.ReadAllLines(EpicLauncher.PortalConfigPath));
                    oauthUrl = string.Format(oauthUrlMask, config["Portal.OnlineSubsystemMcp.OnlineIdentityMcp Prod"]["Domain"].TrimEnd('/'));
                    accountUrl = string.Format(accountUrlMask, config["Portal.OnlineSubsystemMcp.OnlineIdentityMcp Prod"]["Domain"].TrimEnd('/'));
                    assetsUrl = string.Format(assetsUrlMask, config["Portal.OnlineSubsystemMcp.BaseServiceMcp Prod"]["Domain"].TrimEnd('/'));
                    catalogUrl = string.Format(catalogUrlMask, config["Portal.OnlineSubsystemMcp.OnlineCatalogServiceMcp Prod"]["Domain"].TrimEnd('/'));
                    playtimeUrl = string.Format(playtimeUrlMask, config["Portal.OnlineSubsystemMcp.OnlineLibraryServiceMcp Prod"]["Domain"].TrimEnd('/'), "{0}");
                    loadedFromConfig = true;
                }
                catch (Exception e) when (!Debugger.IsAttached)
                {
                    logger.Error(e, "Failed to parse portal config, using default API endpoints.");
                }
            }

            if (!loadedFromConfig)
            {
                oauthUrl = string.Format(oauthUrlMask, "account-public-service-prod03.ol.epicgames.com");
                accountUrl = string.Format(accountUrlMask, "account-public-service-prod03.ol.epicgames.com");
                assetsUrl = string.Format(assetsUrlMask, "launcher-public-service-prod06.ol.epicgames.com");
                catalogUrl = string.Format(catalogUrlMask, "catalog-public-service-prod06.ol.epicgames.com");
                playtimeUrl = string.Format(playtimeUrlMask, "library-service.live.use1a.on.epicgames.com", "{0}");
            }
        }

        public void Login()
        {
            var loggedIn = false;
            var apiRedirectContent = string.Empty;
            using (var view = api.WebViews.CreateView(580, 700))
            {
                view.LoadingChanged += async (s, e) =>
                {
                    var address = view.GetCurrentAddress();
                    if (address.StartsWith(@"https://www.epicgames.com/id/api/redirect"))
                    {
                        apiRedirectContent = await view.GetPageTextAsync();
                        loggedIn = true;
                        view.Close();
                    }
                };

                view.DeleteDomainCookies(".epicgames.com");
                view.Navigate(loginUrl);
                view.OpenDialog();
            }

            if (!loggedIn)
            {
                return;
            }

            if (apiRedirectContent.IsNullOrEmpty())
            {
                return;
            }

            var sid = Serialization.FromJson<ApiRedirectResponse>(apiRedirectContent).sid;
            FileSystem.DeleteFile(tokensPath);
            var exchangeKey = getExcahngeToken(sid);
            if (string.IsNullOrEmpty(exchangeKey))
            {
                logger.Error("Failed to get login exchange key for Epic account.");
                return;
            }

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", "basic " + authEcodedString);
                using (var content = new StringContent($"grant_type=exchange_code&exchange_code={exchangeKey}&token_type=eg1"))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    var response = httpClient.PostAsync(oauthUrl, content).GetAwaiter().GetResult();
                    var respContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    FileSystem.CreateDirectory(Path.GetDirectoryName(tokensPath));
                    File.WriteAllText(tokensPath, respContent);
                }
            }
        }

        public bool GetIsUserLoggedIn()
        {
            var tokens = loadTokens();
            if (tokens == null)
            {
                return false;
            }

            try
            {
                var account = InvokeRequest<AccountResponse>(accountUrl + tokens.account_id, tokens).GetAwaiter().GetResult().Item2;
                return account.id == tokens.account_id;
            }
            catch (Exception e)
            {
                if (e is TokenException)
                {
                    renewToknes(tokens.refresh_token);
                    tokens = loadTokens();
                    var account = InvokeRequest<AccountResponse>(accountUrl + tokens.account_id, tokens).GetAwaiter().GetResult().Item2;
                    return account.id == tokens.account_id;
                }
                else
                {
                    logger.Error(e, "Failed to validation Epic authentication.");
                    return false;
                }
            }
        }

        public List<Asset> GetAssets()
        {
            if (!GetIsUserLoggedIn())
            {
                throw new Exception("User is not authenticated.");
            }

            return InvokeRequest<List<Asset>>(assetsUrl, loadTokens()).GetAwaiter().GetResult().Item2;
        }

        public List<PlaytimeItem> GetPlaytimeItems()
        {
            if (!GetIsUserLoggedIn())
            {
                throw new Exception("User is not authenticated.");
            }

            var tokens = loadTokens();
            var formattedPlaytimeUrl = string.Format(playtimeUrl, tokens.account_id);
            
            return InvokeRequest<List<PlaytimeItem>>(formattedPlaytimeUrl, loadTokens()).GetAwaiter().GetResult().Item2;
        }

        public CatalogItem GetCatalogItem(string nameSpace, string id, string cachePath)
        {
            Dictionary<string, CatalogItem> result = null;
            if (!cachePath.IsNullOrEmpty() && File.Exists(cachePath))
            {
                try
                {
                    result = Serialization.FromJsonFile<Dictionary<string, CatalogItem>>(cachePath);
                }
                catch (Exception e)
                {
                    logger.Error(e, "Failed to load Epic catalog cache.");
                }
            }

            if (result == null)
            {
                if (!GetIsUserLoggedIn())
                {
                    throw new Exception("User is not authenticated.");
                }

                var url = string.Format("{0}/bulk/items?id={1}&country=US&locale=en-US", nameSpace, id);
                var catalogResponse = InvokeRequest<Dictionary<string, CatalogItem>>(catalogUrl + url, loadTokens()).GetAwaiter().GetResult();
                result = catalogResponse.Item2;
                FileSystem.WriteStringToFile(cachePath, catalogResponse.Item1);
            }

            if (result.TryGetValue(id, out var catalogItem))
            {
                return catalogItem;
            }
            else
            {
                throw new Exception($"Epic catalog item for {id} {nameSpace} not found.");
            }
        }

        private void renewToknes(string refreshToken)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", "basic " + authEcodedString);
                using (var content = new StringContent($"grant_type=refresh_token&refresh_token={refreshToken}&token_type=eg1"))
                {
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    var response = httpClient.PostAsync(oauthUrl, content).GetAwaiter().GetResult();
                    var respContent = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    FileSystem.CreateDirectory(Path.GetDirectoryName(tokensPath));
                    File.WriteAllText(tokensPath, respContent);
                }
            }
        }

        private async Task<Tuple<string, T>> InvokeRequest<T>(string url, OauthResponse tokens) where T : class
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("Authorization", tokens.token_type + " " + tokens.access_token);
                var response = await httpClient.GetAsync(url);
                var str = await response.Content.ReadAsStringAsync();

                if (Serialization.TryFromJson<ErrorResponse>(str, out var error) && !string.IsNullOrEmpty(error.errorCode))
                {
                    throw new TokenException(error.errorCode);
                }
                else
                {
                    return new Tuple<string, T>(str, Serialization.FromJson<T>(str));
                }
            }
        }

        private OauthResponse loadTokens()
        {
            if (!File.Exists(tokensPath))
            {
                return null;
            }

            return Serialization.FromJson<OauthResponse>(File.ReadAllText(tokensPath));
        }

        private string getExcahngeToken(string sid)
        {
            using (var handler = new HttpClientHandler())
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("X-Epic-Event-Action", "login");
                httpClient.DefaultRequestHeaders.Add("X-Epic-Event-Category", "login");
                httpClient.DefaultRequestHeaders.Add("X-Epic-Strategy-Flags", "");
                httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:86.0) Gecko/20100101 Firefox/86.0");

                httpClient.GetAsync(@"https://www.epicgames.com/id/api/set-sid?sid=" + sid).GetAwaiter().GetResult();
                var resp = httpClient.GetAsync(@"https://www.epicgames.com/id/api/csrf").GetAwaiter().GetResult();
                var cookies = resp.Headers.Single(header => header.Key == "Set-Cookie").Value;
                if (cookies != null)
                {
                    var match = Regex.Match(cookies.First(), @"=(.+);");
                    var xsrf = match.Groups[1].Value;
                    httpClient.DefaultRequestHeaders.Add("X-XSRF-TOKEN", xsrf);
                    resp = httpClient.PostAsync("https://www.epicgames.com/id/api/exchange/generate", null).GetAwaiter().GetResult();
                    var respContent = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    return Serialization.FromJson<Dictionary<string, string>>(respContent)["code"];
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
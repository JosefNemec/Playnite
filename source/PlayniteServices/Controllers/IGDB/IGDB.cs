using ComposableAsync;
using Newtonsoft.Json;
using Playnite.Common;
using Playnite.SDK;
using PlayniteServices.Controllers.IGDB.DataGetter;
using PlayniteServices.Databases;
using PlayniteServices.Models.IGDB;
using RateLimiter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.IGDB
{
    public class IgdbApi
    {
        public class AuthResponse
        {
            public string access_token { get; set; }
        }

        private static ILogger logger = LogManager.GetLogger();
        private static readonly char[] arrayTrim = new char[] { '[', ']' };
        private static readonly JsonSerializer jsonSerializer = new JsonSerializer();
        private readonly UpdatableAppSettings settings;
        private readonly DelegatingHandler requestLimiterHandler;

        public Games Games;
        public AlternativeNames AlternativeNames;
        public InvolvedCompanies InvolvedCompanies;
        public Genres Genres;
        public Websites Websites;
        public GameModes GameModes;
        public PlayerPerspectives PlayerPerspectives;
        public Covers Covers;
        public Artworks Artworks;
        public Screenshots Screenshots;
        public AgeRatings AgeRatings;
        public Collections Collections;

        public string CacheRoot { get; }

        public HttpClient HttpClient { get; }

        public IgdbApi(UpdatableAppSettings settings)
        {
            this.settings = settings;
            requestLimiterHandler = TimeLimiter
                .GetFromMaxCountByInterval(4, TimeSpan.FromSeconds(1))
                .AsDelegatingHandler();
            HttpClient = new HttpClient(requestLimiterHandler);
            HttpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            CacheRoot = settings.Settings.IGDB.CacheDirectory;
            if (!Path.IsPathRooted(CacheRoot))
            {
                CacheRoot = Path.Combine(Paths.ExecutingDirectory, CacheRoot);
            }

            Games = new Games(this);
            AlternativeNames = new AlternativeNames(this);
            InvolvedCompanies = new InvolvedCompanies(this);
            Genres = new Genres(this);
            Websites = new Websites(this);
            GameModes = new GameModes(this);
            PlayerPerspectives = new PlayerPerspectives(this);
            Covers = new Covers(this);
            Artworks = new Artworks(this);
            Screenshots = new Screenshots(this);
            AgeRatings = new AgeRatings(this);
            Collections = new Collections(this);
        }

        private static void SaveTokens(string accessToken)
        {
            File.WriteAllText(
                Path.Combine(Paths.ExecutingDirectory, "twitchTokens.json"),
                JsonConvert.SerializeObject(new Dictionary<string, object>()
                {
                    { nameof(AppSettings.IGDB), new Dictionary<string, string>()
                        {
                            { nameof(AppSettings.IGDB.AccessToken), accessToken },
                        }
                    }
                })
            );
        }

        private async Task Authenticate()
        {
            var clientId = settings.Settings.IGDB.ClientId;
            var clientSecret = settings.Settings.IGDB.ClientSecret;
            var authUrl = $"https://id.twitch.tv/oauth2/token?client_id={clientId}&client_secret={clientSecret}&grant_type=client_credentials";
            var response = await HttpClient.PostAsync(authUrl, null);
            var auth = Serialization.FromJson<AuthResponse>(await response.Content.ReadAsStringAsync());
            if (auth?.access_token.IsNullOrEmpty() != false)
            {
                throw new Exception("Failed to authenticate IGDB.");
            }
            else
            {
                logger.Info("New IGDB auth token generated.");
                settings.Settings.IGDB.AccessToken = auth.access_token;
                SaveTokens(auth.access_token);
            }
        }

        private HttpRequestMessage CreateRequest(string url, string query, string apiKey)
        {
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(settings.Settings.IGDB.ApiEndpoint + url),
                Method = HttpMethod.Post,
                Content = new StringContent(query)
            };

            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Headers.Add("Client-ID", settings.Settings.IGDB.ClientId);
            return request;
        }

        public async Task<string> SendStringRequest(string url, string query, bool reTry = true)
        {
            logger.Debug($"IGDB Live: {url}, {query}");
            var sharedRequest = CreateRequest(url, query, settings.Settings.IGDB.AccessToken);
            var response = await HttpClient.SendAsync(sharedRequest);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return await response.Content.ReadAsStringAsync();
            }

            var authFailed = response.StatusCode == System.Net.HttpStatusCode.Unauthorized || response.StatusCode == System.Net.HttpStatusCode.Forbidden;
            if (authFailed && reTry)
            {
                logger.Error($"IGDB request failed on authentication {response.StatusCode}.");
                await Authenticate();
                return await SendStringRequest(url, query, false);
            }
            else if (authFailed)
            {
                throw new Exception($"Failed to authenticate IGDB {response.StatusCode}.");
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests && reTry)
            {
                await Task.Delay(250);
                return await SendStringRequest(url, query, false);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                throw new Exception($"IGDB failed due to too many requests.");
            }
            else
            {
                logger.Error(await response.Content.ReadAsStringAsync());
                throw new Exception($"Uknown IGDB API response {response.StatusCode}.");
            }
        }

        public async Task<ulong> GetSteamIgdbMatch(ulong gameId)
        {
            var cache = Database.SteamIgdbMatches.FindById(gameId);
            if (cache != null)
            {
                return cache.igdbId;
            }

            var libraryStringResult = await SendStringRequest("games",
                $"fields id; where external_games.uid = \"{gameId}\" & external_games.category = 1; limit 1;");
            var games = JsonConvert.DeserializeObject<List<Game>>(libraryStringResult);
            if (games.Any())
            {
                Database.SteamIgdbMatches.Upsert(new SteamIdGame()
                {
                    steamId = gameId,
                    igdbId = games.First().id
                });

                return games.First().id;
            }
            else
            {
                return 0;
            }
        }

        public async Task<TItem> GetItem<TItem>(ulong itemId, string endpointPath, object cacheLock)
        {
            var cachePath = Path.Combine(CacheRoot, endpointPath, itemId + ".json");
            lock (cacheLock)
            {
                if (System.IO.File.Exists(cachePath))
                {
                    using (var fs = new FileStream(cachePath, FileMode.Open, FileAccess.Read))
                    using (var sr = new StreamReader(fs))
                    using (var reader = new JsonTextReader(sr))
                    {
                        var cacheItem = jsonSerializer.Deserialize<TItem>(reader);
                        if (cacheItem != null)
                        {
                            return cacheItem;
                        }
                    }
                }
            }

            var stringResult = await SendStringRequest(endpointPath, $"fields *; where id = {itemId};");
            var items = Serialization.FromJson<List<TItem>>(stringResult);

            TItem item;
            // IGDB resturns empty results of the id is a duplicate of another game
            if (items.Count > 0)
            {
                item = items[0];
            }
            else
            {
                item = typeof(TItem).CrateInstance<TItem>();
            }

            lock (cacheLock)
            {
                FileSystem.PrepareSaveFile(cachePath);

                if (items.Count > 0)
                {
                    System.IO.File.WriteAllText(cachePath, stringResult.Trim(arrayTrim), Encoding.UTF8);
                }
                else
                {
                    System.IO.File.WriteAllText(cachePath, Serialization.ToJson(item), Encoding.UTF8);
                }
            }

            return item;
        }
    }
}

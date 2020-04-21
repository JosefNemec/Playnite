using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Playnite.Common;
using Playnite.SDK;
using PlayniteServices.Models.GitHub;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteServices.Controllers.Webhooks
{
    [Route("hooks/github")]
    public class GitHubController : Controller
    {
        private static HttpClient httpClient { get; } = new HttpClient();
        private static ILogger logger = LogManager.GetLogger();
        private AppSettings appSettings;

        public GitHubController(IOptions<AppSettings> settings)
        {
            appSettings = settings.Value;
        }

        private static string GetPayloadHash(string payload, string key)
        {
            var encoding = new UTF8Encoding();
            var textBytes = encoding.GetBytes(payload);
            var keyBytes = encoding.GetBytes(key);
            using (var hash = new HMACSHA1(keyBytes))
            {
                return BitConverter.ToString(hash.ComputeHash(textBytes)).Replace("-", "").ToLower();
            }
        }

        [HttpPost]
        public async Task<ActionResult> GithubWebhook()
        {
            if (Request.Headers.TryGetValue("X-Hub-Signature", out var sig))
            {
                if (!Request.Headers.TryGetValue("X-GitHub-Event", out var eventType))
                {
                    return BadRequest("No event.");
                }

                string payloadString = null;
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    payloadString = await reader.ReadToEndAsync();
                }

                var payloadHash = GetPayloadHash(payloadString, appSettings.GitHub.GitHubSecret);
                if (sig != $"sha1={payloadHash}")
                {
                    return BadRequest("Signature check failed.");
                }

                var forwardEvent = true;
                if (eventType == WebHookEvents.Issues)
                {
                    var payload = Serialization.FromJson<IssuesEvent>(payloadString);

                    // Only forward opened issues
                    if (payload.action != IssuesEventAction.opened)
                    {
                        forwardEvent = false;
                        logger.Debug("Ignored non-opened github issue webhook.");
                    }
                }
                else if (eventType == WebHookEvents.Push)
                {
                    var payload = Serialization.FromJson<PushEvent>(payloadString);

                    // Ignore localization pushes
                    if (payload.@ref?.EndsWith("l10n_devel") == true)
                    {
                        forwardEvent = false;
                        logger.Debug("Ignored l10n_devel github webhook.");
                    }
                    // Don't forward branch merges
                    else if (payload.commits?.Any(a => a.message.StartsWith("Merge branch")) == true)
                    {
                        forwardEvent = false;
                        payload.commits = payload.commits.Where(a => !a.message.StartsWith("Merge branch")).ToList();
                        if (payload.commits.HasItems())
                        {
                            logger.Debug("Forwarded commits without merge commits.");
                            await FormardRequest(Request, JsonConvert.SerializeObject(payload));
                        }
                        else
                        {
                            logger.Debug("Ignored commits with only merge commits.");
                        }
                    }
                }

                if (forwardEvent)
                {
                    await FormardRequest(Request, payloadString);
                }

                return Ok();
            }

            return BadRequest();
        }

        private async Task FormardRequest(HttpRequest request, string payload)
        {
            var cnt = new StringContent(payload, Encoding.UTF8, "application/json");
            cnt.Headers.Add("X-GitHub-Delivery", Request.Headers["X-GitHub-Delivery"].FirstOrDefault());
            cnt.Headers.Add("X-GitHub-Event", Request.Headers["X-GitHub-Event"].FirstOrDefault());
            var discordResp = await httpClient.PostAsync(
                appSettings.GitHub.DiscordWebhookUrl,
                cnt);
            await discordResp.Content.ReadAsStringAsync();
        }
    }
}

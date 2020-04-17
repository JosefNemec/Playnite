using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
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

                var payload = Serialization.FromJson<GitHubWebhook>(payloadString);

                // Ignore localization pushes
                if (payload.referer?.EndsWith("l10n_devel") == true)
                {
                    logger.Debug("Ignored l10n_devel github webhook.");
                }
                else
                {
                    var cnt = new StringContent(payloadString, Encoding.UTF8, "application/json");
                    cnt.Headers.Add("X-GitHub-Delivery", Request.Headers["X-GitHub-Delivery"].FirstOrDefault());
                    cnt.Headers.Add("X-GitHub-Event", Request.Headers["X-GitHub-Event"].FirstOrDefault());
                    var discordResp = await httpClient.PostAsync(
                        appSettings.GitHub.DiscordWebhookUrl,
                        cnt);
                    await discordResp.Content.ReadAsStringAsync();
                }

                return Ok();
            }

            return BadRequest();
        }
    }
}

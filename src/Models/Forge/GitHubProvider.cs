using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SourceGit.Models.Forge
{
    public class GitHubProvider : IForgeProvider
    {
        public ForgeKind Kind => ForgeKind.GitHub;

        private static readonly Regex LINK_LAST_PAGE = new Regex(
            @"[?&]page=(?<page>\d+)(?:&[^>]*)?>;\s*rel=""last""",
            RegexOptions.Compiled);

        public async Task<ForgeCountResult> GetOpenPullRequestCountAsync(ForgeRemote remote, CancellationToken token)
        {
            var result = new ForgeCountResult();

            var credential = ForgeCredentialStore.Get(TokenKey(remote));
            if (string.IsNullOrEmpty(credential))
                return result;

            try
            {
                using var client = CreateClient(credential);
                var url = $"https://api.github.com/repos/{remote.Owner}/{remote.Repo}/pulls?state=open&per_page=1";
                using var response = await client.GetAsync(url, token).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    // 401 = bad/expired token. 403 = forbidden, unless it is a rate-limit answer.
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        result.AuthFailed = true;
                    else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden &&
                             !IsRateLimited(response))
                        result.AuthFailed = true;
                    return result;
                }

                // With per_page=1, the last page number in the Link header equals the total count.
                if (response.Headers.TryGetValues("Link", out var links))
                {
                    foreach (var link in links)
                    {
                        var match = LINK_LAST_PAGE.Match(link);
                        if (match.Success && int.TryParse(match.Groups["page"].Value, out var count))
                        {
                            result.Count = count;
                            return result;
                        }
                    }
                }

                var body = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
                var trimmed = body.TrimStart();
                result.Count = trimmed.StartsWith("[") && trimmed.Length > 3 ? 1 : 0;
                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        private static bool IsRateLimited(HttpResponseMessage response)
        {
            return response.Headers.TryGetValues("x-ratelimit-remaining", out var values) &&
                   values.Any(v => v == "0");
        }

        private static HttpClient CreateClient(string token)
        {
            var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SourceGit", "1.0"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");
            return client;
        }

        private static string TokenKey(ForgeRemote remote) => $"github:{remote.Host.ToLowerInvariant()}";
    }
}

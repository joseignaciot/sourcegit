using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SourceGit.Models.Forge
{
    public class GitLabProvider : IForgeProvider
    {
        public ForgeKind Kind => ForgeKind.GitLab;

        public async Task<int?> GetOpenPullRequestCountAsync(ForgeRemote remote, CancellationToken token)
        {
            var credential = ForgeCredentialStore.Get(TokenKey(remote));
            if (string.IsNullOrEmpty(credential))
                return null;

            try
            {
                using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                client.DefaultRequestHeaders.Add("PRIVATE-TOKEN", credential);

                var project = Uri.EscapeDataString($"{remote.Owner}/{remote.Repo}");
                var url = $"https://{remote.Host}/api/v4/projects/{project}/merge_requests?state=opened&per_page=1";
                using var response = await client.GetAsync(url, token).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                    return null;

                // GitLab returns the total in the x-total header.
                if (response.Headers.TryGetValues("x-total", out var totals))
                {
                    foreach (var total in totals)
                    {
                        if (int.TryParse(total, out var count))
                            return count;
                    }
                }

                var body = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
                var trimmed = body.TrimStart();
                return trimmed.StartsWith("[") && trimmed.Length > 3 ? 1 : 0;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string TokenKey(ForgeRemote remote) => $"gitlab:{remote.Host.ToLowerInvariant()}";
    }
}

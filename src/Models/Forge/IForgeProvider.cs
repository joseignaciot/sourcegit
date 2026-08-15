using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SourceGit.Models.Forge
{
    public enum ForgeKind
    {
        Unknown = 0,
        GitHub,
        GitLab,
    }

    public class ForgeRemote
    {
        public ForgeKind Kind { get; set; } = ForgeKind.Unknown;
        public string Host { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
        public string Repo { get; set; } = string.Empty;
        public bool IsValid => Kind != ForgeKind.Unknown && !string.IsNullOrEmpty(Owner) && !string.IsNullOrEmpty(Repo);
    }

    public interface IForgeProvider
    {
        ForgeKind Kind { get; }

        // Returns the number of open PRs/MRs, or null when the forge is not
        // configured (no token) or the request failed. Never throws.
        Task<int?> GetOpenPullRequestCountAsync(ForgeRemote remote, CancellationToken token);
    }

    public static class ForgeProviders
    {
        public static IForgeProvider For(ForgeKind kind) => kind switch
        {
            ForgeKind.GitHub => new GitHubProvider(),
            ForgeKind.GitLab => new GitLabProvider(),
            _ => null,
        };
    }

    public static class ForgeRemoteParser
    {
        private static readonly Regex HTTPS_PATTERN = new Regex(
            @"^https?://(?:[^@/]+@)?(?<host>[^/]+)/(?<owner>[^/]+)/(?<repo>[^/]+?)(?:\.git)?/?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SSH_PATTERN = new Regex(
            @"^git@(?<host>[^:]+):(?<owner>[^/]+)/(?<repo>[^/]+?)(?:\.git)?$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static ForgeRemote Parse(string remoteUrl)
        {
            if (string.IsNullOrWhiteSpace(remoteUrl))
                return new ForgeRemote();

            var match = HTTPS_PATTERN.Match(remoteUrl.Trim());
            if (!match.Success)
                match = SSH_PATTERN.Match(remoteUrl.Trim());
            if (!match.Success)
                return new ForgeRemote();

            var host = match.Groups["host"].Value;
            var kind = ForgeKind.Unknown;
            if (host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
                kind = ForgeKind.GitHub;
            else if (host.Equals("gitlab.com", StringComparison.OrdinalIgnoreCase))
                kind = ForgeKind.GitLab;
            else if (ViewModels.Preferences.Instance.IsGitLabHost(host))
                kind = ForgeKind.GitLab;

            return new ForgeRemote
            {
                Kind = kind,
                Host = host,
                Owner = match.Groups["owner"].Value,
                Repo = match.Groups["repo"].Value,
            };
        }
    }
}

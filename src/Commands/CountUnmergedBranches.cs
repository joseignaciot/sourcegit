using System;
using System.Threading.Tasks;

namespace SourceGit.Commands
{
    // Counts local branches that contain commits not reachable from the default branch.
    // Default branch resolution: refs/remotes/origin/HEAD -> local main -> local master.
    // Returns null when no default branch exists (indicator disabled for that repo).
    public class CountUnmergedBranches : Command
    {
        public CountUnmergedBranches(string repo)
        {
            WorkingDirectory = repo;
            Context = repo;
            RaiseError = false;
        }

        public async Task<int?> GetResultAsync()
        {
            var defaultBranch = await ResolveDefaultBranchAsync().ConfigureAwait(false);
            if (defaultBranch == null)
                return null;

            Args = $"branch --no-merged {defaultBranch} --format=%(refname:short)";
            var rs = await ReadToEndAsync().ConfigureAwait(false);
            if (!rs.IsSuccess)
                return null;

            var lines = rs.StdOut.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            return lines.Length;
        }

        private async Task<string> ResolveDefaultBranchAsync()
        {
            Args = "symbolic-ref --quiet --short refs/remotes/origin/HEAD";
            var rs = await ReadToEndAsync().ConfigureAwait(false);
            if (rs.IsSuccess && !string.IsNullOrWhiteSpace(rs.StdOut))
                return rs.StdOut.Trim();

            foreach (var candidate in new[] { "main", "master" })
            {
                Args = $"show-ref --verify --quiet refs/heads/{candidate}";
                rs = await ReadToEndAsync().ConfigureAwait(false);
                if (rs.IsSuccess)
                    return candidate;
            }

            return null;
        }
    }
}

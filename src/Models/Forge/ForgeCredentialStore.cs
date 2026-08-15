using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace SourceGit.Models.Forge
{
    // Secure token storage via the OS keychain. Tokens NEVER go into Preferences JSON.
    // macOS is implemented via the `security` CLI; other platforms report unsupported.
    //
    // Known platform limitation: the security CLI accepts the secret only via the -w
    // argument, so the token is briefly visible to same-user processes in `ps`.
    public static class ForgeCredentialStore
    {
        private const string SERVICE_PREFIX = "com.sourcegit.forge.";

        private static readonly Regex TOKEN_PATTERN = new Regex(
            @"^[A-Za-z0-9_\-\.=+/]+$",
            RegexOptions.Compiled);

        public static bool IsSupported => OperatingSystem.IsMacOS();

        public static string Get(string key)
        {
            return Run($"find-generic-password -s \"{SERVICE_PREFIX}{Escape(key)}\" -w");
        }

        public static bool Save(string key, string token)
        {
            // Defense-in-depth: PATs are restricted to a known charset so no argument
            // escaping edge case can ever alter the command line (UseShellExecute=false
            // already prevents shell interpretation).
            if (string.IsNullOrEmpty(token) || !TOKEN_PATTERN.IsMatch(token))
                return false;

            // -U updates the item when it already exists.
            return Run($"add-generic-password -U -s \"{SERVICE_PREFIX}{Escape(key)}\" -a gitvisual -w \"{Escape(token)}\"") != null;
        }

        public static bool Delete(string key)
        {
            return Run($"delete-generic-password -s \"{SERVICE_PREFIX}{Escape(key)}\"") != null;
        }

        // Returns the trimmed stdout on exit code 0, "" for successful commands with no
        // output, or null on any failure (unsupported OS, start failure, timeout, error).
        private static string Run(string arguments, int timeoutMs = 5000)
        {
            if (!IsSupported)
                return null;

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "/usr/bin/security",
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = Process.Start(psi);
                if (process == null)
                    return null;

                var outputTask = process.StandardOutput.ReadToEndAsync();
                if (!process.WaitForExit(timeoutMs))
                {
                    try { process.Kill(); } catch { }
                    return null;
                }

                var output = outputTask.GetAwaiter().GetResult().Trim();
                return process.ExitCode == 0 ? output : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string Escape(string value) => value.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }
}

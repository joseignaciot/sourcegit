using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace SourceGit.ViewModels
{
    public class EnterForgeToken : Popup
    {
        [Required(ErrorMessage = "Host is required!")]
        public string Host
        {
            get => _host;
            set => SetProperty(ref _host, value, true);
        }

        public bool IsGitLab
        {
            get => _isGitLab;
            set => SetProperty(ref _isGitLab, value);
        }

        [Required(ErrorMessage = "Token is required!")]
        public string Token
        {
            get => _token;
            set => SetProperty(ref _token, value, true);
        }

        public override Task<bool> Sure()
        {
            var host = _host?.Trim().ToLowerInvariant() ?? string.Empty;
            if (host.Length == 0)
                return Task.FromResult(false);

            var kind = _isGitLab ? "gitlab" : "github";

            if (!Models.Forge.ForgeCredentialStore.Save($"{kind}:{host}", _token.Trim()))
            {
                Models.Notification.Send(null, App.Text("ForgeToken.SaveFailed"), true);
                return Task.FromResult(false);
            }

            Preferences.Instance.SetForgeHostKind(host, kind);
            _ = Welcome.Instance.UpdateStatusAsync(true, null);
            return Task.FromResult(true);
        }

        private string _host = string.Empty;
        private bool _isGitLab = false;
        private string _token = string.Empty;
    }
}

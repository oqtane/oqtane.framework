using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Oqtane.Services;
using Oqtane.Shared;
using System;
using System.Threading.Tasks;

namespace Oqtane.UI
{
    public partial class Installer
    {
        private readonly NavigationManager _navigationManager;
        private readonly IInstallationService _installationService;
        private readonly IJSRuntime _jsRuntime;

        public Installer(NavigationManager navigationManager, IInstallationService installationService, IJSRuntime jsRuntime)
        {
            _navigationManager = navigationManager;
            _installationService = installationService;
            _jsRuntime = jsRuntime;
        }

        public string DatabaseType { get; set; } = "LocalDB";

        public string ServerName { get; set; } = "(LocalDb)\\MSSQLLocalDB";

        public string DatabaseName { get; set; } = "Oqtane-" + DateTime.UtcNow.ToString("yyyyMMddHHmm");

        public string Username { get; set; }

        public string Password { get; set; }

        public string HostUsername { get; set; } = Constants.HostUser;

        public string HostPassword { get; set; }

        public string ConfirmPassword { get; set; }

        public string HostEmail { get; set; }

        public string Message { get; private set; }

        public string IntegratedSecurityDisplay { get; private set; } = "display: none;";

        public string LoadingDisplay { get; private set; } = "display: none;";

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                var interop = new Interop(_jsRuntime);
                await interop.IncludeLink("app-stylesheet", "stylesheet", "https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css", "text/css", "sha384-ggOyR0iXCbMQv3Xipma34MD+dH/1fQ784/j6cY/iJTQUOhcWr7x9JvoRxT2MZw1T", "anonymous", string.Empty);
            }
        }

        private void SetIntegratedSecurity(ChangeEventArgs e)
        {
            IntegratedSecurityDisplay = Convert.ToBoolean((string)e.Value)
                ? "display: none;"
                : string.Empty;
        }

        private async Task Install()
        {
            if (ServerName != string.Empty && !string.IsNullOrEmpty(DatabaseName) && HostUsername != string.Empty && HostPassword.Length >= 6 && HostPassword == ConfirmPassword && !string.IsNullOrEmpty(HostEmail))
            {
                LoadingDisplay = string.Empty;
                StateHasChanged();

                var connectionstring = string.Empty;
                if (DatabaseType == "LocalDB")
                {
                    connectionstring = "Data Source=" + ServerName + ";AttachDbFilename=|DataDirectory|\\" + DatabaseName + ".mdf;Initial Catalog=" + DatabaseName + ";Integrated Security=SSPI;";
                }
                else
                {
                    connectionstring = "Data Source=" + ServerName + ";Initial Catalog=" + DatabaseName + ";";
                    if (IntegratedSecurityDisplay == "display: none;")
                    {
                        connectionstring += "Integrated Security=SSPI;";
                    }
                    else
                    {
                        connectionstring += "User ID=" + Username + ";Password=" + Password;
                    }
                }

                Uri uri = new Uri(_navigationManager.Uri);

                var config = new InstallConfig
                {
                    ConnectionString = connectionstring,
                    Aliases = uri.Authority,
                    HostEmail = HostEmail,
                    HostPassword = HostPassword,
                    HostName = Constants.HostUser,
                    TenantName = Constants.MasterTenant,
                    IsNewTenant = true,
                    SiteName = Constants.DefaultSite
                };

                var installation = await _installationService.Install(config);
                if (installation.Success)
                {
                    _navigationManager.NavigateTo(uri.Scheme + "://" + uri.Authority, true);
                }
                else
                {
                    Message = "<div class=\"alert alert-danger\" role=\"alert\">" + installation.Message + "</div>";
                    LoadingDisplay = "display: none;";
                }
            }
            else
            {
                Message = "<div class=\"alert alert-danger\" role=\"alert\">Please Enter All Fields And Ensure Passwords Match And Are Greater Than 5 Characters In Length</div>";
            }
        }
    }
}

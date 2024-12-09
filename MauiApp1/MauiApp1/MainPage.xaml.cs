using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Maui.Controls;
using Microsoft.Identity.Client;

namespace MauiApp1
{
    public partial class MainPage : ContentPage
    {
        private string? _currentAccessToken;
        private readonly PlatformActivityLoaderService _platformActivityLoaderService;

        public MainPage(PlatformActivityLoaderService platformActivityLoaderService)
        {
            InitializeComponent();
            _platformActivityLoaderService = platformActivityLoaderService;
        }
        private async void OnLoginClicked(object sender, EventArgs e)
        {
            try
            {
                editor.Text = "Login Clicked";

                _currentAccessToken = await _platformActivityLoaderService.GetTokenByMsalActivity();
                editor.Text = "Loggin successful";
            }
            catch (Exception ex)
            {
                editor.Text = "Error: " + ex.Message;
                editor.Text = "Loggin successful";
            }
        }


        private async void OnApiClicked(object sender, EventArgs e)
        {
            try
            {
                editor.Text = "API Clicked";
                if (_currentAccessToken != null)
                {
                    var handler = new HttpClientHandler();
                    handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                    handler.ServerCertificateCustomValidationCallback =
                        (httpRequestMessage, cert, cetChain, policyErrors) =>
                        {
                            return true;
                        };
                    using (var client = new HttpClient(handler))
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _currentAccessToken);
                        var response = await client.GetAsync("https://192.168.86.243:7274/WeatherForecast");
                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();
                            var doc = JsonDocument.Parse(content).RootElement;
                            editor.Text = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
                        }
                        else
                        {
                            editor.Text = response.ReasonPhrase;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                editor.Text = ex.Message;
            }
        }
    }

    public class PlatformActivityLoaderService
    {
        string authority = "https://mauib2cauth.b2clogin.com/tfp/mauib2cauth.onmicrosoft.com/b2c_1_mauisigninup";
        string _clientId = "5639cbdd-a251-4252-8eca-a82d97b65f99";
        string _redirectUri = "myapp://callback";
        string[] _scope = ["5639cbdd-a251-4252-8eca-a82d97b65f99", "offline_access"];
        private IPublicClientApplication _masalPCA;

        public PlatformActivityLoaderService()
        {
            _masalPCA = PublicClientApplicationBuilder
            .Create(_clientId)
            .WithRedirectUri(_redirectUri)
            .WithB2CAuthority(authority)
            .WithParentActivityOrWindow(() => Platform.CurrentActivity) // Required for Android
            .Build();
        }
        public async Task<string> GetTokenByMsalActivity()
        {
            AuthenticationResult? result = null;
            try
            {
                var accounts = await _masalPCA.GetAccountsAsync();
                var account = accounts.FirstOrDefault();
                result = await _masalPCA.AcquireTokenSilent(_scope, account).ExecuteAsync();
            }
            catch (MsalUiRequiredException)
            {
                result = null;
            }

            if (result == null)
            {
                result = await _masalPCA.AcquireTokenInteractive(_scope).ExecuteAsync();
            }
            return result.AccessToken;

        }
    }
}
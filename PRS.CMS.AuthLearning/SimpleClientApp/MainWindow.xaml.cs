using System.Windows;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using IdentityModel.Client;

namespace SimpleClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DiscoveryResponse _discoResponse;

        public MainWindow()
        {
            InitializeComponent();
        }

        public async Task<DiscoveryResponse> GetDiscoInfo()
        {
            if (_discoResponse != null)
            {
                return _discoResponse;
            }

            var disco = new DiscoveryClient(OpenIdConstants.Authority.ToString());
            _discoResponse = await disco.GetAsync();
            return _discoResponse;
        }

        private async void Button_Click_Browser(object sender, RoutedEventArgs e)
        {
            var disco = await GetDiscoInfo();
            var proxy = new OpenIdClientProxy(disco);

            var tokenResponse = await proxy.Authenticate();
            tokenTextBlock.Text = tokenResponse.AccessToken;

            await DisplayTokenResponse(tokenResponse, disco);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var disco = await GetDiscoInfo();
            var client = new TokenClient(disco.TokenEndpoint, OpenIdConstants.Client2Id, OpenIdConstants.ClientSecret);
            
            var userName = UserNameTextBox.Text;
            var password = PasswordTextBox.Text;

            // the ResourceOwnerPassword means that we are collecting the password in some means
            // most oidc clients want to perform a redirect to a login page.  This should only
            // be done when the application getting the password from the user is TRUSTED.  Our
            // server should only allow ResourceOwner from known clients that we have written.
            // the offline_access is what gives us the ability to request a refresh token
            var tokenResponse = client.RequestResourceOwnerPasswordAsync(userName, password, "openid profile email offline_access").Result;
            tokenTextBlock.Text = tokenResponse.AccessToken;

            await DisplayTokenResponse(tokenResponse, disco);
        }

        private async void Button_Click_WsFedServer(object sender, RoutedEventArgs e)
        {
            ServicePointManager.ServerCertificateValidationCallback = (o, certificate, chain, errors) => true;
            var handler = new HttpClientHandler
            {
                UseDefaultCredentials = true,
            };

            var authority = "https://localhost:44302/windows";

            //PRS.CMS.AuthLearning.WsFedServer

            var client = new TokenClient(authority + "/token", handler)
            {
                ClientId = OpenIdConstants.Client2Id,
                ClientSecret = OpenIdConstants.ClientSecret
            };

            var tokenResponse = client.RequestCustomGrantAsync("windows", "openid profile offline_access").Result;

            await DisplayTokenResponse(tokenResponse, authority + "/userinfo", handler);
        }

        private async void Button_Click_WinAuth(object sender, RoutedEventArgs e)
        {
            var handler = new HttpClientHandler
            {
                UseDefaultCredentials = true
            };

            //PRS.CMS.AuthLearning.WinAuth
            var disco = new DiscoveryClient("http://localhost:26712/", handler);
            var discoResponse = await disco.GetAsync();

            var client = new TokenClient(discoResponse.TokenEndpoint, handler)
            {
                ClientId = OpenIdConstants.Client2Id,
                ClientSecret = OpenIdConstants.ClientSecret
            };

            var tokenResponse = client.RequestCustomGrantAsync("windows", "openid profile offline_access").Result;

            await DisplayTokenResponse(tokenResponse, discoResponse, handler);
        }

        private static async Task DisplayTokenResponse(TokenResponse tokenResponse, DiscoveryResponse disco, HttpClientHandler handler = null)
        {
            await DisplayTokenResponse(tokenResponse, disco.UserInfoEndpoint, handler);
        }

        private static async Task DisplayTokenResponse(TokenResponse tokenResponse, string userInfoEndpoint, HttpClientHandler handler = null)
        {
            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                return;
            }

            // reads the base64 encoded content in the AccessToken into an actual SecurityToken
            var accessToken = new JwtSecurityTokenHandler().ReadToken(tokenResponse.AccessToken);

            // need to get the claims for the user because the Token doesn't contain any user info other than
            // giving us the ability to access APIs as that user
            var userInfoClient = handler == null
                ? new UserInfoClient(userInfoEndpoint)
                : new UserInfoClient(userInfoEndpoint, handler);

            var userInfoResponse = await userInfoClient.GetAsync(tokenResponse.AccessToken);

            // can look through Claims to see what info is in the Token returned
            var claims = userInfoResponse.Claims;
        }
    }
}

using System;
using System.Windows;
using System.IdentityModel.Tokens;
using System.Net;
using System.Net.Http;
using IdentityModel.Client;

namespace SimpleClientApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var connectRootUri = "http://localhost:6159/AuthServer/identity/connect";

            var client = new TokenClient(connectRootUri + "/token",
                "SimpleClientApp",
                "secret");
            
            var userName = UserNameTextBox.Text;
            var password = PasswordTextBox.Text;

            // the ResourceOwnerPassword means that we are collecting the password in some means
            // most oidc clients want to perform a redirect to a login page.  This should only
            // be done when the application getting the password from the user is TRUSTED.  Our
            // server should only allow ResourceOwner from known clients that we have written.
            // the offline_access is what gives us the ability to request a refresh token
            var tokenResponse = client.RequestResourceOwnerPasswordAsync(userName, password, "openid profile email offline_access").Result;
            tokenTextBlock.Text = tokenResponse.AccessToken;

            DisplayTokenResponse(tokenResponse, connectRootUri);
        }

        private void Button_Click_WsFedServer(object sender, RoutedEventArgs e)
        {
            ServicePointManager.ServerCertificateValidationCallback = (o, certificate, chain, errors) => true;

            var handler = new HttpClientHandler
            {
                UseDefaultCredentials = true,
            };
            //PRS.CMS.AuthLearning.WsFedServer
            var connectRootUri = "https://localhost:44302";

            var client = new TokenClient(connectRootUri + "/windows/token", handler)
            {
                ClientId = "SimpleClientApp",
                ClientSecret = "secret"
            };

            var tokenResponse = client.RequestCustomGrantAsync("windows", "openid profile offline_access").Result;

            DisplayTokenResponse(tokenResponse, connectRootUri, handler);
        }

        private void Button_Click_WinAuth(object sender, RoutedEventArgs e)
        {
            var handler = new HttpClientHandler
            {
                UseDefaultCredentials = true
            };
            //PRS.CMS.AuthLearning.WinAuth
            var connectRootUri = "http://localhost:26712/token";

            var client = new TokenClient(connectRootUri, handler)
            {
                ClientId = "SimpleClientApp",
                ClientSecret = "secret"
            };

            var tokenResponse = client.RequestCustomGrantAsync("windows", "openid profile offline_access").Result;

            DisplayTokenResponse(tokenResponse, connectRootUri, handler);
        }

        private static void DisplayTokenResponse(TokenResponse tokenResponse, string connectRootUri, HttpClientHandler handler = null)
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
                ? new UserInfoClient(new Uri(connectRootUri + "/userinfo"), tokenResponse.AccessToken)
                : new UserInfoClient(new Uri(connectRootUri + "/userinfo"), tokenResponse.AccessToken, handler);

            var userInfoResponse = userInfoClient.GetAsync().Result;

            // can look through Claims to see what info is in the Token returned
            var claims = userInfoResponse.Claims;
            
        }

    }
}

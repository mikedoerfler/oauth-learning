using System.Windows;
using System.IdentityModel.Tokens;
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
            var connectRootUri = "https://localhost:44300/AuthServer/identity/connect";

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

            // reads the base64 encoded content in the AccessToken into an actual SecurityToken
            var securityToken = new JwtSecurityTokenHandler().ReadToken(tokenResponse.AccessToken);

            // need to get the claims for the user because the Token doesn't contain any user info other than
            // giving us the ability to access APIs as that user
            var userInfoClient = new UserInfoClient(new System.Uri(connectRootUri + "/userinfo"), tokenResponse.AccessToken);
            var userInfoResponse = userInfoClient.GetAsync().Result;

            // can look through Claims to see what info is in the Token returned
            var claims = userInfoResponse.Claims;
        }
    }
}

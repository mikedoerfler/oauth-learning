using System;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;

namespace SimpleClientApp
{
    public class OpenIdClientProxy
    {
        /*
         * based on sample from Google at
         * https://github.com/googlesamples/oauth-apps-for-windows/blob/master/OAuthConsoleApp/OAuthConsoleApp/Program.cs
         */
          
        public static int GetRandomUnusedPort()
        {/*
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
            */
            return 9549;
        }

        public async Task<TokenResponse> Authenticate()
        {
            // Generates state and PKCE values.
            var state = CryptoRandom.CreateUniqueId();

            // Creates a redirect URI using an available port on the loopback address.
            var redirectUri = $"http://{IPAddress.Loopback}:{GetRandomUnusedPort()}/";
            Console.WriteLine("redirect URI: " + redirectUri);

            var request = new AuthorizeRequest(OpenIdConstants.AuthorizeEndpoint.ToString());
            var url = request.CreateAuthorizeUrl(
                clientId: OpenIdConstants.ClientId,
                responseType: OidcConstants.ResponseTypes.Code,
                responseMode: OidcConstants.ResponseModes.Query,
                redirectUri: redirectUri,
                state: state,
                nonce: Guid.NewGuid().ToString(),
                scope: "openid profile offline_access"
            );

            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectUri);
            Console.WriteLine("Listening..");
            http.Start();

            // Opens request in the browser.
            System.Diagnostics.Process.Start(url);

            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            // Sends an HTTP response to the browser.
            var response = context.Response;
            var responseString = "<html><head><meta http-equiv=\'refresh\' content=\'10;url=https://google.com\'></head><body>Please return to the app.</body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            Task responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
                Console.WriteLine("HTTP server stopped.");
            });

            var authorizeResponse = new AuthorizeResponse(context.Request.RawUrl);

            // Checks for errors.
            if (authorizeResponse.Error != null)
            {
                var message = $"OAuth authorization error: {authorizeResponse.Error}.";
                throw new Exception(message);
            }

            // extracts the code
            var code = authorizeResponse.Code;

            if (code == null || authorizeResponse.State == null)
            {
                var message = "Malformed authorization response. Missing code or incoming state";
                throw new Exception(message);
            }

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (authorizeResponse.State != state)
            {
                var message = $"Received request with invalid state ({authorizeResponse.State})";
                throw new Exception(message);
            }
            Console.WriteLine("Authorization code: " + code);

            var tokenEndpointClient = new TokenClient(OpenIdConstants.TokenEndpoint.ToString(), OpenIdConstants.ClientId,
                OpenIdConstants.ClientSecret, AuthenticationStyle.PostValues);

            var tokenResponse = await tokenEndpointClient.RequestAuthorizationCodeAsync(code, redirectUri);

            var userInfoEndpointClient = new UserInfoClient(OpenIdConstants.UserInfoEndpoint.ToString());
            var userInfoResponse = await userInfoEndpointClient.GetAsync(tokenResponse.AccessToken);

            Console.WriteLine("AccessToken = {0}", tokenResponse.AccessToken);
            Console.WriteLine("IdentityToken = {0}", tokenResponse.IdentityToken);
            Console.WriteLine("RefreshToken = {0}", tokenResponse.RefreshToken);
            Console.WriteLine("TokenType = {0}", tokenResponse.TokenType);
            Console.WriteLine("UserClaims.Count = {0}", userInfoResponse.Claims.Count());

            return tokenResponse;
        }
    }
}

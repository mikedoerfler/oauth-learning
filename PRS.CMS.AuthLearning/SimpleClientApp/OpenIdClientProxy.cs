using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using IdentityModel.Client;
using Microsoft.IdentityModel.Tokens;

namespace SimpleClientApp
{
    public static class NameValueCollectionExtensions
    {
        public static string ToQueryString(this NameValueCollection collection)
        {
            var items = new List<string>();
            foreach (var key in collection.AllKeys)
            {
                var qsKey = HttpUtility.UrlEncode(key);
                var values = collection.GetValues(key)
                    .Select(v => string.Format("{0}={1}", qsKey, HttpUtility.UrlEncode(v)))
                    .ToArray();
                items.AddRange(values);
            }

            return "?" + string.Join("&", items.ToArray());
        }
    }

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

        public async void Authenticate()
        {
            // Generates state and PKCE values.
            var state = RandomDataBase64Url(32);

            // Creates a redirect URI using an available port on the loopback address.
            var redirectUri = $"http://{IPAddress.Loopback}:{GetRandomUnusedPort()}/";
            Console.WriteLine("redirect URI: " + redirectUri);

            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = new HttpListener();
            http.Prefixes.Add(redirectUri);
            Console.WriteLine("Listening..");
            http.Start();

            // Creates the OAuth 2.0 authorization request.
            var qs = new NameValueCollection
            {
                ["response_type"] = "code",
                ["scope"] = "openid profile",
                ["client_id"] = OpenIdConstants.ClientId,
                ["state"] = state,
                ["nonce"] = Guid.NewGuid().ToString(),
                ["redirect_uri"] = redirectUri
            };

            var authorizationRequest = OpenIdConstants.AuthorizeEndpoint + qs.ToQueryString();

            // Opens request in the browser.
            System.Diagnostics.Process.Start(authorizationRequest);

            // Waits for the OAuth authorization response.
            var context = await http.GetContextAsync();

            // Brings the Console to Focus.
            //BringConsoleToFront();

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

            // Checks for errors.
            var reqQueryString = context.Request.QueryString;
            if (reqQueryString.Get("error") != null)
            {
                Console.WriteLine("OAuth authorization error: {0}.", reqQueryString.Get("error"));
                return;
            }
            if (reqQueryString.Get("code") == null
                || reqQueryString.Get("state") == null)
            {
                Console.WriteLine("Malformed authorization response. " + reqQueryString);
                return;
            }

            // extracts the code
            var code = reqQueryString.Get("code");
            var incoming_state = reqQueryString.Get("state");

            // Compares the receieved state to the expected value, to ensure that
            // this app made the request which resulted in authorization.
            if (incoming_state != state)
            {
                Console.WriteLine("Received request with invalid state ({0})", incoming_state);
                return;
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
        }

        /// <summary>
        /// Returns URI-safe data with a given input length.
        /// </summary>
        /// <param name="length">Input length (nb. output will be longer)</param>
        /// <returns></returns>
        public static string RandomDataBase64Url(uint length)
        {
            var rng = new RNGCryptoServiceProvider();
            var bytes = new byte[length];
            rng.GetBytes(bytes);
            return Base64UrlEncoder.Encode(bytes);
        }
    }
}

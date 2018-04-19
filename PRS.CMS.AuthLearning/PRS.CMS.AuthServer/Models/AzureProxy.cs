using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using IdentityModel.Client;

// ReSharper disable once CheckNamespace
namespace PRS.CMS.IdentityModel
{
    public class AzureProxy
    {
        [CLSCompliant(false)]
        public AzureProxy(DiscoveryResponse discoResponse, string clientId)
        {
            if (discoResponse.IsError)
            {
                throw new ArgumentException(discoResponse.Error, nameof(discoResponse));
            }
            Discovery = discoResponse;
            ClientId = clientId;
        }

        [CLSCompliant(false)]
        public DiscoveryResponse Discovery { get; private set; }
        public string ClientId { get; private set; }
        public string ClientSecret { get; set; }

        public string Resource { get; set; } 

        public IEnumerable<string> AdditionalScopes { get; set; }

        [CLSCompliant(false)]
        public async Task<TokenResponse> RequestResourceOwnerPasswordAsync(string user, string password)
        {
            var address = Discovery.TokenEndpoint;
            var client = string.IsNullOrEmpty(ClientSecret)
                ? new TokenClient(address, ClientId)
                : new TokenClient(address, ClientId, ClientSecret, AuthenticationStyle.BasicAuthentication);

            // Not entirely sure why AzureAD makes you specify a resource - with other providers
            // this tends to be scope but those are usually acting as both token provider and token
            // consumer.  But this is what you have to do if you want to use the grant_type of "password".
            var extra = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(Resource))
            {
                extra.Add("resource", Resource);
            }

            var scope = "openid";
            if (AdditionalScopes != null && AdditionalScopes.Any())
            {
                scope += " " + string.Join(" ", AdditionalScopes);
            }


            var tokenResponse = await client
                .RequestResourceOwnerPasswordAsync(user, password, scope, extra)
                .ConfigureAwait(false);
            return tokenResponse;
        }

        [CLSCompliant(false)]
        public async Task<TokenResponse> RequestRefreshTokenAsync(TokenResponse token)
        {
            if (string.IsNullOrEmpty(token?.RefreshToken))
            {
                return null;
            }

            var address = Discovery.TokenEndpoint;
            var client = new TokenClient(address, ClientId);

            var tokenResponse = await client.RequestRefreshTokenAsync(token.RefreshToken).ConfigureAwait(false);
            return tokenResponse;
        }
    }
}
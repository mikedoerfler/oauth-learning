using System;
using System.Threading.Tasks;

using IdentityModel.Client;

// ReSharper disable once CheckNamespace
namespace PRS.CMS.IdentityModel
{
    public static class AzureDiscoveryResponse
    {
        [CLSCompliant(false)]
        public static async Task<DiscoveryResponse> GetAsync(string authority)
        {
            // ReSharper disable once UseObjectOrCollectionInitializer
            // code is clearer the way it is now than with nested {}
            var client = new DiscoveryClient(authority);

            // Azure AD uses an issuer with a different URL (Authority) as the 
            // issuer claim (iss) - sts.windows.net
            client.Policy.ValidateIssuerName = false;

            // these are some endpoints that don't follow the pattern where every endpoint should
            // start with the Authority
            client.Policy.AdditionalEndpointBaseAddresses.Add("https://login.microsoftonline.com/common/");
            client.Policy.AdditionalEndpointBaseAddresses.Add("https://sts.windows.net/");

            var discoResponse = await client.GetAsync().ConfigureAwait(false);
            return discoResponse;
        }
    }
}

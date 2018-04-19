using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

using IdentityModel.Client;

// ReSharper disable once CheckNamespace
namespace PRS.CMS.IdentityModel
{
    public static class TokenResponseExtensions
    {
        [CLSCompliant(false)]
        public static ClaimsPrincipal ToClaimsPrincipal(this TokenResponse tokenResponse, string authenticationType)
        {
            if (tokenResponse.IsError)
            {
                // add logging for what type of error was returned
                return null;
            }

            // don't want any of the mapping from JWT claims to SAML claims to happen
            var handler = new JwtSecurityTokenHandler();

            // not doing anything with AccessToken
            //var accessSecToken = handler.ReadJwtToken(tokenResponse.AccessToken);
            var identitySecToken = handler.ReadJwtToken(tokenResponse.IdentityToken);

            var claims = identitySecToken.Claims.ToList();

            // want to use 'upn' as the name Claim because that should be unique in our Azure AD/local domain
            var claimsIdentity = new ClaimsIdentity(claims, authenticationType, "upn", "role");
            var principal = new ClaimsPrincipal(claimsIdentity);

            return principal;

        }
    }
}
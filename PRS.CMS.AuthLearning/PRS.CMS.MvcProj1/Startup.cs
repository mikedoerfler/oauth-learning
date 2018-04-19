using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Web.Helpers;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;

[assembly: OwinStartup(typeof(PRS.CMS.MvcProj1.Startup))]

namespace PRS.CMS.MvcProj1
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AntiForgeryConfig.UniqueClaimTypeIdentifier = "sub";
            JwtSecurityTokenHandler.InboundClaimTypeMap = new Dictionary<string, string>();

            app.UseCookieAuthentication(new CookieAuthenticationOptions()
            {
                AuthenticationType = "Cookies"
            });

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions()
            {
                ClientId = "PRS.CMS.MvcProj1",
                //Authority = "https://localhost:44300/AuthServer/identity/",
                Authority = "http://localhost:6159/AuthServer/identity/",
                RedirectUri = "https://localhost:44301/",
                ResponseType = "code id_token token",
                Scope = "openid profile email roles offline_access",
                TokenValidationParameters = new TokenValidationParameters()
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                },
                SignInAsAuthenticationType = "Cookies"
            });
        }
    }
}
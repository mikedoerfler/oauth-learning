using Owin;
using IdentityServer.WindowsAuthentication.Configuration;
using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using Microsoft.Owin;
using Microsoft.Owin.Security.WsFederation;
using PRS.CMS.AuthLearning.WsFedServer.Models;

[assembly: OwinStartup(typeof(PRS.CMS.AuthLearning.WsFedServer.Startup))]

namespace PRS.CMS.AuthLearning.WsFedServer
{
    public class Startup
    {
        private static readonly string RootUrl = "https://localhost:44302/";

        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.Map("/windows", ConfigureWindowsTokenProvider);

            var factory = new IdentityServerServiceFactory()
                .UseInMemoryClients(Hardcoded.Clients())
                .UseInMemoryScopes(Hardcoded.Scopes());
            factory.UserService = new Registration<IUserService>(typeof(ExternalRegistrationUserService));

            var options = new IdentityServerOptions
            {
                SigningCertificate = Hardcoded.Cert(),
                Factory = factory,
                AuthenticationOptions = new AuthenticationOptions
                {
                    EnableLocalLogin = true,
                    IdentityProviders = ConfigureIdentityProviders
                },
            };

            appBuilder.UseIdentityServer(options);
        }

        private static void ConfigureWindowsTokenProvider(IAppBuilder app)
        {
            var options = new WindowsAuthenticationOptions
            {
                IdpRealm = "urn:win",
                SubjectType = SubjectType.Sid,
                IdpReplyUrl = RootUrl + "was",
                PublicOrigin = RootUrl,
                SigningCertificate = Hardcoded.Cert(),
                EnableOAuth2Endpoint = true
            };
            app.UseWindowsAuthenticationService(options);
        }

        private static void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var options = new WsFederationAuthenticationOptions
            {
                AuthenticationType = "windows",
                Caption = "Windows",
                SignInAsAuthenticationType = signInAsType,

                MetadataAddress = RootUrl + "windows",
                Wtrealm = "urn:idsrv3"
            };
            app.UseWsFederationAuthentication(options);
        }
    }
}
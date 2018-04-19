using System;
using System.Threading.Tasks;
using IdentityServer3.Core.Configuration;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using Owin;
using PRS.CMS.AuthServer.Models;
using Serilog;

namespace PRS.CMS.AuthServer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Trace()
                .CreateLogger();

            app.Map("/identity", x =>
            {
                var serviceFactory = new IdentityServerServiceFactory()
                    .UseInMemoryClients(Hardcoded.Clients())
                    .UseInMemoryScopes(Hardcoded.Scopes())
                    .UseInMemoryUsers(Hardcoded.Users());

                var options = new IdentityServerOptions
                {
                    SiteName = "CMSAuthServer IdentityServer",
                    // publicorigin would be important if behind a proxy  
                    //PublicOrigin = "",
                    SigningCertificate = Hardcoded.Cert(),
                    Factory = serviceFactory,
                    RequireSsl = false,
                    AuthenticationOptions = new AuthenticationOptions
                    {
                        IdentityProviders = ConfigureIdentityProviders
                    }
                };

                x.UseIdentityServer(options);
            });
        }

        private void ConfigureIdentityProviders(IAppBuilder app, string signInAsType)
        {
            var openIdOptions = new OpenIdConnectAuthenticationOptions
            {
                Caption = "Azure",
                SignInAsAuthenticationType = signInAsType,
                ClientId = "729cfc16-2b7e-47ab-8a6d-63e71cfa813f",
                //ClientSecret = "...",
                Authority = "https://login.microsoft.com/casemax.com",
                // this doesn't need to be set in code but it does need to be set in the
                // azure portal
                //RedirectUri = "http://localhost:6159/AuthServer/identity/",
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = n =>
                    {
                        return Task.FromResult(0);
                    },
                    AuthorizationCodeReceived = notification =>
                    {
                        return Task.FromResult(0);
                    },
                    MessageReceived = n =>
                    {
                        return Task.FromResult(0);
                    },
                    RedirectToIdentityProvider = notification =>
                    {
                        // The open id class can't deal with authorization uri which already contain '?'
                        // We need this work around to cover it in the request
                        var parts = notification.ProtocolMessage.IssuerAddress.Split(new[] {'?'});
                        notification.ProtocolMessage.IssuerAddress = parts[0];
                        if (parts.Length > 1)
                        {
                            //context.ProtocolMessage.Parameters.Add("p", Settings.Default.B2CPolicy);
                        }

                        return Task.FromResult(0);
                    },
                    SecurityTokenReceived = n =>
                    {
                        return Task.FromResult(0);
                    },
                    SecurityTokenValidated = notification =>
                    {
                        return Task.FromResult(0);
                    },
                }
            };
            app.UseOpenIdConnectAuthentication(openIdOptions);
        }
    }
}
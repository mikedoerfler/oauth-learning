using System.Collections.Generic;

using IdentityServer3.Core.Configuration;
using IdentityServer3.Core.Services;
using IdentityServer3.Core.Services.InMemory;
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
                    .UseInMemoryScopes(Hardcoded.Scopes());

                // this will require storing of username/password somewhere
                /*
                 * serviceFactory.UseInMemoryUsers(Hardcoded.Users());
                 */
                
                // this is to allow a new UserService to be created each time one is needed
                /*
                 * serviceFactory.Register(new Registration<List<InMemoryUser>>(Hardcoded.Users(), (string) null));
                 * serviceFactory.UserService = new Registration<IUserService, UserService>((string) null);
                 */

                // this will register one with no dependencies that gets created each time
                //serviceFactory.UserService = new Registration<IUserService>(typeof(UserService));

                // this will creat a single UserService to be shared at runtime - better make it thread safe
                serviceFactory.UserService = new Registration<IUserService>(new UserService());

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
                AuthenticationType = "azure",
                Caption = "Azure",
                SignInAsAuthenticationType = signInAsType,
                ClientId = "9f62507c-99d8-45e3-bb34-e01b7cbd199f",
                // this uses standard oauth/openid dance so it doesn't require the secret
                //ClientSecret = "nWdHjtz7AhGjOpIew36mC/2OeL3aBtfCxEsQjfu3lIs=",
                Authority = "https://login.microsoft.com/casemax.com",
                // this doesn't need to be set in code but it does need to be set in the
                // azure portal
                //RedirectUri = "http://localhost:6159/AuthServer/identity/",
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    AuthenticationFailed = async n =>
                    {
                        var msg = n.ProtocolMessage;
                    },
                    AuthorizationCodeReceived = async n =>
                    {
                        var msg = n.ProtocolMessage;
                    },
                    MessageReceived = async n =>
                    {
                        var msg = n.ProtocolMessage;
                    },
                    RedirectToIdentityProvider = async notification =>
                    {
                        // The open id class can't deal with authorization uri which already contain '?'
                        // We need this work around to cover it in the request
                        var parts = notification.ProtocolMessage.IssuerAddress.Split(new[] {'?'});
                        notification.ProtocolMessage.IssuerAddress = parts[0];
                        if (parts.Length > 1)
                        {
                            //context.ProtocolMessage.Parameters.Add("p", Settings.Default.B2CPolicy);
                        }
                    },
                    SecurityTokenReceived = async n =>
                    {
                        var msg = n.ProtocolMessage;
                    },
                    SecurityTokenValidated = async n =>
                    {
                        var msg = n.ProtocolMessage;
                    },
                }
            };
            app.UseOpenIdConnectAuthentication(openIdOptions);
        }
    }
}
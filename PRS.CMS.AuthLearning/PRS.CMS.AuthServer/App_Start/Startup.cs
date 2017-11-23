using IdentityServer3.Core.Configuration;
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
                    SiteName = "CMSAuthServer Security Token Server",
                    IssuerUri = "https://casemaxsolutions.com/sts/",
                    // publicorigin would be important if behind a proxy  
                    //PublicOrigin = "",
                    SigningCertificate = Hardcoded.Cert(),
                    Factory = serviceFactory,
                    RequireSsl = false,
                };

                x.UseIdentityServer(options);
            });
        }
    }
}
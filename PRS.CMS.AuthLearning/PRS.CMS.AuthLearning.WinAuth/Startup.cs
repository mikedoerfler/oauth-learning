using System.IO;
using System.Security.Cryptography.X509Certificates;
using IdentityServer.WindowsAuthentication.Configuration;
using Microsoft.Owin;
using Owin;
using PRS.CMS.AuthLearning.WinAuth;

[assembly: OwinStartup(typeof(Startup))]

namespace PRS.CMS.AuthLearning.WinAuth
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var options = new WindowsAuthenticationOptions
            {
                EnableOAuth2Endpoint = true,
                EnableWsFederationEndpoint = true,
                IdpRealm = "urn:idp",
                IdpReplyUrl = "http://localhost:26712/core/was",
                SigningCertificate = Cert(),
            };
            app.UseWindowsAuthenticationService(options);
        }


        public static X509Certificate2 Cert()
        {
            var assembly = typeof (Startup).Assembly;
            using (var stream = assembly.GetManifestResourceStream("PRS.CMS.AuthLearning.WinAuth.Configuration.idsrv3test.pfx"))
            {
                var buffer = new byte[16*1024];
                using (var ms = new MemoryStream())
                {
                    int read;
                    while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                    }
                    var data = ms.ToArray();
                    return new X509Certificate2(data, "idsrv3test");
                }
            }
        }
    }
}
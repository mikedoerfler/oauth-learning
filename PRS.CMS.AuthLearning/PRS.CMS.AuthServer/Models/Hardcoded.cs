using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.InMemory;

namespace PRS.CMS.AuthServer.Models
{
    public static class Hardcoded
    {
        public static IEnumerable<Client> Clients()
        {
            var mvcWebAppClient = new Client
            {
                Enabled = true,
                ClientName = "PRS.CMS.MvcProj1 Web App",
                ClientId = "PRS.CMS.MvcProj1",
                Flow = Flows.Implicit,
                RedirectUris = new List<string>
                {
                    "https://localhost:44300/MvcProj1"
                }
            };

            var fatClient = new Client
            {
                Enabled = true,
                Flow = Flows.ResourceOwner,
                ClientName = "SimpleClientApp",
                ClientId = "SimpleClientApp",
                ClientSecrets = new List<Secret>
                {
                    new Secret("secret".Sha256())
                },
                AllowAccessToAllScopes = true,
                AccessTokenType = AccessTokenType.Jwt,
                AccessTokenLifetime = 3600,

                IncludeJwtId = true,
                IdentityTokenLifetime = 3600

            };

            return new[] {mvcWebAppClient, fatClient};
        }

        public static IEnumerable<Scope> Scopes()
        {
            /*return new [] {
                new Scope(){
                    
                }
            }*/
            return new[]
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                StandardScopes.Roles,
                StandardScopes.OfflineAccess,
            };
        } 

        public static List<InMemoryUser> Users()
        {
            var u1 = new InMemoryUser
            {
                Username = "bob",
                Password = "123",
                Subject = "InMemoryUser#1",
                Claims = new[]
                {
                    new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                    new Claim(Constants.ClaimTypes.Email, "bobsmith@aol.com"),
                    new Claim(Constants.ClaimTypes.Role, "BadAssAdmin")
                }
            };

            return new List<InMemoryUser> {u1};
        }

        public static X509Certificate2 Cert()
        {
            var assembly = typeof (Hardcoded).Assembly;
            using (var stream = assembly.GetManifestResourceStream("PRS.CMS.AuthServer.Configuration.idsrv3test.pfx"))
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
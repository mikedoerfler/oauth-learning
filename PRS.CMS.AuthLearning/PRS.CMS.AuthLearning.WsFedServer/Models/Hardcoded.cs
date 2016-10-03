using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using IdentityServer3.Core;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.InMemory;

namespace PRS.CMS.AuthLearning.WsFedServer.Models
{
    public static class Hardcoded
    {
        public static IEnumerable<Client> Clients()
        {
            var fatClient = new Client
            {
                ClientName = "SimpleClientApp",
                Enabled = true,

                Flow = Flows.Custom,

                ClientId = "SimpleClientApp",
                ClientSecrets = new List<Secret>
                {
                    new Secret("secret".Sha256())
                },
                AllowedCustomGrantTypes = new List<string>
                {
                    "custom"
                },
                AllowedScopes = new List<string>
                {
                    "read",
                    "write"
                },
                IncludeJwtId = true
            };

            return new[] {fatClient};
        }

        public static IEnumerable<Scope> Scopes()
        {
            return new[]
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                StandardScopes.Roles,
                StandardScopes.OfflineAccess
            };
        } 

        public static List<InMemoryUser> Users()
        {
            var u1 = new InMemoryUser
            {
                Username = "bob",
                Password = "123",
                Subject = "1",
                Claims = new[]
                {
                    new Claim(Constants.ClaimTypes.GivenName, "Bob"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Smith"),
                    new Claim(Constants.ClaimTypes.Email, "bobsmith@aol.com"),
                    new Claim(Constants.ClaimTypes.Role, "BadAssAdmin")
                }
            };

            var u2 = new InMemoryUser
            {
                Username = "casemax\\mike.doerfler",
                Password = "123",
                Subject = "S-1-5-21-226636460-1763764126-764655366-1107",
                Claims = new[]
                {
                    new Claim(Constants.ClaimTypes.GivenName, "Mike"),
                    new Claim(Constants.ClaimTypes.FamilyName, "Doerfler"),
                    new Claim(Constants.ClaimTypes.Email, "mike.doerfler@casemax.com"),
                    new Claim(Constants.ClaimTypes.Role, "KickAssDev")
                }
            };
            return new List<InMemoryUser> {u1, u2};
        }

        public static X509Certificate2 Cert()
        {
            var assembly = typeof (Hardcoded).Assembly;
            using (var stream = assembly.GetManifestResourceStream("PRS.CMS.AuthLearning.WsFedServer.Configuration.idsrv3test.pfx"))
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
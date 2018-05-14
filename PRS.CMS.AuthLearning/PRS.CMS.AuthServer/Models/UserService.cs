using System.Collections.Generic;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.Default;
using IdentityServer3.Core.Services.InMemory;
using PRS.CMS.IdentityModel;

namespace PRS.CMS.AuthServer.Models
{
    public class UserService : UserServiceBase
    {
        private readonly List<InMemoryUser> _users;

        public UserService() : this(new List<InMemoryUser>())
        {
        }

        public UserService(List<InMemoryUser> users)
        {
            _users = users;
        }

        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            // this is the authentication done when a username/password is supplied - that could be through a resource owner
            // connection or by typing in the displayed UI

            var discoResponse = await AzureDiscoveryResponse.GetAsync("https://login.microsoftonline.com/b5af619b-e5a7-4290-813b-fd2ea1ede0bf");
            var proxy = new AzureProxy(discoResponse, "9f62507c-99d8-45e3-bb34-e01b7cbd199f")
            {
                Resource = AzureResources.AzureGraph,
                ClientSecret = "nWdHjtz7AhGjOpIew36mC/2OeL3aBtfCxEsQjfu3lIs="
            };
            var tokenResponse = await proxy.RequestResourceOwnerPasswordAsync(context.UserName, context.Password);

            if (tokenResponse.IsError)
            {
                context.AuthenticateResult = new AuthenticateResult(tokenResponse.Error);
            }
            else
            {
                var principal = tokenResponse.ToClaimsPrincipal(Constants.BuiltInIdentityProvider);

                var subjectId = principal.GetSubjectId();
                var username = principal.GetName();

                var user = _users.FirstOrDefault(u => u.Subject == subjectId);
                if (user == null)
                {
                    user = new InMemoryUser
                    {
                        Subject = subjectId,
                        Username = username,
                        Enabled = true
                    };
                    _users.Add(user);
                }

                user.Claims = principal.Claims;
                context.AuthenticateResult = new AuthenticateResult(user.Subject, user.Username, user.Claims, "idsrv", "pwd");

            }
        }

        public override Task IsActiveAsync(IsActiveContext context)
        {
            // this would need to check if the Subject represented an active user
            context.IsActive = true;
            return Task.FromResult(0);
        }

        public override Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = _users.Single(u => u.Subject == context.Subject.GetSubjectId());
            var claims = user.Claims.ToList();

            if (claims.FirstOrDefault(c=>c.Type == "sub") == null)
            {
                var subClaim = new Claim("sub", user.Subject);
                claims.Add(subClaim);
            };

            context.IssuedClaims = claims;
            return Task.FromResult(0);
        }

        public override async Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            // checking to see if they will go straight to Azure by doing this
            // this has to match with the Options.AuthenticationType
            context.SignInMessage.IdP = "azure";

            await base.PreAuthenticateAsync(context);
            var msg2 = context.SignInMessage;
        }

        public override async Task PostAuthenticateAsync(PostAuthenticationContext context)
        {
            var msg = context.SignInMessage;
            await base.PostAuthenticateAsync(context);
            var msg2 = context.SignInMessage;
        }

        public override Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            // this is the method called after an external IdP has been used to get how that user
            // is represented on that system
            var identity = context.ExternalIdentity;

            var result = context.AuthenticateResult;

            return Task.FromResult(0);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer3.Core;
using IdentityServer3.Core.Extensions;
using IdentityServer3.Core.Models;
using IdentityServer3.Core.Services.InMemory;
using PRS.CMS.IdentityModel;

namespace PRS.CMS.AuthServer.Models
{
    public class UserService : InMemoryUserService
    {
        public UserService() : base(new List<InMemoryUser>())
        {
        }

        public UserService(List<InMemoryUser> users) : base(users)
        {
        }

        public override async Task AuthenticateLocalAsync(LocalAuthenticationContext context)
        {
            // this is the authentication done when a username/password is supplied
            //await base.AuthenticateLocalAsync(context);
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
                context.AuthenticateResult = new AuthenticateResult(principal.GetSubjectId(), principal.GetName(),
                    principal.Claims, "idsrv", "pwd");
            }
        }

        public override async Task IsActiveAsync(IsActiveContext context)
        {
            await base.IsActiveAsync(context);
            // this would need to check if the Subject represented an active user
            context.IsActive = true;
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            await base.GetProfileDataAsync(context);
            var claims = context.AllClaimsRequested;
        }

        protected override string GetDisplayName(InMemoryUser user)
        {
            var name = base.GetDisplayName(user);
            return name;
        }

        public override async Task PreAuthenticateAsync(PreAuthenticationContext context)
        {
            var msg = context.SignInMessage;
            await base.PreAuthenticateAsync(context);
            var msg2 = context.SignInMessage;
        }

        public override async Task PostAuthenticateAsync(PostAuthenticationContext context)
        {
            var msg = context.SignInMessage;
            await base.PostAuthenticateAsync(context);
            var msg2 = context.SignInMessage;
        }

        public override async Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
        {
            var msg = context.SignInMessage;
            await base.AuthenticateExternalAsync(context);

            var result = context.AuthenticateResult;
            var msg2 = context.SignInMessage;
        }
    }
}
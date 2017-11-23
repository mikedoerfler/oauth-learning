using System;

namespace SimpleClientApp
{
    public static class OpenIdConstants
    {
        public static string ClientId = "SimpleClientApp-Browser";
        public static string ClientSecret = "secret";

        public static Uri RootUri => new Uri("http://localhost:6159/AuthServer/identity/connect");
        public static Uri AuthorizeEndpoint => new Uri(RootUri + "/authorize");
        public static Uri TokenEndpoint => new Uri(RootUri + "/token");
        public static Uri UserInfoEndpoint => new Uri(RootUri + "/userinfo");


    }
}
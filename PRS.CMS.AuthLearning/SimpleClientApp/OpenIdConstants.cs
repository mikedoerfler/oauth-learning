using System;

namespace SimpleClientApp
{
    public static class OpenIdConstants
    {
        public static string ClientId = "SimpleClientApp-Browser";
        public static string Client2Id = "SimpleClientApp";
        public static string ClientSecret = "secret";

        public static Uri Authority => new Uri("http://localhost:6159/AuthServer/identity");
        public static Uri AuthorizeEndpoint => new Uri(Authority + "/connect/authorize");
        public static Uri TokenEndpoint => new Uri(Authority + "/connect/token");
        public static Uri UserInfoEndpoint => new Uri(Authority + "/connect/userinfo");
    }
}
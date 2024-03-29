﻿using System;
using System.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

using IdentityModel.Tokens;

namespace PRS.CMS.AuthLearning.CustomSTS.Controllers
{
    public class BasicAuthController : Controller
    {
        // GET: BasicAuth
        public ActionResult Get(string user, string password, string format = "")
        {
            var now = DateTime.UtcNow;
            var key = new byte[32];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(key);
            }

            var securityKey = Encoding.UTF8.GetBytes("notsecureatallyouidiot");

            var issuer = "https://casemaxsolutions.com/customsts";
            var audience = "http://www.example.com";
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user),  
                new Claim("sub", user),  
                new Claim("password", password),
                new Claim("secretSecurityKey", Base64UrlEncoder.Encode(key))
            };
            var notBefore = now;
            var expires = now.AddMinutes(2);

            var signingCredentials = new HmacSigningCredentials(key);

            // simple example to see what building my own JWT looks like just in case
            // i didn't want to go the full path of using identityserver because I 
            // don't need a full blow oauth/openid server.
            var token = new JwtSecurityToken(issuer, audience, claims, notBefore, expires, signingCredentials);

            if (format == "json")
            {
                return Json(token, JsonRequestBehavior.AllowGet);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwt = tokenHandler.WriteToken(token);
            return Content(jwt);
        }
    }
}
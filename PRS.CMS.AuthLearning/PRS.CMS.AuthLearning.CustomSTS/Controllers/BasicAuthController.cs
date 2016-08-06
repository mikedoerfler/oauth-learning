using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PRS.CMS.AuthLearning.CustomSTS.Controllers
{
    public class BasicAuthController : Controller
    {
        // GET: BasicAuth
        public ActionResult Get(string user, string password, string format = "")
        {
            // simple example to see what building my own JWT looks like just in case
            // i didn't want to go the full path of using identityserver because I 
            // don't need a full blow oauth/openid server.
            var token = new JwtSecurityToken(
                issuer: "https://casemaxsolutions.com/customsts",
                audience: ""
            );

            if (format == "json")
            {
                return Json(token, JsonRequestBehavior.AllowGet);
            }

            var tokenText = new JwtSecurityTokenHandler().WriteToken(token);
            return Content(tokenText);
        }
    }
}
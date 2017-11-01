using System.Web.Mvc;

namespace PRS.CMS.MvcProj1.Controllers
{
    public sealed class AccountController : Controller
    {
        [Authorize]
        public ActionResult SignIn()
        {
            return Redirect("/");
        }
    }
}
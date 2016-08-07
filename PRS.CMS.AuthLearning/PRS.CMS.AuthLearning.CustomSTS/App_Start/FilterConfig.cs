using System.Web.Mvc;

namespace PRS.CMS.AuthLearning.CustomSTS
{
    public static class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}

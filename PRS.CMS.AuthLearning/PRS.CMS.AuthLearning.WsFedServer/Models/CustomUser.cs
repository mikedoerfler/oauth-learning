using System.Collections.Generic;
using System.Security.Claims;

namespace PRS.CMS.AuthLearning.WsFedServer.Models
{
    public class CustomUser
    {
        public string Subject { get; set; }
        public string Provider { get; set; }
        public string ProviderID { get; set; }
        public List<Claim> Claims { get; set; }
    }
}
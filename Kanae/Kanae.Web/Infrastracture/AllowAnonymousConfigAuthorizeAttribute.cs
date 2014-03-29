using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kanae.Web.Infrastracture
{
    public class AllowAnonymousConfigAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var allowAnoymousViewing = ConfigurationManager.AppSettings["Kanae:AllowAnonymousViewing"] ?? "true";
            if ((String.Compare(allowAnoymousViewing, "true", StringComparison.OrdinalIgnoreCase) == 0))
            {
                return true;
            }
            return base.AuthorizeCore(httpContext);
        }
    }
}
using Kanae.Handler.Maintenance;
using Kanae.Web.Infrastracture;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Kanae.Web.Controllers
{
    public class MaintenanceController : BaseController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // 管理キーが設定されていてかつ一致していないとメンテナンス管理アクションは利用できない
            var configuredKey = ConfigurationManager.AppSettings["Kanae:MaintenanceKey"];
            if (
                String.IsNullOrWhiteSpace(configuredKey) ||
                (filterContext.ActionParameters.ContainsKey("MaintenanceKey") && (filterContext.ActionParameters["MaintenanceKey"] as String) != configuredKey)
            )
            {
                filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            base.OnActionExecuting(filterContext);
        }

        //
        // GET: /Maintenance/
        public async Task<ActionResult> CleanUp(String MaintenanceKey)
        {
            await Using<CleanUpMedia>().Execute(ApplicationConfig.Current.RetentionTimeLimit);

            return Content("OK");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Kanae.Web.Controllers
{
    public class ErrorController : Controller
    {
        //
        // GET: /Error/NotFound
        public ActionResult NotFound()
        {
            return View();
        }

        //
        // GET: /Error/Forbidden
        public ActionResult Forbidden()
        {
            return View();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SenkaKichi.Controllers
{
    public class ErrorController : Controller
    {
        // GET: Error
        public ActionResult Index(int statusCode = 500)
        {
            if (ViewEngines.Engines.FindView(ControllerContext, statusCode.ToString(), null).View == null) {
                statusCode = 500;
            }
            Response.StatusCode = statusCode;
            Response.TrySkipIisCustomErrors = true;
            return View(statusCode.ToString());
        }
    }
}
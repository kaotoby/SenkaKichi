using log4net;
using SenkaKichi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SenkaKichi.Filters
{
    public class LogErrorAttribute : HandleErrorAttribute
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LogErrorAttribute).FullName);

        public override void OnException(ExceptionContext filterContext) {
            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled) {
                return;
            }

            if (new HttpException(null, filterContext.Exception).GetHttpCode() != 500) {
                return;
            }

            if (!ExceptionType.IsInstanceOfType(filterContext.Exception)) {
                return;
            }

            // if the request is AJAX return JSON else view.
            if (filterContext.HttpContext.Request.RequestContext.RouteData.Values["area"].ToString() == "Api") {
                filterContext.Result = new AjaxResult<string>(false, filterContext.Exception.Message);
            } else {
                filterContext.Result = new RedirectResult("/error/500");
            }

            // log the error using log4net.
            _logger.Error(string.Format("{0}\r\nat {1}", filterContext.Exception.Message, filterContext.HttpContext.Request.Url),
                filterContext.Exception);
            filterContext.ExceptionHandled = true;
            filterContext.HttpContext.Response.Clear();
            filterContext.HttpContext.Response.StatusCode = 500;

            filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
        }
    }
}
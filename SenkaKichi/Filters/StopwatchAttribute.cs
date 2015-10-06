using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SenkaKichi.Filters
{
    public class StopwatchAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext) {
            if (filterContext.HttpContext.Response.ContentType == "text/html" && filterContext.HttpContext.Response.Output is HttpWriter) {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                filterContext.HttpContext.Items["Stopwatch"] = stopwatch;
                filterContext.HttpContext.Items["UseStopwatch"] = true;
            } else {
                filterContext.HttpContext.Items["UseStopwatch"] = false;
            }
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext) {
            if ((bool)filterContext.HttpContext.Items["UseStopwatch"]) {
                Stopwatch stopWatch = (Stopwatch)filterContext.HttpContext.Items["Stopwatch"];
                int et = (int)stopWatch.Elapsed.TotalMilliseconds;

                filterContext.Controller.ViewBag.ElapsedTime = et.ToString();
                stopWatch.Stop();
            }
        }
    }
}
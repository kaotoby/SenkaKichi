using Owin;
using SenkaKichi.DbModels;
using SenkaKichi.Models;
using SenkaKichi.OAuthApi.Twitter;
using System.Configuration;
using System.Reflection;
using System.Security.Claims;
using System.Web;
using System.Web.Caching;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Linq;

namespace SenkaKichi
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new RazorViewEngine());

            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.NameIdentifier;

            SenkaRepository.Startup();
        }

        public override string GetVaryByCustomString(HttpContext context, string custom) {
            string str = base.GetVaryByCustomString(context, custom);
            var varys = custom.Split(';');
            if (varys.Contains("IsMobile") && context.Request.Browser.IsMobileDevice) {
                str += "IsMobile";
            }
            if (varys.Contains("Ajax") && context.Request.Headers["X-Requested-With"] == "XMLHttpRequest") {
                str += "Ajax";
            }
            return str;
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app) {
            app.CreatePerOwinContext(SenkaContext.Create);
            app.CreatePerOwinContext<TwitterApiManager>(TwitterApiManager.Create);
            app.CreatePerOwinContext<SenkaRepository>(SenkaRepository.Create);
            IdentityConfig.ConfigureAuth(app);
        }
    }
}

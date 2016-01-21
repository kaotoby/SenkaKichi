using System.Web.Mvc;

namespace SenkaKichi.Areas.Api
{
    public class ApiAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Api";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) {
            context.MapRoute(
                name: "Api_default",
                url: "Api/{controller}/{action}/{id}",
                defaults: new { action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "SenkaKichi.Areas.Api.Controllers" }
            );
        }
    }
}
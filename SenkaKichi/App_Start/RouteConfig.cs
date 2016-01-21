using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SenkaKichi
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes) {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.LowercaseUrls = true;

            routes.MapRoute(
                name: "ServerRanking",
                url: "Server/Ranking",
                defaults: new { controller = "Server", action = "Ranking", id = 0 },
                namespaces: new[] { "SenkaKichi.Controllers" }
            );

            routes.MapRoute(
                name: "PlayerSearch",
                url: "Player/{action}",
                defaults: new { controller = "Player", action = "Search" },
                constraints: new { action = "(Search|Suggest)" }
            );

            routes.MapRoute(
                name: "Error",
                url: "Error/{statusCode}",
                defaults: new { controller = "Error", action = "Index" }
            );

            routes.MapRoute(
                name: "Primary",
                url: "{controller}/{id}/{action}",
                defaults: new { action = "Info", id = 0 },
                constraints: new { controller = "(Server|Player)" },
                namespaces: new[] { "SenkaKichi.Controllers" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

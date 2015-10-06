using System.Web;
using System.Web.Optimization;

namespace SenkaKichi
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles) {
            Styles.DefaultTagFormat = @"<link rel=""stylesheet"" href=""{0}"">";
#if !DEBUG
            BundleTable.EnableOptimizations = true;
#endif
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/highcharts").Include(
                        "~/Scripts/highcharts.js",
                        "~/Scripts/highcharts.exporting.js",
                        "~/Scripts/highcharts.lang.ja.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate.js",
                        "~/Scripts/jquery.validate.unobtrusive.js"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js",
                      "~/Scripts/respond.matchmedia.addListener.js"));

            bundles.Add(new ScriptBundle("~/bundles/viewmodel").Include(
                      "~/Scripts/ViewModels/viewmodel.js"));

            bundles.Add(new ScriptBundle("~/bundles/search").Include(
                     "~/Scripts/typeahead.js",
                     "~/Scripts/ViewModels/search.js"));

            bundles.Add(new ScriptBundle("~/bundles/models/player").Include(
                      "~/Scripts/ViewModels/Player/*.js"));

            //bundles.Add(new ScriptBundle("~/bundles/models/server").Include(
            //          "~/Scripts/ViewModels/Server/*.js"));

            bundles.Add(new StyleBundle("~/styles/bootstrap").Include(
                      "~/Content/Bootstrap.css"));

            bundles.Add(new StyleBundle("~/styles/site").Include(
                      "~/Content/Base.css",
                      "~/Content/Site.css"));
        }
    }
}

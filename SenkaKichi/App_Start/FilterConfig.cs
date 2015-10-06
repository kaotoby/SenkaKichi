using System.Web;
using System.Web.Mvc;
using SenkaKichi.Filters;

namespace SenkaKichi
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters) {
            filters.Add(new LogErrorAttribute());
            filters.Add(new MutiLanguageAttribute());
            filters.Add(new StopwatchAttribute());
        }
    }
}

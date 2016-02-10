using DevTrends.MvcDonutCaching;
using Microsoft.AspNet.Identity.Owin;
using SenkaKichi.DbModels;
using SenkaKichi.Models;
using SenkaKichi.ViewModels.Home;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SenkaKichi.Controllers
{
    public class HomeController : ControllerBase
    {
        [DonutOutputCache(Duration = 1200)]
        public async Task<ActionResult> Index() {
            if (User.Identity.IsAuthenticated && Session["User"] != null) {
                var user = Session["User"] as AspNetUser;
                if (DateTime.Now > user.TwitterInfo.ProfileUpdateTime.AddHours(1)) {
                    await userManager.UpdateTwitterProfileAsync(user.Id);
                }
            }
            var date = await repository.GetAllServerLastUpdatedAsync();
            var model = new IndexViewModel {
                DateInfo = date,
                RankPointDeltaRanking = await repository.GetAllServerDeltaRankingAsync(date, 0, 10),
                RankPointRanking = await repository.GetAllServerRankingAsync(date, 0, 10)
            };
            return View(model);
        }

        [ChildActionOnly]
        public ActionResult _ServerStatePartial() {
            return PartialView();
        }
    }
}
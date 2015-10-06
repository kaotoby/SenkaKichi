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
    public class HomeController : Controller
    {
        //[DonutOutputCacheAttribute(Duration = 120)]
        public async Task<ActionResult> Index() {
            if (User.Identity.IsAuthenticated && Session["User"] != null) {
                var user = Session["User"] as AspNetUser;
                if (DateTime.Now > user.TwitterInfo.ProfileUpdateTime.AddHours(1)) {
                    await UserManager.UpdateTwitterProfileAsync(user.Id);
                }
            }
            var date = await Repository.GetAllServerLastUpdatedAsync();
            var model = new IndexViewModel {
                DateInfo = date,
                RankPointDeltaRanking = await Repository.GetAllServerDeltaRankingAsync(date, 0, 10),
                RankPointRanking = await Repository.GetAllServerRankingAsync(date, 0, 10)
            };
            return View(model);
        }

        public ActionResult About() {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        [ChildActionOnly]
        public ActionResult _ServerStatePartial() {
            return PartialView();
        }

        #region Declare
        private SenkaRepository _repository;

        public SenkaRepository Repository {
            get {
                return _repository ?? HttpContext.GetOwinContext().Get<SenkaRepository>();
            }
            private set {
                _repository = value;
            }
        }

        public HomeController() {
        }

        public HomeController(SenkaRepository repository) {
            Repository = repository;
        }

        private ApplicationUserManager _userManager;

        public ApplicationUserManager UserManager {
            get {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set {
                _userManager = value;
            }
        }

        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (_userManager != null) {
                    _userManager.Dispose();
                    _userManager = null;
                }
            }

            base.Dispose(disposing);
        }
        #endregion
    }
}
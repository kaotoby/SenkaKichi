using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using SenkaKichi.DbModels;
using SenkaKichi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SenkaKichi.Controllers
{
    public class SharedController : Controller
    {
        private ApplicationUserManager _userManager;

        public SharedController() {
        }

        public SharedController(ApplicationUserManager userManager) {
            UserManager = userManager;
        }

        public ApplicationUserManager UserManager {
            get {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set {
                _userManager = value;
            }
        }

        [ChildActionOnly]
        public ActionResult _LoginPartial() {
            AspNetUser user = null;

            if (Request.IsAuthenticated && Session["User"] == null) {
                try {
                    user = Task.Run(() => UserManager.FindByIdAsync(User.Identity.GetUserId<int>())).Result;
                    if (user == null) {
                        HttpContext.GetOwinContext().Authentication.SignOut();
                    } else {
                        Session["User"] = user;
                    }
                } catch (Exception) {
                    HttpContext.GetOwinContext().Authentication.SignOut();
                }
            } else if (!Request.IsAuthenticated && Session["User"] != null) {
                HttpContext.GetOwinContext().Authentication.SignOut();
                Session["User"] = null;
            }

            return PartialView(Session["User"]);
        }

        [ChildActionOnly]
        public ActionResult _AlertPartial() {
            return PartialView();
        }
    }
}
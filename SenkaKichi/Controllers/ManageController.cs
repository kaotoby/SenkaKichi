using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SenkaKichi.Models;
using SenkaKichi.ViewModels.Manage;

namespace SenkaKichi.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index() {
            ViewBag.StatusMessage = TempData["StatusMessage"];

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            return View(user);
        }

        //
        // POST: /Manage/UpdatePlayer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdatePlayer(int playerId) {
            if (ModelState.IsValid) {
                var result = await UserManager.SetPlayerAsync(User.Identity.GetUserId<int>(), playerId);
                AddErrors(result);
            }

            return View();
        }

        //
        // POST: /Manage/GetVerifyToken
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GetVerifyToken() {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            string token = user.PlayerVerifyToken;
            if (token == null && user.PlayerId != null) {
                token = await UserManager.GeneratePlayerVerifyTokenAsync(user);
            }
            if (token != null) {
                return new AjaxResult<string>(true, token);
            } else {
                return new AjaxResult<string>(false, "Error");
            }
        }

        //
        // POST: /Manage/VerifyPlayer
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPlayer() {
            var verifyResult = await UserManager.VerifyPlayerAsync(User.Identity.GetUserId<int>());
            if (verifyResult.Succeeded) {
                return new AjaxResult<string>(true);
            } else {
                return new AjaxResult<string>(true, verifyResult.Errors.First());
            }
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey) {
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId<int>(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded) {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
                if (user != null) {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
            } else {
            }
            return RedirectToAction("ManageLogins");
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins() {
            ViewBag.StatusMessage = TempData["StatusMessage"];
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId<int>());
            if (user == null) {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId<int>());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = true;
            return View(new ManageLoginsViewModel {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider) {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId<int>().ToString());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback() {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId<int>().ToString());
            if (loginInfo == null) {
                return RedirectToAction("ManageLogins");
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId<int>(), loginInfo);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins");
        }

        [ChildActionOnly]
        public ActionResult _AddPlayerPartial() {
            ViewBag.StatusMessage = TempData["StatusMessage"];

            return PartialView();
        }

        #region Declare
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController() {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager {
            get {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set {
                _signInManager = value;
            }
        }

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

                if (_signInManager != null) {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager {
            get {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result) {
            foreach (var error in result.Errors) {
                ModelState.AddModelError("", error);
            }
        }

        #endregion
    }
}
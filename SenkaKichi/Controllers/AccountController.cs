using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SenkaKichi.DbModels;
using SenkaKichi.Models;
using SenkaKichi.ViewModels;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections.Generic;
using SenkaKichi.OAuthApi.Twitter;
using System;

namespace SenkaKichi.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        #region Declare
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public AccountController() {
        }

        public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager) {
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
        #endregion

        //
        // POST: /Account/ExternalLogin
        [AllowAnonymous, HttpPost, ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl) {
            Session["dummy"] = "To make sure a SessionId cookie is created.";
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl) {
            TempData["ReturnUrl"] = returnUrl;
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null) {
                return RedirectToAction("Index", "Home");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            AspNetUser user;
            switch (result) {
                case SignInStatus.Success:
                    user = await UserManager.FindByLoginAsync(loginInfo.Login);
                    await UserManager.UpdateTwitterProfileAsync(user.Id);

                    return RedirectToLocal(returnUrl);
                case SignInStatus.Failure:
                    AddErrors(IdentityResult.Failed());

                    return View("LoginFailure");
                case SignInStatus.RequiresVerification:
                default:
                    // Create new user
                    user = new AspNetUser();
                    var managerResult = await UserManager.CreateAsync(user, loginInfo);
                    if (managerResult.Succeeded) {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    } else {
                        throw new HttpException(managerResult.Errors.First());
                    }

                    string antiForgeryToken = Guid.NewGuid().ToString("N");
                    TempData["AntiForgeryToken"] = antiForgeryToken;

                    return RedirectToAction("LoginSucceed", new { token = antiForgeryToken });
            }
        }

        //
        // GET: /Account/LoginSucceed
        public ActionResult LoginSucceed(string token) {
            if ((string)TempData["AntiForgeryToken"] != token) {
                return RedirectToAction("Index", "Home");
            } else {
                return View();
            }
        }

        //
        // POST: /Account/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult Logout() {
            AuthenticationManager.SignOut();
            string antiForgeryToken = Guid.NewGuid().ToString("N");
            TempData["AntiForgeryToken"] = antiForgeryToken;
            Session["User"] = null;

            return RedirectToAction("LogoutSucceed", new { token = antiForgeryToken });
        }

        //
        // GET: /Account/LogoutSucceed
        [AllowAnonymous]
        public ActionResult LogoutSucceed(string token) {
            if ((string)TempData["AntiForgeryToken"] != token) {
                return RedirectToAction("Index", "Home");
            } else {
                return View();
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

        private ActionResult RedirectToLocal(string returnUrl) {
            if (Url.IsLocalUrl(returnUrl)) {
                return Redirect(returnUrl);
            }
            RouteData.Values.Clear();
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null) {
            }

            public ChallengeResult(string provider, string redirectUri, string userId) {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context) {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null) {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}
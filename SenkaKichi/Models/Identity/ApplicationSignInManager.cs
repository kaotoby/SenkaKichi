using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using SenkaKichi.DbModels;
using System;
using System.Linq;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace SenkaKichi.Models
{
    /// <summary>
    /// Manages Sign In operations for users
    /// </summary>
    public class ApplicationSignInManager : IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="authenticationManager"></param>
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager) {
            if (userManager == null) {
                throw new ArgumentNullException("userManager");
            }
            if (authenticationManager == null) {
                throw new ArgumentNullException("authenticationManager");
            }
            UserManager = userManager;
            AuthenticationManager = authenticationManager;
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context) {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }

        private string _authType;
        /// <summary>
        /// AuthenticationType that will be used by sign in, defaults to DefaultAuthenticationTypes.ApplicationCookie
        /// </summary>
        public string AuthenticationType {
            get { return _authType ?? DefaultAuthenticationTypes.ApplicationCookie; }
            set { _authType = value; }
        }

        /// <summary>
        /// Used to operate on users
        /// </summary>
        public ApplicationUserManager UserManager { get; set; }

        /// <summary>
        /// Used to sign in identities
        /// </summary>
        public IAuthenticationManager AuthenticationManager { get; set; }

        /// <summary>
        /// Called to generate the ClaimsIdentity for the user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public ClaimsIdentity CreateUserIdentity(AspNetUser user) {
            return UserManager.CreateIdentity(user, AuthenticationType);
        }

        /// <summary>
        /// Convert a int userId to a string, by default this just calls ToString()
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual string ConvertIdToString(int id) {
            return Convert.ToString(id, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Convert a string id to the proper int using Convert.ChangeType
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual int ConvertIdFromString(string id) {
            if (id == null) {
                return default(int);
            }
            return (int)Convert.ChangeType(id, typeof(int), CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Creates a user identity and then signs the identity using the AuthenticationManager
        /// </summary>
        /// <param name="user"></param>
        /// <param name="isPersistent"></param>
        /// <param name="rememberBrowser"></param>
        /// <returns></returns>
        public Task SignInAsync(AspNetUser user, bool isPersistent, bool rememberBrowser) {
            var userIdentity = CreateUserIdentity(user);
            // Clear any partial cookies from external or two factor partial sign ins
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie);
            if (rememberBrowser) {
                var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(ConvertIdToString(user.Id));
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity, rememberBrowserIdentity);
            } else {
                AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, userIdentity);
            }
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Sign the user in using an associated external login
        /// </summary>
        /// <param name="loginInfo"></param>
        /// <param name="isPersistent"></param>
        /// <returns></returns>
        public async Task<SignInStatus> ExternalSignInAsync(ExternalLoginInfo loginInfo, bool isPersistent) {
            var user = await UserManager.FindByLoginAsync(loginInfo.Login);
            if (user == null) {
                return SignInStatus.RequiresVerification;
            }

            var dblogin = user.UserLogins
                .FirstOrDefault(login => login.LoginProvider.Name == loginInfo.Login.LoginProvider);
            if (dblogin == null) {
                return SignInStatus.Failure;
            } else if (dblogin.AccessTokenSecret != loginInfo.ExternalIdentity.Claims.First(claim => claim.Type == "urn:accesstokensecret").Value) {
                await UserManager.UpdateAccessTokenAsync(dblogin, loginInfo);
            }
            return await SignInPrivate(user, isPersistent);
        }

        private async Task<SignInStatus> SignInPrivate(AspNetUser user, bool isPersistent) {
            var id = Convert.ToString(user.Id);
            await SignInAsync(user, isPersistent, false);
            return SignInStatus.Success;
        }

        /// <summary>
        ///     Dispose
        /// </summary>
        public void Dispose() { }
    }
}
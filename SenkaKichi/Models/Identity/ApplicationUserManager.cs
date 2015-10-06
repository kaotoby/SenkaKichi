using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using SenkaKichi.DbModels;
using SenkaKichi.OAuthApi.Twitter;
using SenkaKichi.ServiceReference;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SenkaKichi.Models
{
    /// <summary>
    ///     Exposes user related api which will automatically save changes to the UserStore
    /// </summary>
    public class ApplicationUserManager : IDisposable
    {
        private bool _disposed;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="store">The IUserStore is responsible for commiting changes via the UpdateAsync/CreateAsync methods</param>
        public ApplicationUserManager(ApplicationUserStore store, TwitterApiManager twitterManager) {
            if (store == null) {
                throw new ArgumentNullException("store");
            }
            Store = store;
            TwitterManager = twitterManager;
            UserValidator = new ApplicationUserValidator(this);
            WcfClient = new ServiceClient();
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context) {
            var manager = new ApplicationUserManager(new ApplicationUserStore(context.Get<SenkaContext>()), context.Get<TwitterApiManager>());
            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null) {
                manager.UserTokenProvider =
                    new ApplicationUserTokenProvider(dataProtectionProvider.Create("ASP.NET Identity"));
            }
            return manager;
        }

        /// <summary>
        ///     Persistence abstraction that the UserManager operates against
        /// </summary>
        protected internal ApplicationUserStore Store { get; set; }

        /// <summary>
        ///     Used to validate users before changes are saved
        /// </summary>
        public ApplicationUserValidator UserValidator { get; set; }

        /// <summary>
        ///     Used to interact with Twitter api
        /// </summary>
        public TwitterApiManager TwitterManager { get; set; }

        /// <summary>
        ///     Used to exchange data with WCF service
        /// </summary>
        public ServiceClient WcfClient { get; set; }

        /// <summary>
        ///     Used for generating reset password and confirmation tokens
        /// </summary>
        public ApplicationUserTokenProvider UserTokenProvider { get; set; }

        /// <summary>
        ///     Returns an IQueryable of users if the store is an IQueryableUserStore
        /// </summary>
        public virtual IQueryable<AspNetUser> Users {
            get {
                var queryableStore = Store as IQueryableUserStore<AspNetUser, int>;
                if (queryableStore == null) {
                    throw new NotSupportedException(Resources.StoreNotIQueryableUserStore);
                }
                return queryableStore.Users;
            }
        }

        /// <summary>
        ///     Creates a ClaimsIdentity representing the user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="authenticationType"></param>
        /// <returns></returns>
        public ClaimsIdentity CreateIdentity(AspNetUser user, string authenticationType) {
            ThrowIfDisposed();
            if (user == null) {
                throw new ArgumentNullException("user");
            }
            string RoleClaimType = ClaimsIdentity.DefaultRoleClaimType,
                   UserIdClaimType = ClaimTypes.NameIdentifier,
                   UserNameClaimType = ClaimsIdentity.DefaultNameClaimType,
                   IdentityProviderClaimType = "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
                   DefaultIdentityProviderClaimValue = "ASP.NET Identity";
            ClaimsIdentity id = new ClaimsIdentity(authenticationType, UserNameClaimType, RoleClaimType);
            id.AddClaim(new Claim(UserIdClaimType, user.Id.ToString(), ClaimValueTypes.String));
            id.AddClaim(new Claim(UserNameClaimType, user.UserName, ClaimValueTypes.String));
            id.AddClaim(new Claim(IdentityProviderClaimType, DefaultIdentityProviderClaimValue, ClaimValueTypes.String));

            return id;
        }

        /// <summary>
        ///     Create a user with no password
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<IdentityResult> CreateAsync(AspNetUser user, ExternalLoginInfo info) {
            ThrowIfDisposed();
            AspNetLoginProvider provider = await Store.GetLoginProviderAsync(info.Login.LoginProvider);
            if (provider == null) {
                return IdentityResult.Failed("Provider not supported.");
            }

            try {
                user.UserLogins.Add(new AspNetUserLogin {
                    UserId = user.Id,
                    LoginProvider = provider,
                    ProviderKey = long.Parse(info.Login.ProviderKey),
                    AccessToken = info.ExternalIdentity.Claims.First(claim => claim.Type == "urn:accesstoken").Value,
                    AccessTokenSecret = info.ExternalIdentity.Claims.First(claim => claim.Type == "urn:accesstokensecret").Value
                });
            } catch (Exception ex) {
                return IdentityResult.Failed(ex.Message);
            }

            var result = await UserValidator.ValidateAsync(user);
            if (!result.Succeeded) {
                return result;
            }

            await Store.CreateAsync(user);
            var twitterUser = await TwitterManager.GetUsersShowAsync(user.Id);
            await Store.UpdateTwitterProfileAsync(user.Id, twitterUser);
            return IdentityResult.Success;
        }

        /// <summary>
        ///     Update a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<IdentityResult> UpdateAsync(AspNetUser user) {
            ThrowIfDisposed();
            if (user == null) {
                throw new ArgumentNullException("user");
            }

            var result = await UserValidator.ValidateAsync(user);
            if (!result.Succeeded) {
                await Store.DiscardAsync();
                return result;
            }
            await Store.UpdateAsync(user);
            return IdentityResult.Success;
        }

        /// <summary>
        ///     Delete a user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<IdentityResult> DeleteAsync(AspNetUser user) {
            ThrowIfDisposed();
            await Store.DeleteAsync(user);
            return IdentityResult.Success;
        }

        /// <summary>
        ///     Find a user by id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<AspNetUser> FindByIdAsync(int userId) {
            ThrowIfDisposed();
            var user = await Store.FindByIdAsync(userId);
            if (user == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.UserIdNotFound,
                    userId));
            }
            return user;
        }

        /// <summary>
        ///     Find a user by user name
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Task<AspNetUser> FindByNameAsync(string userName) {
            ThrowIfDisposed();
            if (userName == null) {
                throw new ArgumentNullException("userName");
            }
            return Store.FindByNameAsync(userName);
        }

        /// <summary>
        ///     Returns the user associated with this login
        /// </summary>
        /// <returns></returns>
        public Task<AspNetUser> FindByLoginAsync(UserLoginInfo login) {
            ThrowIfDisposed();
            return Store.FindAsync(login);
        }

        /// <summary>
        ///     Remove a user login
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        public virtual async Task<IdentityResult> RemoveLoginAsync(int userId, UserLoginInfo login) {
            ThrowIfDisposed();
            var loginStore = Store;
            if (login == null) {
                throw new ArgumentNullException("login");
            }
            var user = await FindByIdAsync(userId);
            if (login.LoginProvider == "Twitter") {
                return IdentityResult.Failed("Can not remove Twitter.");
            }

            await loginStore.RemoveLoginAsync(user, login);
            return await UpdateAsync(user);
        }

        /// <summary>
        ///     Associate a login with a user
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task<IdentityResult> AddLoginAsync(int userId, ExternalLoginInfo info) {
            ThrowIfDisposed();
            if (info == null) {
                throw new ArgumentNullException("info");
            }

            var user = await FindByIdAsync(userId);

            await Store.AddLoginAsync(user, info);
            return await UpdateAsync(user);
        }

        /// <summary>
        ///     Update user login token
        /// </summary>
        /// <param name="user"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public Task UpdateAccessTokenAsync(AspNetUserLogin login, ExternalLoginInfo info) {
            ThrowIfDisposed();
            if (login == null) throw new ArgumentNullException("login");
            if (info == null) throw new ArgumentNullException("info");

            return Store.UpdateLoginAsync(login, info);
        }

        /// <summary>
        ///     Update user avata from twitter
        /// </summary>
        /// <param name="user"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public async Task UpdateTwitterProfileAsync(int userId) {
            var twitterUser = await TwitterManager.GetUsersShowAsync(userId);

            await Store.UpdateTwitterProfileAsync(userId, twitterUser);
        }

        /// <summary>
        ///     Gets the logins for a user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<IList<UserLoginInfo>> GetLoginsAsync(int userId) {
            ThrowIfDisposed();
            var loginStore = Store;
            var user = await FindByIdAsync(userId);

            return await loginStore.GetLoginsAsync(user);
        }

        /// <summary>
        ///     Get a user's email
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<Player> GetPlayerAsync(int userId) {
            ThrowIfDisposed();
            var store = Store;

            return await store.GetPlayerAsync(userId);
        }

        /// <summary>
        ///     Set a user's email
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<IdentityResult> SetPlayerAsync(int userId, int playerId) {
            ThrowIfDisposed();
            var store = Store;
            var user = await FindByIdAsync(userId);
            if (user == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.UserIdNotFound,
                    userId));
            }
            await store.SetPlayerAsync(user, playerId);
            await store.SetPlayerVerifiedAsync(user, false);
            return await UpdateAsync(user);
        }

        /// <summary>
        ///     Find a user by his player id
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<AspNetUser> FindByPlayerIdAsync(int playerId) {
            ThrowIfDisposed();
            var store = Store;
            return store.FindByPlayerIdAsync(playerId);
        }

        /// <summary>
        ///     Get the player verification token for the user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<string> GeneratePlayerVerifyTokenAsync(AspNetUser user) {
            ThrowIfDisposed();
            if (user == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.UserIdNotFound,
                    user));
            }

            string token;
            token = await GenerateUserTokenAsync("PlayerVerify", user.Id);
            token = token.Substring(0, 12);

            await Store.SavePlayerVerifyTokenAsync(user, token);
            return token;
        }

        public async Task<IdentityResult> VerifyPlayerAsync(int userId) {
            var user = await FindByIdAsync(userId);
            if (user == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.UserIdNotFound,
                    userId));
            }
            if (user.PlayerVerifyEndTime == null) {
                return IdentityResult.Failed("No token");

            } else if (user.PlayerVerifyEndTime < DateTime.Now) {
                TimeSpan span = user.PlayerVerifyEndTime.Value - DateTime.Now;
                return IdentityResult.Failed(string.Format("Lokout {0:F0} Minutes", span.TotalMinutes));
            } else {
                switch (await WcfClient.VerifyUserTokenAsync(userId)) {
                    case ServiceResult.Success:
                        return IdentityResult.Success;
                    case ServiceResult.Fail:
                        return IdentityResult.Failed("Wrong Token");
                    case ServiceResult.UnknowError:
                    default:
                        return IdentityResult.Failed("Unknow Error");
                }
            }
        }

        /// <summary>
        ///     Returns true if the user's player id has been confirmed
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> IsPlayerVerifiedAsync(int userId) {
            ThrowIfDisposed();
            var user = await FindByIdAsync(userId);
            if (user == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.UserIdNotFound,
                    userId));
            }
            return user.IsPlayerVerified;
        }

        /// <summary>
        ///     Get a user token for a specific purpose
        /// </summary>
        /// <param name="purpose"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<string> GenerateUserTokenAsync(string purpose, int userId) {
            ThrowIfDisposed();
            if (UserTokenProvider == null) {
                throw new NotSupportedException(Resources.NoTokenProvider);
            }
            var user = await FindByIdAsync(userId);
            if (user == null) {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, Resources.UserIdNotFound,
                    userId));
            }
            return UserTokenProvider.Generate(purpose, this, user);
        }

        private void ThrowIfDisposed() {
            if (_disposed) {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        /// <summary>
        ///     When disposing, actually dipose the store
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing) {
            if (disposing && !_disposed) {
                _disposed = true;
            }
        }

        /// <summary>
        ///     Dispose this object
        /// </summary>
        public void Dispose() {
            Dispose(true);
            Store.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
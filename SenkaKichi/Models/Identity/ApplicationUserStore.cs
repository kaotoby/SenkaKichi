using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using SenkaKichi.DbModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;
using SenkaKichi.OAuthApi.Twitter;

namespace SenkaKichi.Models
{
    /// <summary>
    ///     EntityFramework based user store implementation that supports IUserStore, IUserLoginStore and IQueryableUserStore
    /// </summary>
    public class ApplicationUserStore :
        IUserStore<AspNetUser, int>,
        IUserLoginStore<AspNetUser, int>,
        IQueryableUserStore<AspNetUser, int>
    {
        public SenkaContext Database { get; private set; }

        public IQueryable<AspNetUser> Users {
            get {
                return Database.AspNetUsers;
            }
        }

        public ApplicationUserStore() {
            Database = new SenkaContext();
        }

        public ApplicationUserStore(SenkaContext context) {
            Database = context;
        }

        public async Task CreateAsync(AspNetUser user) {
            if (user == null) throw new ArgumentNullException("user");

            Database.AspNetUsers.Add(user);
            await Database.SaveChangesAsync();
        }

        public async Task DeleteAsync(AspNetUser user) {
            if (user == null) throw new ArgumentNullException("user");

            Database.AspNetUsers.Remove(user);
            await Database.SaveChangesAsync();
        }

        public Task<AspNetUser> FindByIdAsync(int userId) {
            return Database.AspNetUsers
                .Include(user => user.TwitterInfo)
                .Include(user => user.UserLogins)
                .FirstOrDefaultAsync(user => user.Id == userId);
        }

        public Task<AspNetUser> FindByNameAsync(string userName) {
            if (userName == null) throw new ArgumentNullException("userName");

            return Database.AspNetUsers
                .Include(user => user.TwitterInfo)
                .Include(user => user.UserLogins)
                .FirstOrDefaultAsync(user => user.Player.Name == userName);
        }

        public async Task UpdateAsync(AspNetUser user) {
            if (user == null) throw new ArgumentNullException("user");

            await Database.SaveChangesAsync();
        }

        public Task DiscardAsync() {
            var changedEntries = Database.ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged).ToList();

            foreach (var entry in changedEntries.Where(x => x.State == EntityState.Modified)) {
                entry.CurrentValues.SetValues(entry.OriginalValues);
                entry.State = EntityState.Unchanged;
            }

            foreach (var entry in changedEntries.Where(x => x.State == EntityState.Added)) {
                entry.State = EntityState.Detached;
            }

            foreach (var entry in changedEntries.Where(x => x.State == EntityState.Deleted)) {
                entry.State = EntityState.Unchanged;
            }
            return Task.FromResult<object>(null);
        }

        public async Task SavePlayerVerifyTokenAsync(AspNetUser user, string token) {
            user.PlayerVerifyToken = token;
            user.PlayerVerifyEndTime = DateTime.Now;
            await Database.SaveChangesAsync();
        }

        public async Task<AspNetLoginProvider> GetLoginProviderAsync(string name) {
            var loginProvider = await Database.AspNetLoginProviders
                .FirstOrDefaultAsync(provider => provider.Name == name);
            return loginProvider;
        }

        public Task AddLoginAsync(AspNetUser user, UserLoginInfo login) {
            throw new NotSupportedException("Use overload AddLoginAsync(AspNetUser user, ExternalLoginInfo login) instead.");
        }

        public async Task AddLoginAsync(AspNetUser user, ExternalLoginInfo info) {
            if (user == null) throw new ArgumentNullException("user");
            if (info == null) throw new ArgumentNullException("info");

            long providerKey;
            if (!long.TryParse(info.Login.ProviderKey, out providerKey))
                throw new ArgumentNullException("info");

            var provider = await GetLoginProviderAsync(info.Login.LoginProvider);
            if (provider == null) {
                throw new ArgumentNullException("info");
            }

            user.UserLogins.Add(new AspNetUserLogin {
                UserId = user.Id,
                LoginProvider = provider,
                ProviderKey = providerKey,
                AccessToken = info.ExternalIdentity.Claims.First(claim => claim.Type == "urn:accesstoken").Value,
                AccessTokenSecret = info.ExternalIdentity.Claims.First(claim => claim.Type == "urn:accesstokensecret").Value
            });
        }

        public async Task UpdateLoginAsync(AspNetUserLogin login, ExternalLoginInfo info) {
            if (login == null) throw new ArgumentNullException("user");
            if (info == null) throw new ArgumentNullException("info");

            login.AccessToken = info.ExternalIdentity.Claims.First(claim => claim.Type == "urn:accesstoken").Value;
            login.AccessTokenSecret = info.ExternalIdentity.Claims.First(claim => claim.Type == "urn:accesstokensecret").Value;
            await Database.SaveChangesAsync();
        }

        public Task<AspNetUser> FindAsync(UserLoginInfo login) {
            if (login == null) throw new ArgumentNullException("login");

            long providerKey;
            if (!long.TryParse(login.ProviderKey, out providerKey))
                throw new ArgumentNullException("login");

            return Database.AspNetUsers
                .Include(user => user.TwitterInfo)
                .Include(user => user.UserLogins)
                .FirstOrDefaultAsync(user =>
                    user.UserLogins.Any(userlogin =>
                        userlogin.LoginProvider.Name == login.LoginProvider &&
                        userlogin.ProviderKey == providerKey)
                );
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(AspNetUser user) {
            IList<UserLoginInfo> result = new List<UserLoginInfo>();

            var logins = await Database.AspNetUserLogins.Where(login => login.UserId == user.Id).ToArrayAsync();
            if (logins == null) throw new ArgumentNullException("user");

            return logins.Select(login => new UserLoginInfo(login.LoginProvider.Name, login.ProviderKey.ToString())).ToList();
        }

        public Task RemoveLoginAsync(AspNetUser user, UserLoginInfo login) {
            AspNetUserLogin dbuserlogin = user.UserLogins
                .FirstOrDefault(userlogin => userlogin.LoginProvider.Name == login.LoginProvider);
            if (dbuserlogin == null) {
                throw new ArgumentNullException("login");
            }

            user.UserLogins.Remove(dbuserlogin);
            return Task.FromResult<object>(null);
        }

        public Task<Player> GetPlayerAsync(int playerId) {
            return Database.Players
                .FirstOrDefaultAsync(player => player.PlayerId == playerId);
        }

        public Task<AspNetUser> FindByPlayerIdAsync(int playerId) {
            return Database.AspNetUsers
                .Include(user => user.TwitterInfo)
                .Include(user => user.UserLogins)
                .FirstOrDefaultAsync(user => user.PlayerId == playerId);
        }

        public async Task UpdateTwitterProfileAsync(int userId, TwitterUser twitterUser) {
            TwitterInfo twitterInfo = await Database.TwitterInfoes.FirstOrDefaultAsync(info => info.AspNetUserId == userId);
            if (twitterInfo == null) {
                twitterInfo = new TwitterInfo();
                twitterInfo.AspNetUserId = userId;
                Database.TwitterInfoes.Add(twitterInfo);
            }
            twitterInfo.AvatarUrl = twitterUser.Profile_image_url_https ?? "";
            twitterInfo.BannerUrl = twitterUser.Profile_banner_url ?? "";
            twitterInfo.Description = twitterUser.Description ?? "";
            twitterInfo.Location = twitterUser.Location ?? "";
            twitterInfo.Name = twitterUser.Name ?? "";
            twitterInfo.ProfileUpdateTime = DateTime.Now;
            twitterInfo.ScreenName = twitterUser.Screen_name;
            twitterInfo.UserSite = twitterInfo.UserSiteUrl = "";
            if (twitterUser.Entities.Url.Urls.Length > 0) {
                twitterInfo.UserSite = twitterUser.Entities.Url.Urls[0].Display_url ?? "";
                twitterInfo.UserSiteUrl = twitterUser.Entities.Url.Urls[0].Expanded_url ?? "";
            }
            await Database.SaveChangesAsync();
        }

        public Task SetPlayerAsync(AspNetUser user, int playerId) {
            if (user == null) throw new ArgumentNullException("user");
            
            user.PlayerId = playerId;
            return Task.FromResult<object>(null);
        }

        public Task SetPlayerVerifiedAsync(AspNetUser user, bool verified) {
            if (user == null) throw new ArgumentNullException("user");
            
            user.IsPlayerVerified = verified;
            return Task.FromResult<object>(null);
        }

        public void Dispose() {
            Database = null;
        }
    }
}
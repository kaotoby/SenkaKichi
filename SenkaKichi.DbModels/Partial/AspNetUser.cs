using Microsoft.AspNet.Identity;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SenkaKichi.DbModels
{
    public partial class AspNetUser : IUser<int>
    {
        public override string ToString() {
            return this.UserLogins.First().ToString();
        }

        public string UserName {
            get {
                return this.Player != null ? this.Player.Name
                    : this.TwitterInfo != null ? this.TwitterInfo.Name
                    : this.ToString();
            }
            set {
                throw new NotSupportedException();
            }
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<AspNetUser, int> manager) {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }
}

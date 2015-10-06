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
using System.Globalization;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace SenkaKichi.Models
{
    /// <summary>
    ///     Validates users before they are saved
    /// </summary>
    public class ApplicationUserValidator : IIdentityValidator<AspNetUser>
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="manager"></param>
        public ApplicationUserValidator(ApplicationUserManager manager) {
            if (manager == null) {
                throw new ArgumentNullException("manager");
            }
            Manager = manager;
        }

        private ApplicationUserManager Manager { get; set; }

        /// <summary>
        ///     Validates a user before saving
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public async Task<IdentityResult> ValidateAsync(AspNetUser item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            var errors = new List<string>();
            if (item.PlayerId.HasValue) {
                await ValidatePlayerAsync(item, errors);
            }
            if (errors.Count > 0) {
                return IdentityResult.Failed(errors.ToArray());
            }
            return IdentityResult.Success;
        }

        // make sure email is not empty, valid, and unique
        private async Task ValidatePlayerAsync(AspNetUser user, List<string> errors) {
            var db = Manager.Store.Database;
            try {
                Player player = await db.Players.FindAsync(user.PlayerId);
                if (player.AspNetUsers.Any(dbuser => dbuser.IsPlayerVerified)) {
                    errors.Add(String.Format(CultureInfo.CurrentCulture, Resources.DuplicatePlayer, player));
                }
            } catch {
                errors.Add(String.Format(CultureInfo.CurrentCulture, Resources.InvalidPlayer, user.PlayerId));
            }
        }
    }
}
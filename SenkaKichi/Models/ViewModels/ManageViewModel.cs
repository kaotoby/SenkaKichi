using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace SenkaKichi.ViewModels.Manage
{
    public class IndexViewModel
    {
        public IList<UserLoginInfo> Logins { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class AddPlayerViewModel
    {
        [Required]
        [Display(Name = "Player")]
        public int? PlayerId { get; set; }
    }
}
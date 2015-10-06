using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SenkaKichi.DbModels
{
    public partial class AspNetUserLogin
    {
        public override string ToString() {
            return string.Format("{0} {1}", this.LoginProvider.Name, this.ProviderKey);
        }
    }
}

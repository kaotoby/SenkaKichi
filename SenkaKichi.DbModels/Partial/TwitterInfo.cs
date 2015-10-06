using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SenkaKichi.DbModels
{
    public partial class TwitterInfo
    {
        public override string ToString() {
            return string.Format("UserId: {0}, ScreenName: @{1}", this.AspNetUserId, this.ScreenName);
        }
    }
}

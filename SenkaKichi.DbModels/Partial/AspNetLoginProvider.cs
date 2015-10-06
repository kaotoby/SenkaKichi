using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SenkaKichi.DbModels
{
    public partial class AspNetLoginProvider
    {
        public override string ToString() {
            return string.Format("LoginProviderId: {0}, Name: {1}", this.LoginProviderId, this.Name);
        }
    }
}

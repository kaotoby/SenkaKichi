using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SenkaKichi.DbModels
{
    public partial class ServerMaintenance
    {
        public override string ToString() {
            return string.Format("Id: {0}, StartTime: {0:yyyy/M/d H:mm}, EndTime: {1:yyyy/M/d H:mm}",
                this.StartTime, this.EndTime);
        }
    }
}

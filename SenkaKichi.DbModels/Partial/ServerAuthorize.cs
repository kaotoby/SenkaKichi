using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SenkaKichi.DbModels
{
    public partial class ServerAuthorize
    {
        public override string ToString() {
            return this.Server.ToString();
        }
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SenkaKichi.DbModels
{
    public partial class Server
    {
        public override string ToString() {
            return string.Format("ServerId: {0}, Name: {1}, {2}",
                this.ServerId, this.Name, this.Enabled ? "Enabled" : "Disabled");
        }

        public bool Enabled {
            get {
                return !string.IsNullOrEmpty(this.ServerAuthorize.Password);
            }
        }

        public Server DeepClone(int id) {
            return new Server {
                ServerId = Convert.ToByte(id),
                Name = this.Name,
                NickName = this.NickName
            };
        }
    }
}

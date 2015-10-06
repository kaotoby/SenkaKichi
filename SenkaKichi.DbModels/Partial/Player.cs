using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SenkaKichi.DbModels
{
    public partial class Player
    {
        public override string ToString() {
            return string.Format("{0} {1}", this.Name, this.PlayerId);
        }
    }
}

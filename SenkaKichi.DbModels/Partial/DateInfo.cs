using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SenkaKichi.DbModels
{
    public partial class DateInfo : IComparable<DateInfo>
    {
        public override string ToString() {
            return this.Date.ToString("yyyy年M月d日 H時");
        }

        public int CompareTo(DateInfo other) {
            return this.Date.CompareTo(other.Date);
        }
    }
}

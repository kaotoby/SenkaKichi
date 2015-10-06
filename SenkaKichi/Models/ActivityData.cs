using SenkaKichi.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SenkaKichi.Models
{
    public class ActivityData
    {
        public DateInfo Date { get; set; }
        public byte Level { get; set; }
        public string Comment { get; set; }
    }
}
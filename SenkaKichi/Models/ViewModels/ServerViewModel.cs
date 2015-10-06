using SenkaKichi.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SenkaKichi.ViewModels.Server
{
    public class RankingViewModel
    {
        public IEnumerable<SenkaData> Data { get; set; }
        public SenkaKichi.DbModels.Server Server { get; set; }
        public int Page { get; set; }
        public int TotalPage { get; set; }
    }
}
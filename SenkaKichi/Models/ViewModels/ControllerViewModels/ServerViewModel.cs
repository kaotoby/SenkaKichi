using SenkaKichi.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SenkaKichi.ViewModels.Server
{
    public class RankingViewModel
    {
        public List<SenkaData> Data { get; set; }
        public DbModels.Server Server { get; set; }
        public PagerViewModels Pager { get; set; }
    }

    public class InfoViewModel
    {
        public DbModels.Server Server { get; set; }
        public DateInfo DateInfo { get; set; }
        public string JsonChart { get; set; }
    }
}
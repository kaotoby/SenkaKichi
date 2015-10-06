using SenkaKichi.DbModels;
using System.Collections.Generic;

namespace SenkaKichi.ViewModels.Home
{
    public class IndexViewModel
    {
        public IList<SenkaData> RankPointDeltaRanking { get; set; }
        public IList<SenkaData> RankPointRanking { get; set; }
        public DateInfo DateInfo { get; set; }
    }
}
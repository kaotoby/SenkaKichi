using SenkaKichi.DbModels;
using SenkaKichi.Models;
using System.Collections.Generic;

namespace SenkaKichi.ViewModels.Player
{
    public class InfoViewModel
    {
        public SenkaData LastData { get; set; }
        public IEnumerable<ActivityData> Activity { get; set; }
        public int RankPointExtra { get; set; }
        public string JsonChart { get; set; }
    }

    public class ActivityViewModel
    {
        public SenkaData LastData { get; set; }
        public IEnumerable<ActivityData> Activity { get; set; }
    }
}
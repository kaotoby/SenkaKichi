using SenkaKichi.DbModels;
using SenkaKichi.ViewModels.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SenkaKichi.Models
{
    public class AjaxSenkaResult
    {
        private RankingViewModel _model;

        public int page => _model.Pager.Page;
        public int page_total => _model.Pager.TotalPage;
        public int server_id => _model.Server?.ServerId ?? 0;
        public string server_name => _model.Server?.Name;
        public long date => (long)_model.Data[0].DateInfo.Date.Subtract(new DateTime(1970, 1, 1)).TotalMilliseconds;
        public int item_count => _model.Data.Count;
        public IEnumerable<AjaxSenkaData> senka_data { get; private set; }

        public AjaxSenkaResult(RankingViewModel model) {
            _model = model;
            senka_data = model.Data.Select(d => new AjaxSenkaData(d));
        }
    }

    public class AjaxSenkaData
    {
        private SenkaData _data;

        public short ranking => _data.Ranking;
        public short? ranking_delta => _data.RankingDelta;
        public short? ranking_all => _data.RankingAll;
        public short? ranking_all_delta => _data.RankingAllDelta;
        public int player_id => _data.PlayerId;
        public string player_name => _data.Player.Name;
        public string comment => _data.Comment;
        public short level => _data.Level;
        public int experience => _data.Experience;
        public int? experience_delta => _data.ExperienceDelta;
        public short point => _data.RankPoint;
        public short? point_delta => _data.RankPointDelta;
        public short? point_exact => _data.RankPointDeltaExact;
        public short? point_extra => _data.RankPointDeltaExtra;
        public string rank_type => _data.RankType;
        public short medals => _data.Medals;
        public AjaxSenkaData(SenkaData data) {
            _data = data;
        }
    }
}

using DevTrends.MvcDonutCaching;
using Microsoft.AspNet.Identity.Owin;
using MvcSiteMapProvider;
using SenkaKichi.DbModels;
using SenkaKichi.Models;
using SenkaKichi.ViewModels.Player;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SenkaKichi.Controllers
{
    public class PlayerController : Controller
    {
        /// <summary>
        ///     GET: Player/{id}
        /// </summary>
        /// <param name="id">Server Id</param>
        /// <param name="d">Date in yyMM formate</param>
        [DonutOutputCacheAttribute(Duration = 60)]
        [MvcSiteMapNodeAttribute(DynamicNodeProvider = "SenkaKichi.SiteMap.Player.InfoDynamicNodeProvider, SenkaKichi")]
        public async Task<ActionResult> Info(int id, string d = "") {
            DateTime date;
            SenkaData lastData = null;
            if (d == "") {
                lastData = await Repository.GetPlayerLastDataAsync(id);
            } else if (DateTime.TryParseExact(d, "yyMM", null, DateTimeStyles.None, out date)) {
                lastData = await Repository.GetPlayerLastDataAsync(id, new DateTime(date.Year, date.Month, 1, 3, 0, 0));
            }
            if (lastData == null) {
                return HttpNotFound();
            }

            date = new DateTime(lastData.DateInfo.Date.Year, lastData.DateInfo.Date.Month, 1, 3, 0, 0);
            var startDate = await Repository.FindDateInfoByDateAsync(date);
            var endDate = lastData.DateInfo;

            var playerData = new KeyValuePair<short, List<SenkaData>>(lastData.Ranking, await Repository.GetPlayerInfoAsync(id, startDate, endDate));
            var boundData = await Repository.GetServerRankingBoundAsync(lastData.Player.ServerId, lastData.Ranking, startDate, endDate);
            var chart = new ChartModels.Player(playerData, boundData);
            chart.StartTime = startDate.Date.ToString("s") + "Z";
            chart.Date = boundData.Last().Value.Last().DateInfo.ToString();
            chart.PlayerName = lastData.Player.Name;
            chart.ServerName = lastData.Player.Server.Name;
            if (lastData.DateId == lastData.Player.Server.LastUpdated) {
                chart.ServerName += " " + lastData.Ranking + "位";
            }

            var model = new InfoViewModel {
                Activity = await Repository.GetPlayerActivityAsync(id, endDate, 3),
                LastData = lastData,
                RankPointExtra = playerData.Value.Sum(data => data.RankPointDeltaExtra ?? 0),
                JsonChart = chart.ToJsonString()
            };

            return View(model);
        }


        /// <summary>
        ///     GET: Player/{id}/Activity
        /// </summary>
        /// <param name="id">Server Id</param>
        [DonutOutputCacheAttribute(Duration = 60)]
        [MvcSiteMapNodeAttribute(DynamicNodeProvider = "SenkaKichi.SiteMap.Player.ActivityDynamicNodeProvider, SenkaKichi")]
        public async Task<ActionResult> Activity(int id) {
            SenkaData lastData = lastData = await Repository.GetPlayerLastDataAsync(id);
            if (lastData == null) {
                return HttpNotFound();
            }
            var activity = await Repository.GetPlayerActivityAsync(id, lastData.DateInfo, 20);
            var model = new ActivityViewModel {
                LastData = lastData,
                Activity = activity
            };
            return View(model);
        }

        /// <summary>
        ///     GET: Player/Search
        /// </summary>
        /// <param name="q">Search query</param>
        /// <param name="p">Page</param>
        [DonutOutputCacheAttribute(Duration = 120)]
        public async Task<ActionResult> Search(string q, int server = 0, int p = 1) {
            var date = await Repository.GetAllServerLastUpdatedAsync();
            var result = await Repository.SearchPlayerAsync(q, server, date, p);
            return View(result);
        }

        /// <summary>
        ///     GET: Player/Suggest
        /// </summary>
        /// <param name="q">Search query</param>
        /// <param name="p">Page</param>
        [DonutOutputCacheAttribute(Duration = 120, Location = System.Web.UI.OutputCacheLocation.Any)]
        public async Task<ActionResult> Suggest(string q, int server = 0) {
            if (string.IsNullOrWhiteSpace(q)) {
                return new AjaxResult<object>(false);
            }
            var date = await Repository.GetAllServerLastUpdatedAsync();
            var result = await Repository.SearchSuggestPlayerAsync(q, server, date);
            return new AjaxResult<PlayerSuggestResult[]>(true, result);
        }

        #region Declare
        private SenkaRepository _repository;

        public SenkaRepository Repository {
            get {
                return _repository ?? HttpContext.GetOwinContext().Get<SenkaRepository>();
            }
            private set {
                _repository = value;
            }
        }

        public PlayerController() {
        }

        public PlayerController(SenkaRepository repository) {
            Repository = repository;
        }

        #endregion
    }
}
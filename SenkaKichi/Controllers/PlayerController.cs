using DevTrends.MvcDonutCaching;
using Microsoft.AspNet.Identity.Owin;
using MvcSiteMapProvider;
using SenkaKichi.DbModels;
using SenkaKichi.Models;
using SenkaKichi.ViewModels;
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
    public class PlayerController : ControllerBase
    {
        /// <summary>
        ///     GET: Player/{id}
        /// </summary>
        /// <param name="id">Server Id</param>
        /// <param name="date">Date in yyMM formate</param>
        [OutputCache(Duration = 1200)]
        [MvcSiteMapNode(DynamicNodeProvider = "SenkaKichi.SiteMap.Player.InfoDynamicNodeProvider, SenkaKichi")]
        public async Task<ActionResult> Info(int id, string date = "") {
            DateTime dateTime;
            SenkaData lastData = null;
            if (date == "") {
                lastData = await repository.GetPlayerLastDataAsync(id);
            } else if (DateTime.TryParseExact(date, "yyMM", null, DateTimeStyles.None, out dateTime)) {
                lastData = await repository.GetPlayerLastDataAsync(id, new DateTime(dateTime.Year, dateTime.Month, 1, 3, 0, 0));
            }
            if (lastData == null) {
                return HttpNotFound();
            }

            dateTime = new DateTime(lastData.DateInfo.Date.Year, lastData.DateInfo.Date.Month, 1, 3, 0, 0);
            var startDate = await repository.FindDateInfoByDateAsync(dateTime);

            int endDateId = _repository.CalcuateEndDateId(startDate);
            var playerInfoes = await repository.GetPlayerInfoAsync(id, startDate, endDateId);
            var playerData = new KeyValuePair<short, List<SenkaData>>(lastData.Ranking, playerInfoes);
            var boundData = await repository.GetServerRankingBoundAsync(lastData.Player.ServerId, lastData.Ranking, startDate);
            var chart = new ChartModels.Player(playerData, boundData);
            chart.StartTime = startDate.Date.ToString("s") + "Z";
            chart.Date = boundData.Last().Value.Last().DateInfo.ToString();
            chart.PlayerName = lastData.Player.Name;
            chart.ServerName = lastData.Player.Server.Name;
            if (lastData.DateId == lastData.Player.Server.LastUpdated) {
                chart.ServerName += " " + lastData.Ranking + "位";
            }

            var model = new InfoViewModel {
                Activity = await repository.GetPlayerActivityAsync(id, lastData.DateInfo, 3),
                LastData = lastData,
                RankPointExtra = playerData.Value.Sum(data => data.RankPointDeltaExtra ?? 0),
                JsonChart = ChartModels.ConvertToJson(chart)
            };

            return View(model);
        }


        /// <summary>
        ///     GET: Player/{id}/Activity
        /// </summary>
        /// <param name="id">Server Id</param>
        [OutputCache(Duration = 1200)]
        [MvcSiteMapNode(DynamicNodeProvider = "SenkaKichi.SiteMap.Player.ActivityDynamicNodeProvider, SenkaKichi")]
        public async Task<ActionResult> Activity(int id) {
            SenkaData lastData = lastData = await repository.GetPlayerLastDataAsync(id);
            if (lastData == null) {
                return HttpNotFound();
            }
            var activity = await repository.GetPlayerActivityAsync(id, lastData.DateInfo, 20);
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
        /// <param name="server">Server Id</param>
        /// <param name="page">Page</param>
        [OutputCache(Duration = 1200)]
        public async Task<ActionResult> Search(string q, int server = 0, int page = 1) {
            var date = await repository.GetAllServerLastUpdatedAsync();
            var result = await repository.SearchPlayerAsync(q, server, date, page);
            return View(result);
        }

        /// <summary>
        ///     GET: Player/Suggest
        /// </summary>
        /// <param name="q">Search query</param>
        /// <param name="server">Server Id</param>
        [OutputCache(Duration = 7200)]
        public async Task<ActionResult> Suggest(string q, int server = 0) {
            if (string.IsNullOrWhiteSpace(q)) {
                return new AjaxResult<object>(false);
            }
            var date = await repository.GetAllServerLastUpdatedAsync();
            var result = await repository.SearchSuggestPlayerAsync(q, server, date);
            return new AjaxResult<PlayerSuggestResult[]>(true, result);
        }
    }
}
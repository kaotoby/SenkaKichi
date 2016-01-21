using DevTrends.MvcDonutCaching;
using Microsoft.AspNet.Identity.Owin;
using MvcSiteMapProvider;
using SenkaKichi.DbModels;
using SenkaKichi.Models;
using SenkaKichi.ViewModels.Server;
using SenkaKichi.ViewModels;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Collections.Generic;

namespace SenkaKichi.Controllers
{
    public class ServerController : ControllerBase
    {
        // GET: Server
        [OutputCache(Duration = 1200)]
        [MvcSiteMapNode(DynamicNodeProvider = "SenkaKichi.SiteMap.Server.InfoDynamicNodeProvider, SenkaKichi")]
        public async Task<ActionResult> Info(int id, string date = "") {
            if (id == 0) {
                return View("InfoAll");
            }
            if (!SenkaRepository.Servers.ContainsKey(id)) {
                return HttpNotFound();
            }

            Server server = SenkaRepository.Servers[id];
            DateTime dateTime = default(DateTime);
            if (date == "") {
                dateTime = DateTime.Now;
            } else {
                DateTime.TryParseExact(date, "yyMM", null, DateTimeStyles.None, out dateTime);
            }
            if (dateTime == default(DateTime)) {
                return HttpNotFound();
            }            

            dateTime = new DateTime(dateTime.Year, dateTime.Month, 1, 3, 0, 0);
            DateInfo startDate = await repository.FindDateInfoByDateAsync(dateTime);
            if (startDate == null) {
                startDate = await repository.GetServerLastUpdatedAsync(id);
            }

            int endDateId = _repository.CalcuateEndDateId(startDate);
            var serverData = await repository.GetServerInfoAsync(id, startDate, endDateId);
            var chart = new ChartModels.Server(serverData);
            var lastData = serverData[1].Last();
            chart.StartTime = startDate.Date.ToString("s") + "Z";
            chart.Date = lastData.DateInfo.ToString();
            chart.ServerName = server.Name;

            var model = new InfoViewModel {
                Server = server,
                DateInfo = lastData.DateInfo,
                JsonChart = ChartModels.ConvertToJson(chart)
            };

            return View(model);
        }

        /// <summary>
        ///     GET: Server/{id}/Ranking
        /// </summary>
        /// <param name="id">Server Id</param>
        /// <param name="page">Page number</param>
        /// <param name="date">Date in yyMMddHH formate</param>
        [OutputCache(Duration = 1200)]
        [MvcSiteMapNode(DynamicNodeProvider = "SenkaKichi.SiteMap.Server.RankingDynamicNodeProvider, SenkaKichi")]
        public async Task<ActionResult> Ranking(int id, int page = 0, string date = "") {
            DateTime dateTime;

            #region Ranking All

            if (id == 0) {
                if (page == 0) page = 1;
                DateInfo dateInfo = null;
                if (date == "") {
                    dateInfo = await repository.GetAllServerLastUpdatedAsync();
                } else if (DateTime.TryParseExact(date, "yyMMddHH", null, DateTimeStyles.None, out dateTime)) {
                    dateInfo = await repository.FindDateInfoByDateAsync(dateTime);
                }
                if (dateInfo == null) {
                    return HttpNotFound();
                }
                var data = await repository.GetAllServerRankingAsync(dateInfo, (page - 1) * 1000, 1000);
                if (data.Count == 0) {
                    return HttpNotFound();
                }
                var rankingModel = new RankingViewModel() {
                    Server = null,
                    Data = data,
                    Pager = new PagerViewModels() {
                        Page = page,
                        TotalPage = 10
                    }                    
                };
                return View("RankingAll", rankingModel);
            }

            #endregion

            if (!SenkaRepository.Servers.ContainsKey(id)) {
                return HttpNotFound();
            }
            var server = SenkaRepository.Servers[id].DeepClone(id);

            if (date == "") {
                server.DateInfo = await repository.GetServerLastUpdatedAsync(id);
            } else if (DateTime.TryParseExact(date, "yyMMddHH", null, DateTimeStyles.None, out dateTime)) {
                server.DateInfo = await repository.FindDateInfoByDateAsync(dateTime);
            } else {
                return HttpNotFound();
            }

            var model = new RankingViewModel() {
                Server = server,
                Pager = new PagerViewModels() {
                    Page = page,
                    TotalPage = 5
                }
            };

            model.Data = await repository.GetServerRankingAsync(id, server.DateInfo);
            if (page == 0) {
                model.Data = model.Data
                    .Where(data => data.Ranking <= 100 || data.Ranking == 500)
                    .ToList();
            } else {
                model.Data = model.Data.Skip(200 * (page - 1)).Take(200).ToList();
            }
            return View(model);
        }

    }
}
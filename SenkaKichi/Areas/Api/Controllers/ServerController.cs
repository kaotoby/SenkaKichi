using DevTrends.MvcDonutCaching;
using Microsoft.AspNet.Identity.Owin;
using SenkaKichi.DbModels;
using SenkaKichi.Models;
using SenkaKichi.ViewModels;
using SenkaKichi.ViewModels.Server;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Collections.Generic;

namespace SenkaKichi.Areas.Api.Controllers
{
    public class ServerController : ControllerBase
    {
        /// <summary>
        ///     GET: Server/{id}/Ranking
        /// </summary>
        /// <param name="id">Server Id</param>
        /// <param name="page">Page number</param>
        /// <param name="date">Date in yyMMddHH formate</param>
        [DonutOutputCache(Duration = 300)]
        public async Task<ActionResult> Ranking(int id, int page = 0, string date = "") {
            if (id == 0) {
                return View("RankingAll");
            }
            if (!SenkaRepository.Servers.ContainsKey(id)) {
                return new AjaxResult<object>(false);
            }
            if (!SenkaRepository.Servers[id].Enabled) {
                return new AjaxResult<object>(false);
            }

            DateTime dateTime;
            var server = SenkaRepository.Servers[id].DeepClone(id);

            if (date == "") {
                server.DateInfo = await repository.GetServerLastUpdatedAsync(id);
            } else if (DateTime.TryParseExact(date, "yyMMddHH", null, DateTimeStyles.None, out dateTime)) {
                server.DateInfo = await repository.FindDateInfoByDateAsync(dateTime);
            } else {
                return new AjaxResult<object>(false);
            }

            var model = new RankingViewModel() {
                Server = server,
                Pager = new PagerViewModels() {
                    Page = page,
                    TotalPage = 10
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
            return new AjaxResult<IEnumerable<SenkaData>>(true, model.Data);
        }

    }
}
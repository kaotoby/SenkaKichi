using DevTrends.MvcDonutCaching;
using Microsoft.AspNet.Identity.Owin;
using MvcSiteMapProvider;
using SenkaKichi.DbModels;
using SenkaKichi.Models;
using SenkaKichi.ViewModels.Server;
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
    public class ServerController : Controller
    {
        // GET: Server
        [DonutOutputCacheAttribute(Duration = 60)]
        [MvcSiteMapNodeAttribute(DynamicNodeProvider = "SenkaKichi.SiteMap.Server.InfoDynamicNodeProvider, SenkaKichi")]
        public async Task<ActionResult> Info(int id) {
            if (id == 0) {
                return View("InfoAll");
            }
            if (!SenkaRepository.Servers.ContainsKey(id)) {
                return HttpNotFound();
            }
            if (!SenkaRepository.Servers[id].Enabled) {
                return View("NoData", SenkaRepository.Servers[id]);
            }
            return View(SenkaRepository.Servers[id]);
        }

        /// <summary>
        ///     GET: Server/{id}/Ranking
        /// </summary>
        /// <param name="id">Server Id</param>
        /// <param name="p">Page number</param>
        /// <param name="d">Date in yyMMddHH formate</param>
        /// <returns></returns>
        [DonutOutputCacheAttribute(Duration = 60, VaryByCustom = "Ajax")]
        [MvcSiteMapNodeAttribute(DynamicNodeProvider = "SenkaKichi.SiteMap.Server.RankingDynamicNodeProvider, SenkaKichi")]
        public async Task<ActionResult> Ranking(int id, int p = 0, string d = "") {
            if (id == 0) {
                return View("RankingAll");
            }
            if (!SenkaRepository.Servers.ContainsKey(id)) {
                return HttpNotFound();
            }
            if (!SenkaRepository.Servers[id].Enabled) {
                return View("NoData", SenkaRepository.Servers[id]);
            }

            DateTime date;
            var server = SenkaRepository.Servers[id].DeepClone(id);

            if (d == "") {
                server.DateInfo = await Repository.GetServerLastUpdatedAsync(id);
            } else if (DateTime.TryParseExact(d, "yyMMddHH", null, DateTimeStyles.None, out date)) {
                server.DateInfo = await Repository.FindDateInfoByDateAsync(date);
            } else {
                return HttpNotFound();
            }

            var model = new RankingViewModel {
                Server = server,
                Page = p,
                TotalPage = 10
            };

            model.Data = await Repository.GetServerRankingAsync(id, server.DateInfo);
            if (p == 0) {
                model.Data = model.Data
                    .Where(data => data.Ranking <= 100 || data.Ranking == 500);
            } else {
                model.Data = model.Data.Skip(200 * (p - 1)).Take(200);
            }
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") {
                return new AjaxResult<IEnumerable<SenkaData>>(true, model.Data);
            }
            return View(model);
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

        public ServerController() { }

        public ServerController(SenkaRepository repository) {
            Repository = repository;
        }
        #endregion

    }
}
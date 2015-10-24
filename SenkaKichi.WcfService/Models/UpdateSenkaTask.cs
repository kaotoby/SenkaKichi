using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SenkaKichi.DbModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Entity;

namespace SenkaKichi.WcfService.Models
{
    public class UpdateSenkaTask : TaskInfo
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(UpdateSenkaTask).FullName);

        private const int MAX_RETRY_COUNT = 5;

        public UpdateSenkaTask(IEnumerable<DateTime> schedule) : base(schedule) { }

        public UpdateSenkaTask(TimeSpan interval, int startHour, int startMinute, int startSecond)
            : base(interval, startHour, startMinute, startSecond) { }

        protected override void Main() {
            var now = DateTime.Now;
            var servers = Manager.Servers.Values.Where(server => server.Enabled);

            log.Info("[UpdateSenkaTask] Start");

            UpdateDateInfo();
            Parallel.ForEach(servers, server => {
                UpdateServerInfoHost(server);
            });
            WriteRankingAllData();
            PostRankingToTwitter();
            PostDeltaRankingToTwitter();
            PostExactDeltaRankingToTwitter();

            log.Info("[UpdateSenkaTask] End");
        }

        private void UpdateServerInfoHost(ServerInfo info) {
            bool success = false;

            for (int i = 0; i < MAX_RETRY_COUNT && !success; i++) {
                Thread.Sleep(5 * 60 * 1000 * i); //5*i min
                if (i > 0 && info.Server.ServerAuthorize.Password != null) {
                    info.RefreshToken();
                }
                try {
                    log.Debug(string.Format("[ServerId {0}] Server update begin.", info.Server.ServerId));
                    UpdateServerInfo(info);
                    log.Info(string.Format("[ServerId {0}] Server update ended.", info.Server.ServerId));
                    success = true;
                } catch (Exception ex) {
                    log.Error(string.Format("[ServerId {0}] Update failed! Count: {1}", info.Server.ServerId, i + 1), ex);
                }
            }
            if (!success) {
                log.Fatal(string.Format("[ServerId {0}] Update disabled due to too many fail!", info.Server.ServerId));
                info.Enabled = false;
            }
        }

        public static void UpdateServerInfo(ServerInfo data) {
            data.InitializeUpdate(Manager.DateInfo);

            HttpHelper helper = new HttpHelper();
            Random rand = new Random();
            var postDic = new Dictionary<string, object>();
            int currentRetryCount = 0;

#if DEBUG
            for (int i = 1; i <= 3; i++) {
#else
            for (int i = 1; i < 100; i++) {
#endif
                string jsonResult = "";
                postDic["api_pageno"] = i.ToString();
                postDic["api_verno"] = "1";
                postDic["api_token"] = data.ApiToken;
                helper.CTRHttp(data.FullPath, data.SwfReferer, postDic, ref jsonResult);

                if (BatchParse(jsonResult, data, i)) {
                    Thread.Sleep(rand.Next(700, 2000));
                    currentRetryCount = 0;
                } else {
                    if (currentRetryCount == 2) {
                        throw new WebException(string.Format("[ServerId {0}] Page {1}, request failed!", data.Server.ServerId, i));
                    }

                    i--; //Retry 3s, 30s
                    Thread.Sleep(1000 * 3 * (int)Math.Pow(10, currentRetryCount++));
                }
            }

            data.SaveToDataBase();
        }

        public static void UpdateDateInfo() {
            DateTime current = DateTime.UtcNow.AddHours(6);
            DateTime date = DateTime.UtcNow.AddHours(6);
            if (current.Hour >= 12) {
                date = new DateTime(current.Year, current.Month, current.Day, 15, 0, 0);
            } else {
                date = new DateTime(current.Year, current.Month, current.Day, 3, 0, 0);
            }

            Manager.DateInfo = Manager.Database.DateInfoes.FirstOrDefault(dateInfo => dateInfo.Date == date);
            if (Manager.DateInfo == null) {
                Manager.DateInfo = new DateInfo { Date = date };
                Manager.Database.DateInfoes.Add(Manager.DateInfo);
                Manager.Database.SaveChanges();
            }
            log.Info(string.Format("Current date: {0} ", Manager.DateInfo.DateId, Manager.DateInfo));
        }

        private static bool BatchParse(string jsonString, ServerInfo server, int page) {
            jsonString = jsonString.Replace("svdata=", "");

            try {
                JObject jsonData = JObject.Parse(jsonString);
                int apiResult = (int)jsonData["api_result"];

                if (apiResult == 1) {
                    var results = jsonData["api_data"]["api_list"].Children();
                    foreach (var result in results) {
                        var senka = JsonConvert.DeserializeObject<ApiSenkaResult>(result.ToString());
                        server.DataSet.Add(senka);
                    }
                    // Save To File
                    File.AppendAllText(server.LogPath, string.Format("api_req_ranking,getlist,\"{0}\"\"\",\r\n", jsonString.Replace("\"\"", new String('\"', 8))));
                } else if (apiResult == 201) {
                    log.Warn(string.Format("[ServerId {0}] Requested failed (Error 201). Server will be re-login.", server.Server.ServerId));
                    server.RefreshToken();
                    return false;
                } else if (apiResult == 100) {
                    log.Warn(string.Format("[ServerId {0}] Requested failed (Error 100). Server under maintence.", server.Server.ServerId));
                    Thread.Sleep(60 * 60 * 1000); //Wait 1 hour
                    return false;
                } else {
                    throw new WebException(jsonString);
                }
            } catch (Exception ex) {
                log.Warn(string.Format("[ServerId {0}] Page {1} Pharing ERROR!", server.Server.ServerId, page), ex);
                return false;
            }
            return true;
        }

        private void WriteRankingAllData() {
            var manager = ServiceManager.Current;
            manager.RefreshServer();
            DateInfo date = manager.Servers[1].DateInfo;
            if (manager.Servers.Values.Any(s => s.DateInfo != date)) {
                throw new InvalidOperationException("Can't write ranking with in all servers. Some servers haven't complete the update.");
            }

            var ranking = manager.Database.SenkaDatas
                    .Where(data => data.DateId == manager.Servers[1].DateInfo.DateId)
                    .OrderByDescending(data => data.RankPoint)
                    .ThenByDescending(data => data.Experience)
                    .Take(10000)
                    .ToArray();

            for (int i = 0; i < ranking.Length; i++) {
                ranking[i].RankingAll = (short)(i + 1);
            }
            manager.Database.SaveChanges();
            log.Debug("Finish writing the ranking within all servers.");
        }

        private void PostRankingToTwitter() {
            var manager = ServiceManager.Current;
            var top3RankPoint = manager.Database.SenkaDatas
                .Include(data => data.Player)
                .Where(data => data.DateId == manager.Servers[1].DateInfo.DateId)
                .OrderByDescending(data => data.RankPoint)
                .ThenByDescending(data => data.Experience)
                .Take(3)
                .ToArray();
            if (top3RankPoint.Length == 0) {
                throw new InvalidOperationException("Error when getting top 3 rank point.");
            }

            var date = manager.Servers[1].DateInfo;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} 戦果ランキング\n", date);
            for (int i = 0; i < 3; i++) {
                var player = top3RankPoint[i].Player;
                sb.AppendFormat("{0}位 {1} {2} {3}\n", i + 1,
                    manager.Servers[player.ServerId].Server.NickName, player.Name, top3RankPoint[i].RankPoint);
            }
            sb.Length--;
            manager.TwitterManager.PostStatusesUpdateAsync(1, sb.ToString()).RunSynchronously();
            log.Debug("Finish posting ranking to twitter");
        }

        private void PostDeltaRankingToTwitter() {
            var manager = ServiceManager.Current;
            var top3RankDelta = manager.Database.SenkaDatas
                .Include(data => data.Player)
                .Where(data => data.DateId == manager.Servers[1].DateInfo.DateId)
                .Where(data => data.RankingDelta != null)
                .OrderByDescending(data => data.RankingDelta)
                .Take(3)
                .ToArray();
            if (top3RankDelta.Length == 0) {
                log.Info("No ranking delta data to post.");
                return;
            }

            var date = manager.Servers[1].DateInfo;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} 戦果増分ランキング\n", date);
            for (int i = 0; i < 3; i++) {
                var player = top3RankDelta[i].Player;
                sb.AppendFormat("{0}位 {1} {2} +{3}\n", i + 1,
                    manager.Servers[player.ServerId].Server.NickName, player.Name, top3RankDelta[i].RankingDelta);
            }
            manager.TwitterManager.PostStatusesUpdateAsync(1, sb.ToString()).RunSynchronously();
            log.Debug("Finish posting ranking delta to twitter");
        }

        private void PostExactDeltaRankingToTwitter() {
            var manager = ServiceManager.Current;
            var top3RankDelta = manager.Database.SenkaDatas
                .Include(data => data.Player)
                .Where(data => data.DateId == manager.Servers[1].DateInfo.DateId)
                .Where(data => data.ExperienceDelta != null)
                .OrderByDescending(data => data.ExperienceDelta)
                .Take(3)
                .ToArray();
            if (top3RankDelta.Length == 0) {
                log.Info("No exact ranking delta data to post.");
                return;
            }

            var date = manager.Servers[1].DateInfo;
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0} 経験値増分ランキング\n", date);
            for (int i = 0; i < 3; i++) {
                var player = top3RankDelta[i].Player;
                sb.AppendFormat("{0}位 {1} {2} +{3}({4:0.00})\n", i + 1,
                    manager.Servers[player.ServerId].Server.NickName, player.Name,
                    top3RankDelta[i].ExperienceDelta, top3RankDelta[i].ExactRankPointDelta);
            }
            manager.TwitterManager.PostStatusesUpdateAsync(1, sb.ToString()).RunSynchronously();
            log.Debug("Finish posting exact ranking delta to twitter");
        }
    }
}
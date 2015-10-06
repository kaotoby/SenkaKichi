using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SenkaKichi.DbModels;
using SenkaKichi.WcfService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SenkaKichi.WcfService.Models
{
    public class UpdateSenkaTask : TaskInfo
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(UpdateSenkaTask).FullName);

        public UpdateSenkaTask(IEnumerable<DateTime> schedule) : base(schedule) { }

        public UpdateSenkaTask(TimeSpan interval, int startHour, int startMinute, int startSecond)
            : base(interval, startHour, startMinute, startSecond) { }

        protected override void Main() {
            var now = DateTime.Now;
            var maintenance = ServiceManager.Current.Database.ServerMaintenances
                .OrderByDescending(m => m.Id)
                .FirstOrDefault();

            if (maintenance != null && now >= maintenance.StartTime && now <= maintenance.EndTime) {
                log.Warn("[UpdateSenkaTask] Server Under Maintenance, " + maintenance.ToString());
                NextRunTime = maintenance.EndTime;
                return;
            }

            log.Info("[UpdateSenkaTask] Start");
            UpdateDateInfo();
            var servers = Manager.Servers.Values.Where(server => server.Enabled);
            Parallel.ForEach(servers, server => {
                UpdateServerInfoHost(server);
            });
            log.Info("[UpdateSenkaTask] End");
        }

        private void UpdateServerInfoHost(ServerInfo info) {
            bool success = false;

            for (int i = 0; i < 5 && !success; i++) {
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

                    i--; //Retry
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
                } else {
                    throw new WebException(jsonString);
                }
            } catch (Exception ex) {
                log.Warn(string.Format("[ServerId {0}] Page {1} Pharing ERROR!", server.Server.ServerId, page), ex);
                return false;
            }
            return true;
        }
    }
}
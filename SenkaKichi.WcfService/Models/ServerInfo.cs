using log4net;
using SenkaKichi.DbModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace SenkaKichi.WcfService.Models
{
    public class ServerInfo
    {

        #region Public Declarition
        /// <summary>
        /// The Server is enabled or not;
        /// </summary>
        public bool Enabled { get; set; }
        /// <summary>
        /// The Server data in DB.
        /// </summary>
        public Server Server { get; private set; }
        /// <summary>
        /// The time that data collected.
        /// </summary>
        public DateInfo DateInfo { get; private set; }
        /// <summary>
        /// The server IP address.
        /// </summary>
        public string IP { get; set; }
        /// <summary>
        /// The API Token
        /// </summary>
        public string ApiToken { get; private set; }
        /// <summary>
        /// The path to the senka list.
        /// </summary>
        public const string ApiPath = "/kcsapi/api_req_ranking/getlist";
        /// <summary>
        /// The full path to the senka list.
        /// </summary>
        public string FullPath {
            get {
                return string.Format("http://{0}{1}", IP, ApiPath);
            }
        }
        /// <summary>
        /// The full path to the main swf file.
        /// </summary>
        public string SwfReferer {
            get {
                return string.Format("http://{0}/kcs/mainD2.swf?api_token={1}/[[DYNAMIC]]/1", IP, ApiToken);
            }
        }
        /// <summary>
        /// The log file path to be saved.
        /// </summary>
        public string LogPath {
            get {
                return string.Format("{0}{1}{2:MMddt}.log", LogDirPath, Server.NickName, DateInfo.Date);
            }
        }
        /// <summary>
        /// The log file dir to be saved.
        /// </summary>
        public string LogDirPath {
            get {
                return string.Format("{0}\\{1}{2}\\", _logdir, Server.ServerId, Server.NickName);
            }
        }
        /// <summary>
        /// The httphelper contains cookies.
        /// </summary>
        public HttpHelper HttpHelper { get; private set; }
        /// <summary>
        /// The server cache data.
        /// </summary>
        public List<ApiSenkaResult> DataSet { get; set; }
        /// <summary>
        /// The server cache data.
        /// </summary>
        public bool IsUpdating { get; private set; }
        #endregion

        #region Private Declarition
        private string _logdir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SenkaLog");
        private static readonly ILog log = LogManager.GetLogger(typeof(ServerInfo).FullName);
        #endregion

        public ServerInfo(Server server) {
            IsUpdating = true;
            Server = server;
            Enabled = Server.Enabled;
            HttpHelper = new HttpHelper();
            DataSet = new List<ApiSenkaResult>();
            IP = Server.ServerAuthorize.IpAddress;
            ApiToken = server.ServerAuthorize.Token;
            if (Enabled && string.IsNullOrEmpty(ApiToken)) {
                RefreshToken();
            }
            IsUpdating = false;
        }

        public override string ToString() {
            return Server.ToString();
        }

        public void InitializeUpdate(DateInfo date) {
            IsUpdating = true;
            DateInfo = date;
            DataSet.Clear();
            if (!Directory.Exists(LogDirPath)) {
                Directory.CreateDirectory(LogDirPath);
            }
            if (File.Exists(this.LogPath)) {
                File.Delete(this.LogPath);
            }
        }

        public void RefreshToken() {
            DmmLoginHelper helper = new DmmLoginHelper(Server.ServerAuthorize, HttpHelper);
            log.Debug(string.Format("[ServerId {0}] GetToken started.", Server.ServerId));

            if (Server.ServerAuthorize.Password == null) {
                Enabled = false;
                log.Fatal(string.Format("[ServerId {0}] Token expired, update disabled due to account not set!", Server.ServerId));
            }
#if DEBUG
            if (Server.ServerId == 19) {
                ApiToken = helper.GetToken();
                using (var db = new SenkaContext()) {
                    var sa = db.ServerAuthorizes.Find(Server.ServerId);
                    sa.Token = ApiToken;
                    db.SaveChanges();
                }
                log.Warn(string.Format("[ServerId {0}] Token updated", Server.ServerId));
            } else {
                Enabled = false;
            }
            //helper.Process(out _ip, out _apiToken, out _apiStartTime);
#else
            try {
                ApiToken = helper.GetToken();
                using (var db = new SenkaContext()) {
                    var sa = db.ServerAuthorizes.Find(Server.ServerId);
                    sa.Token = ApiToken;
                    db.SaveChanges();
                }
                Enabled = true;
                log.Warn(string.Format("[ServerId {0}] Token updated", Server.ServerId));
            } catch (Exception ex) {
                Enabled = false;
                log.Error(string.Format("[ServerId {0}] Login fail!", Server.ServerId), ex);
            }
#endif
        }

        public void SaveToDataBase() {
            var database = new SenkaContext();
            int count = database.SenkaDatas
                .RemoveRange(from data in database.SenkaDatas
                             where data.DateId == DateInfo.DateId
                             && data.Player.ServerId == Server.ServerId
                             select data)
                .Count();

            if (count > 0) {
                log.Info(string.Format("[ServerId {0}] {1} datas were deleted and have been re-requested.", Server.ServerId, count));
            }

            var lastData = database.SenkaDatas
                .Where(data =>
                    data.DateId == DateInfo.DateId - 1 &&
                    data.Player.ServerId == Server.ServerId)
                .ToDictionary(d => d.PlayerId, d => d);

            foreach (var item in DataSet) {
                SenkaData data = new SenkaData {
                    DateId = DateInfo.DateId,
                    Ranking = item.api_no,
                    PlayerId = item.api_member_id,
                    RankPoint = item.api_rate,
                    Comment = item.api_comment,
                    Level = item.api_level,
                    Experience = item.api_experience,
                    RankTypeId = item.api_rank,
                    Medals = item.api_medals
                };

                if (database.Players.Find(data.PlayerId) == null) {
                    database.Players.Add(new Player() {
                        PlayerId = data.PlayerId,
                        Name = item.api_nickname,
                        ServerId = Server.ServerId
                    });
                }

                if (DateInfo.Date.Day == 1 && DateInfo.Date.Hour == 3) {
                    if (lastData.Keys.Contains(data.PlayerId)) {
                        data.SetDelta(lastData[data.PlayerId], true);
                    } else {
                        data.SetDelta();
                    }
                } else if (lastData.Keys.Contains(data.PlayerId)) {
                    data.SetDelta(lastData[data.PlayerId], false);
                }

                database.SenkaDatas.Add(data);
            }

            database.Servers.Find(Server.ServerId).LastUpdated = DateInfo.DateId;
            database.SaveChanges();
            database.Dispose();
            IsUpdating = false;
        }
    }
}

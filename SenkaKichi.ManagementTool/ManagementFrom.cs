using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SenkaKichi.DbModels;
using SenkaKichi.OAuthApi.Twitter;
using SenkaKichi.ManagementTool.ServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SenkaKichi.ManagementTool
{
    public partial class ManagementFrom : Form
    {
        public static Logger Log {
            get {
                if (_log == null) {
                    _log = new Logger();
                }
                return _log;
            }
        }

        private static Logger _log;
        private SenkaContext _db;
        private ServiceClient _client;
        private bool inprogress = false;

        public ManagementFrom() {
            InitializeComponent();
        }

        private void ManagementFrom_Load(object sender, EventArgs e) {
            Log.OnLogChanged += Log_OnLogChanged;
            _db = new SenkaContext();
            _client = new ServiceClient();
        }

        void Log_OnLogChanged(object sender, LogEventArgs e) {
            FromHelper.AppendText(this, textBoxLog, e.NewLog + "\r\n");
        }

        private async void buttonForceUpdateData_Click(object sender, EventArgs e) {
            if (!inprogress) {
                inprogress = true;
                string serverId = ShowInputDialog("Enter Server Id");
                if (!string.IsNullOrEmpty(serverId)) {
                    var result = await _client.UpdateSenkaDataAsync(int.Parse(serverId));
                    Log.Debug("Update Senka Data: " + result.ToString());
                }
                inprogress = false;
            }
        }

        private async void buttonUpdateIp_Click(object sender, EventArgs e) {
            if (!inprogress) {
                inprogress = true;
                var result = await _client.UpdateServerIpAddressAsync();
                Log.Debug("Update Ip: " + result.ToString());
                inprogress = false;
            }
        }

        private string ShowInputDialog(string text) {
            InputDialog inputDialog = new InputDialog();

            string result = null;
            if (inputDialog.ShowDialog("Input", text, this) == DialogResult.OK) {
                result = inputDialog.textBox1.Text;
            }

            inputDialog.Dispose();
            return result;
        }

        private DateTime ShowDateTimeDialog(string text) {
            DateTimeDialog datetimeDialog = new DateTimeDialog();
            datetimeDialog.dateTimePicker1.Value = DateTime.Now;

            DateTime result = default(DateTime);
            if (datetimeDialog.ShowDialog("Input", text, this) == DialogResult.OK) {
                result = datetimeDialog.dateTimePicker1.Value;
            }

            datetimeDialog.Dispose();
            return result;
        }

        private async void buttonTest_Click(object sender, EventArgs e) {
            Log.Debug("Pressed");
            SenkaKichi.OAuthApi.Twitter.TwitterApiManager mamger = new SenkaKichi.OAuthApi.Twitter.TwitterApiManager(new SenkaContext());
            try {
                Log.Debug("ready!");
                await mamger.PostStatusesUpdateAsync(1, Guid.NewGuid().ToString("N") + "\ntest2 #test");
                Log.Debug("OK");
            } catch (Exception ex) {
                Log.Debug("Fail", ex);
            }
        }

        private async void buttonImportData_Click(object sender, EventArgs e) {
            if (folderBrowserDialog1.ShowDialog() != DialogResult.OK) {
                return;
            }
            var servers = await _db.Servers.ToListAsync();
            DateTime startDate = new DateTime(2016, 1, 1, 3, 0, 0);
            string folder = folderBrowserDialog1.SelectedPath;
            //foreach (var server in servers)
            Parallel.ForEach(servers, server =>
            {
                using (var database = new SenkaContext()) {
                    var players = new Dictionary<int, Player>();
                    var lastData = new Dictionary<int, SenkaData>();
                    for (int i = 1; i <= 22; i++) {
                        string logFile = string.Format("{0}\\{1}\\{2}{3:MMddt}.log", folder, server.ServerId, server.NickName, startDate.AddHours((i - 1) * 12));
                        var jdata = File.ReadLines(logFile)
                            .Select(s => s
                                .Replace(new string('\"', 8), "\"\"")
                                .Replace("\"\"\",", "")
                                .Replace("api_req_ranking,getlist,\"", ""));
                        var result = new List<ApiSenkaResult>();
                        foreach (var item in jdata) {
                            BatchParse(item, ref result);
                        }

                        var listData = new Dictionary<int, SenkaData>();

                        foreach (var item in result) {
                            SenkaData data = new SenkaData {
                                DateId = i,
                                Ranking = item.api_no,
                                PlayerId = item.api_member_id,
                                RankPoint = item.api_rate,
                                Comment = item.api_comment,
                                Level = item.api_level,
                                Experience = item.api_experience,
                                RankTypeId = item.api_rank,
                                Medals = item.api_medals
                            };

                            if (!players.ContainsKey(data.PlayerId)) {
                                Player pl = new Player() {
                                    PlayerId = data.PlayerId,
                                    Name = item.api_nickname,
                                    ServerId = server.ServerId
                                };
                                players[pl.PlayerId] = pl;
                                database.Players.Add(pl);
                            }

                            if (i == 1) {
                                if (lastData.Keys.Contains(data.PlayerId)) {
                                    data.SetDelta(lastData[data.PlayerId], true);
                                } else {
                                    data.SetDelta();
                                }
                            } else if (lastData.Keys.Contains(data.PlayerId)) {
                                data.SetDelta(lastData[data.PlayerId], false);
                            }



                            listData[data.PlayerId] = data;
                        }
                        database.SenkaDatas.AddRange(listData.Values);
                        lastData = listData;
                    }
                    database.SaveChanges();
                }
            }
            );
            MessageBox.Show("Done!");
        }

        private void BatchParse(string jsonString, ref List<ApiSenkaResult> list) {
            try {
                JObject jsonData = JObject.Parse(jsonString);
                int apiResult = (int)jsonData["api_result"];

                if (apiResult == 1) {
                    var results = jsonData["api_data"]["api_list"].Children();
                    foreach (var result in results) {
                        var senka = JsonConvert.DeserializeObject<ApiSenkaResult>(result.ToString());
                        list.Add(senka);
                    }
                } else {
                    throw new ArgumentException();
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.ToString() + "\r\n" + jsonString);
                throw;
            }
        }

        public class ApiSenkaResult
        {
            public string api_comment { get; set; }
            public string api_comment_id { get; set; }
            public int api_experience { get; set; }
            public string api_flag { get; set; }
            public byte api_level { get; set; }
            public short api_medals { get; set; }
            public int api_member_id { get; set; }
            public string api_nickname { get; set; }
            public string api_nickname_id { get; set; }
            public short api_no { get; set; }
            public byte api_rank { get; set; }
            public short api_rate { get; set; }
        }

        private async void buttonCalcRanking_Click(object sender, EventArgs e) {
            var servers = await _db.Servers.ToListAsync();

            for (int i = 25; i <= 25; i++) {
                using (var database = new SenkaContext()) {
                    var lastData = database.SenkaDatas
                     .Where(data =>
                         data.DateId == i - 1 &&
                         data.RankingAll != null)
                     .ToDictionary(data => data.PlayerId, data => data);

                    var ranking = database.SenkaDatas
                            .Where(data => data.DateId == i)
                            .OrderByDescending(data => data.RankPoint)
                            .ThenByDescending(data => data.Experience)
                            .Take(10000)
                            .ToArray();

                    for (int j = 0; j < ranking.Length; j++) {
                        var data = ranking[j];
                        data.RankingAll = (short)(j + 1);
                        if (lastData.ContainsKey(data.PlayerId)) {
                            data.SetRankAllDelta(lastData[data.PlayerId]);
                        }
                    }
                    database.SaveChanges();
                    lastData = ranking.ToDictionary(d => d.PlayerId, d => d);
                }
            }
            MessageBox.Show("Done!");
        }

        private async void buttonPostTwitter_Click(object sender, EventArgs e) {
            using (var db = new SenkaContext()) {
                var TwitterManager = new TwitterApiManager(db);
                await TwitterManager.PostStatusesUpdateAsync(1, "(単冠湾)");
                await TwitterManager.PostStatusesUpdateAsync(1, "2位 5511 ユウキ＠姫柊雪菜は俺の嫁 (パラオ)");
                await TwitterManager.PostStatusesUpdateAsync(1, "3位 5181 Dark Viper E (佐伯湾)");
                var servers = await db.Servers.Include(s=>s.DateInfo).ToListAsync();
                int dateId = servers[1].LastUpdated;
                if (servers.Any(s=>s.LastUpdated!=dateId)) {
                    MessageBox.Show("Error");
                    return;
                }
                var top3RankPoint = db.SenkaDatas
                    .Include(data => data.Player)
                    .Where(data => data.DateId == dateId)
                    .OrderByDescending(data => data.RankPoint)
                    .ThenByDescending(data => data.Experience)
                    .Take(3)
                    .ToArray();
                if (top3RankPoint.Length == 0) {
                    throw new InvalidOperationException("Error when getting top 3 rank point.");
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0} 戦果ランキング\n", servers[1].DateInfo);
                for (int i = 0; i < 3; i++) {
                    var player = top3RankPoint[i].Player;
                    sb.AppendFormat("{0}位 {1} {2} ({3})\n", i + 1, top3RankPoint[i].RankPoint,
                         player.Name, servers[player.ServerId].NickName);
                }
                sb.Length--;
                await TwitterManager.PostStatusesUpdateAsync(1, sb.ToString());

                var top3RankDelta = db.SenkaDatas
                    .Include(data => data.Player)
                    .Where(data => data.DateId == dateId)
                    .Where(data => data.RankingDelta != null)
                    .OrderByDescending(data => data.RankingDelta)
                    .Take(3)
                    .ToArray();
                if (top3RankDelta.Length == 0) {

                    return;
                }

                sb = new StringBuilder();
                sb.AppendFormat("{0} 戦果増分ランキング\n", servers[1].DateInfo);
                for (int i = 0; i < 3; i++) {
                    var player = top3RankDelta[i].Player;
                    sb.AppendFormat("{0}位 {1} {2} ({3})\n", i + 1, top3RankDelta[i].RankPointDelta,
                         player.Name, servers[player.ServerId].NickName);
                }
                await TwitterManager.PostStatusesUpdateAsync(1, sb.ToString());

                var top3RankEDelta = db.SenkaDatas
                    .Include(data => data.Player)
                    .Where(data => data.DateId == dateId)
                    .Where(data => data.ExperienceDelta != null)
                    .OrderByDescending(data => data.ExperienceDelta)
                    .Take(3)
                    .ToArray();
                if (top3RankEDelta.Length == 0) {
                    return;
                }

                sb = new StringBuilder();
                sb.AppendFormat("{0} 経験値増分ランキング\n", servers[1].DateInfo);
                for (int i = 0; i < 3; i++) {
                    var player = top3RankEDelta[i].Player;
                    sb.AppendFormat("{0}位 {1} {2} ({3})\n", i + 1, top3RankEDelta[i].ExactRankPointDelta,
                         player.Name, servers[player.ServerId].NickName);
                }
                await TwitterManager.PostStatusesUpdateAsync(1, sb.ToString());

                MessageBox.Show("done");
            }
        }
    }
}

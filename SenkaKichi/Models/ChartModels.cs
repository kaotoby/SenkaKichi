using Newtonsoft.Json;
using SenkaKichi.DbModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace SenkaKichi.Models
{
    public class ChartModels
    {
        public static string ConvertToJson(object data) {
            var serializer = new JsonSerializer();
            serializer.ContractResolver = new SenkaContextContractResolver();
#if DEBUG
            serializer.Formatting = Formatting.Indented;
#endif
            var stringWriter = new StringWriter();
            using (var writer = new JsonTextWriter(stringWriter)) {
                writer.QuoteName = false;
                serializer.Serialize(writer, data);
            }
            return stringWriter.ToString();
        }

        public class Export
        {
            public string FileName { get; set; }
            public string Type { get; set; }
            public int Width { get; set; }
            public float Scale { get; set; }
            public string Svg { get; set; }
            public string Domain { get; set; }
            public bool Fill { get; set; }
        }

        public class Player
        {
            public KeyValuePair<short, int?[]> RankPoint { get; set; }
            public KeyValuePair<short, int?[]> RankPointUpper { get; set; }
            public KeyValuePair<short, int?[]> RankPointLower { get; set; }
            public int?[] Ranking { get; set; }
            public int?[] RankPointDeltaAm { get; set; }
            public int?[] RankPointDeltaPm { get; set; }
            public int?[] RankPointDeltaExtra { get; set; }
            public string StartTime { get; set; }
            public string Date { get; set; }
            public string PlayerName { get; set; }
            public string ServerName { get; set; }

            public Player(KeyValuePair<short, Dictionary<int, SenkaData>> playerData, IDictionary<short, Dictionary<int, SenkaData>> boundData) {
                List<int?> RankPointList = new List<int?>();
                List<int?> RankPointUpperList = new List<int?>();
                List<int?> RankPointLowerList = new List<int?>();
                List<int?> RankingList = new List<int?>();
                List<int?> RankPointDeltaAmList = new List<int?>();
                List<int?> RankPointDeltaPmList = new List<int?>();
                List<int?> RankPointDeltaExtraList = new List<int?>();

                int start = boundData.Last().Value.First().Value.DateId;
                int end = boundData.Last().Value.Last().Value.DateId;

                int extraSum = 0;
                for (int i = start; i <= end; i++) {
                    if (playerData.Value.ContainsKey(i)) {
                        RankPointList.Add(playerData.Value[i].RankPoint);
                        RankingList.Add(playerData.Value[i].Ranking);
                        if (i % 2 == 1) {
                            RankPointDeltaAmList.Add(playerData.Value[i].RankPointDeltaExact);
                            extraSum = playerData.Value[i].RankPointDeltaExtra ?? 0;
                        } else {
                            RankPointDeltaPmList.Add(playerData.Value[i].RankPointDeltaExact);
                            extraSum += playerData.Value[i].RankPointDeltaExtra ?? 0;
                            RankPointDeltaExtraList.Add(extraSum == 0 ? null : (int?)extraSum);
                        }
                    } else {
                        RankPointList.Add(null);
                        RankingList.Add(null);
                        if (i % 2 == 1) {
                            extraSum = 0;
                            RankPointDeltaAmList.Add(null);
                        } else {
                            RankPointDeltaPmList.Add(null);
                            RankPointDeltaExtraList.Add(extraSum == 0 ? null : (int?)extraSum);
                        }
                    }
                    if (boundData.Last().Value.ContainsKey(i)) {
                        if (boundData.First().Key != 0) {
                            RankPointUpperList.Add(boundData.First().Value[i].RankPoint);
                        }
                        RankPointLowerList.Add(boundData.Last().Value[i].RankPoint);
                    } else {
                        RankPointUpperList.Add(null);
                        RankPointLowerList.Add(null);
                    }
                }
                if (end % 2 == 1) {
                    RankPointDeltaExtraList.Add(extraSum == 0 ? null : (int?)extraSum);
                }

                Ranking = RankingList.ToArray();

                RankPoint = new KeyValuePair<short, int?[]>(playerData.Key, RankPointList.ToArray());

                if (boundData.First().Key != 0) {
                    RankPointUpper = new KeyValuePair<short, int?[]>(boundData.First().Key, RankPointUpperList.ToArray());
                }
                RankPointLower = new KeyValuePair<short, int?[]>(boundData.Last().Key, RankPointLowerList.ToArray());

                RankPointDeltaAm = RankPointDeltaAmList.ToArray();

                RankPointDeltaPm = RankPointDeltaPmList.ToArray();

                RankPointDeltaExtra = RankPointDeltaExtraList.ToArray();
                if (RankPointDeltaExtra.All(data => data == null)) RankPointDeltaExtra = null;
            }
        }

        public class Server
        {
            public KeyValuePair<short, int?[]>[] RankPoint { get; set; }
            public double[] RankPointDeltaAm { get; set; }
            public double[] RankPointDeltaPm { get; set; }
            public string[] TopName { get; set; }
            public string StartTime { get; set; }
            public string Date { get; set; }
            public string ServerName { get; set; }

            public Server(Dictionary<short, Dictionary<int, SenkaData>> serverData) {
                int start = serverData.First().Value.First().Value.DateId;
                int end = serverData.First().Value.Last().Value.DateId;

                var RankPointList = serverData.ToDictionary(d => d.Key, d => new List<int?>());
                var TopNameList = new List<string>();

                for (int i = start; i <= end; i++) {
                    if (serverData.First().Value.ContainsKey(i)) {
                        foreach (var key in serverData.Keys) {
                            RankPointList[key].Add(serverData[key][i].RankPoint);
                        }
                        TopNameList.Add(serverData[1][i].Player.Name);
                    } else {
                        foreach (var key in serverData.Keys) {
                            RankPointList[key].Add(null);
                        }
                        TopNameList.Add(null);
                    }
                }
                RankPoint = RankPointList.ToDictionary(d => d.Key, d => d.Value.ToArray()).ToArray();

                RankPointDeltaAm = serverData.Select(
                    data =>
                    {
                        var amData = data.Value.Where(d => d.Value.DateInfo.Date.Hour == 3 && d.Value.RankPointDelta != null);
                        return amData.Count() == 0 ? 0 : Math.Round(amData.Average(d => d.Value.RankPointDelta.Value), 1);
                    }).ToArray();

                RankPointDeltaPm = serverData.Select(
                    data =>
                    {
                        var pmData = data.Value.Where(d => d.Value.DateInfo.Date.Hour == 15 && d.Value.RankPointDelta != null);
                        return pmData.Count() == 0 ? 0 : Math.Round(pmData.Average(d => d.Value.RankPointDelta.Value), 1);
                    }).ToArray();

                TopName = TopNameList.ToArray();
            }
        }
    }
}
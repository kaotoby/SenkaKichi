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
            public KeyValuePair<short, short?[]> RankPoint { get; set; }
            public KeyValuePair<short, short[]> RankPointUpper { get; set; }
            public KeyValuePair<short, short[]> RankPointLower { get; set; }
            public short?[] Ranking { get; set; }
            public int?[] RankPointDeltaAm { get; set; }
            public int?[] RankPointDeltaPm { get; set; }
            public int?[] RankPointDeltaExtra { get; set; }
            public string StartTime { get; set; }
            public string Date { get; set; }
            public string PlayerName { get; set; }
            public string ServerName { get; set; }

            public Player(KeyValuePair<short, List<SenkaData>> playerData, IDictionary<short, List<SenkaData>> boundData) {
                KeyValuePair<short, List<SenkaData>> upperPair, lowerPair;

                if (boundData.Count == 1) {
                    lowerPair = boundData.First();
                    RankPointLower = new KeyValuePair<short, short[]>(lowerPair.Key, lowerPair.Value.Select(d => d.RankPoint).ToArray());
                } else {
                    upperPair = boundData.First();
                    lowerPair = boundData.Last();
                    RankPointUpper = new KeyValuePair<short, short[]>(upperPair.Key, upperPair.Value.Select(d => d.RankPoint).ToArray());
                    RankPointLower = new KeyValuePair<short, short[]>(lowerPair.Key, lowerPair.Value.Select(d => d.RankPoint).ToArray());
                }

                var playerDataJoined = from bound in lowerPair.Value
                                       join player in playerData.Value
                                       on bound.DateId equals player.DateId into joined
                                       from data in joined.DefaultIfEmpty()
                                       select new {
                                           Date = bound.DateInfo.Date,
                                           SenkaData = data
                                       };

                Ranking = playerDataJoined
                    .Select(data => data.SenkaData == null ? null : (short?)data.SenkaData.Ranking)
                    .ToArray();

                RankPoint = new KeyValuePair<short, short?[]>(playerData.Key, playerDataJoined
                    .Select(data => data.SenkaData == null ? null : (short?)data.SenkaData.RankPoint)
                    .ToArray());

                RankPointDeltaAm = playerDataJoined
                    .Where(data => data.Date.Hour == 3)
                    .Select(data => data.SenkaData == null ? null
                                    : data.SenkaData.RankPointDelta - (data.SenkaData.RankPointDeltaExtra ?? 0))
                    .ToArray();

                RankPointDeltaPm = playerDataJoined
                    .Where(data => data.Date.Hour == 15)
                    .Select(data => data.SenkaData == null ? null
                                    : data.SenkaData.RankPointDelta - (data.SenkaData.RankPointDeltaExtra ?? 0))
                    .ToArray();

                RankPointDeltaExtra = playerDataJoined
                    .GroupBy(data => data.Date.Day)
                    .Select(group =>
                    {
                        int sum = group.Sum(data => data.SenkaData == null ? 0 : data.SenkaData.RankPointDeltaExtra ?? 0);
                        return sum == 0 ? null : (int?)sum;
                    })
                    .ToArray();
                if (RankPointDeltaExtra.All(data => data == null)) RankPointDeltaExtra = null;
            }
        }

        public class Server
        {
            public KeyValuePair<short, short[]>[] RankPoint { get; set; }
            public double[] RankPointDeltaAm { get; set; }
            public double[] RankPointDeltaPm { get; set; }
            public string[] TopName { get; set; }
            public string StartTime { get; set; }
            public string Date { get; set; }
            public string ServerName { get; set; }

            public Server(Dictionary<short, List<SenkaData>> serverData) {
                RankPoint = serverData.ToDictionary(data => data.Key,
                    data => data.Value.Select(d => d.RankPoint).ToArray())
                    .ToArray();

                RankPointDeltaAm = serverData.Select(
                    data =>
                    {
                        var amData = data.Value.Where(d => d.DateInfo.Date.Hour == 3 && d.RankPointDelta != null);
                        return amData.Count() == 0 ? 0 : Math.Round(amData.Average(d => d.RankPointDelta.Value), 1);
                    }).ToArray();

                RankPointDeltaPm = serverData.Select(
                    data =>
                    {
                        var pmData = data.Value.Where(d => d.DateInfo.Date.Hour == 15 && d.RankPointDelta != null);
                        return pmData.Count() == 0 ? 0 : Math.Round(pmData.Average(d => d.RankPointDelta.Value), 1);
                    }).ToArray();

                TopName = serverData[1].Select(data => data.Player.Name).ToArray();
            }
        }
    }
}
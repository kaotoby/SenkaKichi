using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using SenkaKichi.DbModels;
using SenkaKichi.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SenkaKichi.Models
{
    public class SenkaRepository
    {
        public static Dictionary<int, Server> Servers { get; private set; }

        private SenkaContext _db;

        public SenkaRepository(SenkaContext database) {
            _db = database;
        }

        /// <summary>
        ///     Cache servers infomation
        /// </summary>
        public static void Startup() {
            using (var db = new SenkaContext()) {
                Servers = db.Servers
                    .Include(server => server.DateInfo)
                    .ToDictionary(server => (int)server.ServerId, server => server);
            }
        }

        /// <summary>
        ///     Get the server last updated date
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns></returns>
        public async Task<DateInfo> GetServerLastUpdatedAsync(int serverId) {
            // No cache
            var server = await _db.Servers
                    .Include(s => s.DateInfo)
                    .FirstOrDefaultAsync(s => s.ServerId == (byte)serverId);
            if (Servers[serverId].LastUpdated != server.LastUpdated) {
                Servers[serverId] = server;
            }
            return server.DateInfo;
        }

        /// <summary>
        ///     Get all the server last updated date
        /// </summary>
        /// <returns></returns>
        public async Task<DateInfo> GetAllServerLastUpdatedAsync() {
            // No cache
            int min = await _db.Servers.MinAsync(s => s.LastUpdated);
            return await _db.DateInfoes.FindAsync(min);
        }

        /// <summary>
        ///     Get last player data
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public Task<SenkaData> GetPlayerLastDataAsync(int playerId) {
            // No cache
            return _db.SenkaDatas
                .Include(data => data.DateInfo)
                .Include(data => data.Player.Server)
                .Where(data => data.PlayerId == playerId)
                .OrderByDescending(data => data.DateId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        ///     Get last player data
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public Task<SenkaData> GetPlayerLastDataAsync(int playerId, DateTime start) {
            DateTime end = start.AddMonths(1).AddHours(-12);
            // No cache
            return _db.SenkaDatas
                .Include(data => data.DateInfo)
                .Include(data => data.Player.Server.DateInfo)
                .Where(data =>
                    data.PlayerId == playerId &&
                    data.DateInfo.Date >= start &&
                    data.DateInfo.Date <= end)
                .OrderByDescending(data => data.DateId)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        ///     Gets player datas
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public Task<List<SenkaData>> GetPlayerInfoAsync(int playerId, DateInfo start, int endDateId) {
            //return SenkaCache.GetAndCache(() =>
            //{
                return _db.SenkaDatas
                    .Include(data => data.DateInfo)
                    .Where(data =>
                        data.PlayerId == playerId &&
                        data.DateId >= start.DateId &&
                        data.DateId <= endDateId)
                    .OrderBy(data => data.DateId)
                    .ToListAsync();
            //});
        }

        /// <summary>
        ///     Gets server ranking bound datas with specific ranking
        /// </summary>
        /// <param name="ranking"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public async Task<IDictionary<short, List<SenkaData>>> GetServerRankingBoundAsync(int serverId, short ranking, DateInfo start) {
            short upper = 0, lower = 0;
            if (ranking == 1) {
                upper = 0;
                lower = 5;
            } else if (ranking <= 5) {
                upper = 1;
                lower = 5;
            } else if (ranking <= 20) {
                upper = 5;
                lower = 20;
            } else if (ranking <= 100) {
                upper = 20;
                lower = 100;
            } else if (ranking <= 500) {
                upper = 100;
                lower = 500;
            } else {
                upper = 500;
                lower = 990;
            }
            var bound = new SortedDictionary<short, List<SenkaData>>();
            int endDateId = CalcuateEndDateId(start);
            bound[upper] = await GetServerRankingBoundAsync(serverId, upper, start.DateId, endDateId);
            bound[lower] = await GetServerRankingBoundAsync(serverId, lower, start.DateId, endDateId);
            return bound;
        }

        private Task<List<SenkaData>> GetServerRankingBoundAsync(int serverId, short ranking, int start, int end) {
            //return SenkaCache.GetAndCache(() =>
            //{
                return _db.SenkaDatas
                    .Include(data => data.DateInfo)
                    .Where(data =>
                        data.DateId >= start &&
                        data.DateId <= end &&
                        data.Player.ServerId == (byte)serverId &&
                        data.Ranking == ranking)
                    .OrderBy(data => data.DateId)
                    .ToListAsync();
            //});
        }

        /// <summary>
        ///     Gets player activities by specific end date and count
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="end"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public Task<List<ActivityData>> GetPlayerActivityAsync(int playerId, DateInfo end, int take) {
            //return SenkaCache.GetAndCache(() =>
            //{
                return _db.SenkaDatas
                    .Where(data =>
                        data.PlayerId == playerId &&
                        data.DateId <= end.DateId)
                    .GroupBy(data => new {
                        Level = data.Level,
                        Comment = data.Comment
                    })
                    .Select(group => new ActivityData {
                        Date = group.OrderBy(data => data.DateId).FirstOrDefault().DateInfo,
                        Level = group.Key.Level,
                        Comment = group.Key.Comment
                    })
                    .OrderByDescending(data => data.Date.DateId)
                    .Take(take)
                    .ToListAsync();
            //});
        }

        public async Task<Dictionary<short, List<SenkaData>>> GetServerInfoAsync(int serverId, DateInfo start, int endDateId) {
            //return SenkaCache.GetAndCache(async () =>
            //{
                var raw = await _db.SenkaDatas
                    .Include(data => data.Player)
                    .Include(data => data.DateInfo)
                    .Where(data =>
                        data.Player.ServerId == (byte)serverId &&
                        data.DateId >= start.DateId &&
                        data.DateId <= endDateId &&
                        (data.Ranking == 1 ||
                        data.Ranking == 5 ||
                        data.Ranking == 20 ||
                        data.Ranking == 100 ||
                        data.Ranking == 500))
                    .ToArrayAsync();
                return raw.GroupBy(data => data.Ranking)
                    .OrderBy(group => group.Key)
                    .ToDictionary(
                        group => group.Key, 
                        group => group.OrderBy(d => d.DateId).ToList());
            //});
        }

        /// <summary>
        ///     Find DateInfo with DateTime
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public Task<DateInfo> FindDateInfoByDateAsync(DateTime date) {
            //return SenkaCache.GetAndCache(48 * 60 * 60, () =>
            //{
                return _db.DateInfoes
                    .FirstOrDefaultAsync(info => info.Date == date);
            //});
        }

        /// <summary>
        ///     Gets ranking info of a server
        /// </summary>
        /// <param name="serverId"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public Task<List<SenkaData>> GetServerRankingAsync(int serverId, DateInfo info) {
            //return SenkaCache.GetAndCache(() =>
            //{
                return _db.SenkaDatas
                    .Include(data => data.Player)
                    .Include(data => data.DateInfo)
                    .Where(data =>
                        data.DateId == info.DateId &&
                        data.Player.ServerId == (byte)serverId)
                    .OrderBy(data => data.Ranking)
                    .ToListAsync();
            //});
        }

        /// <summary>
        ///     Gets all ranking info of a server
        /// </summary>
        /// <param name="info"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public Task<List<SenkaData>> GetAllServerRankingAsync(DateInfo info, int skip, int take) {
            //return SenkaCache.GetAndCache(() =>
            //{
                return _db.SenkaDatas
                    .Include(data => data.Player.Server)
                    .Include(data => data.DateInfo)
                    .Where(data =>
                        data.DateId == info.DateId &&
                        data.RankingAll != null)
                    .OrderBy(data => data.RankingAll)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            //});
        }

        /// <summary>
        ///     Gets all delta ranking info of a server
        /// </summary>
        /// <param name="info"></param>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        public Task<List<SenkaData>> GetAllServerDeltaRankingAsync(DateInfo info, int skip, int take) {
            //return SenkaCache.GetAndCache(() =>
            //{
                return _db.SenkaDatas
                    .Include(data => data.Player)
                    .Include(data => data.DateInfo)
                    .Where(data =>
                        data.DateId == info.DateId &&
                        data.ExperienceDelta != null)
                    .OrderByDescending(data => data.ExperienceDelta)
                    .Skip(skip)
                    .Take(take)
                    .ToListAsync();
            //});
        }

        /// <summary>
        ///     Search player by player id, player name and server name
        /// </summary>
        /// <param name="query"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public Task<PlayerSearchResult[]> SearchPlayerAsync(string query, int serverId, DateInfo date, int page) {
            //return SenkaCache.GetAndCache(2 * 60 * 60, () =>
            //{
                var keywords = query.Split(' ');

                IQueryable<Player> filter = _db.Players;
                if (serverId != 0) {
                    filter = _db.Players.Where(player => player.ServerId == serverId);
                }

                return filter
                    .Where(player =>
                        keywords.All(word => player.Name.Contains(word))
                    )
                    .OrderBy(result =>
                        keywords.Sum(word => result.Name.IndexOf(word))
                    )
                    .Skip((page - 1) * 20)
                    .Take(20)
                    .Select(player => new {
                        Player = player,
                        Data = player.SenkaDatas
                                .OrderByDescending(data => data.DateId)
                                .FirstOrDefault()
                    })
                    .Select(data => new PlayerSearchResult {
                        DateInfo = data.Data.DateInfo,
                        Player = data.Player,
                        Comment = data.Data.Comment,
                        Ranking = data.Data.Ranking
                    })
                    .ToArrayAsync();
            //});
        }

        /// <summary>
        ///     Search player for suggest by player id, player name and server name
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public Task<PlayerSuggestResult[]> SearchSuggestPlayerAsync(string query, int serverId, DateInfo date) {
            //return SenkaCache.GetAndCache(() =>
            //{
                var keywords = query.Split(' ');

                IQueryable<Player> filter = _db.Players;
                if (serverId != 0) {
                    filter = _db.Players.Where(player => player.ServerId == serverId);
                }

                return filter
                    .Where(player =>
                        keywords.All(word => player.Name.Contains(word))
                    )
                    .OrderBy(result =>
                        keywords.Sum(word => result.Name.IndexOf(word))
                    )
                    .Take(8)
                    .Select(player => new PlayerSuggestResult {
                        Server = player.Server.NickName,
                        Name = player.Name,
                        Comment = player.SenkaDatas
                                .OrderByDescending(data => data.DateId)
                                .FirstOrDefault()
                                .Comment,
                        Id = player.PlayerId.ToString()
                    })
                    .ToArrayAsync();
            //});
        }

        public int CalcuateEndDateId(DateInfo start) {
            return start.DateId + DateTime.DaysInMonth(start.Date.Year, start.Date.Month) * 2 - 1;
        }
    }

    //public class SenkaCache
    //{
    //    private static readonly ObjectCache Cache = MemoryCache.Default;
    //    private static readonly string NULL = Guid.NewGuid().ToString("N");

    //    public static Task<TResult> GetAndCache<TResult>(Func<Task<TResult>> func) {
    //        return GetAndCache(13 * 60 * 60, func);
    //    }

    //    public async static Task<TResult> GetAndCache<TResult>(long second, Func<Task<TResult>> func) {
    //        Regex reg = new Regex("<(.+)>");
    //        var fields = func.Target.GetType().GetFields().Reverse().Skip(1);
    //        string callerName = reg.Match(func.Method.Name).Groups[1].Value;

    //        StringBuilder keyBuilder = new StringBuilder();
    //        keyBuilder.AppendFormat("Name:{0}#", callerName);
    //        keyBuilder.Append("Param:");
    //        keyBuilder.Append("{");
    //        foreach (var field in fields) {
    //            keyBuilder.AppendFormat("{{{0}:{1}}}", field.Name, field.GetValue(func.Target));
    //        }
    //        keyBuilder.Append("}");
    //        string key = keyBuilder.ToString();

    //        object value = Cache.Get(key);
    //        if (value == null) {
    //            TResult result = await func();
    //            if (result == null) {
    //                Cache.Set(key, NULL, DateTime.Now.AddSeconds(second));
    //            } else {
    //                Cache.Set(key, result, DateTime.Now.AddSeconds(second));
    //            }
    //            return result;
    //        } else if (value.ToString() == NULL) {
    //            return default(TResult);
    //        } else {
    //            return (TResult)value;
    //        }
    //    }
    //}
}
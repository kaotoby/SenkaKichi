using Newtonsoft.Json;
using SenkaKichi.DbModels;
using SenkaKichi.Models;
using System.Collections.Generic;

namespace SenkaKichi.ViewModels
{
    public class PlayerSearchResult
    {
        [JsonIgnore]
        public DateInfo DateInfo { get; set; }
        public DbModels.Player Player { get; set; }
        public short Ranking { get; set; }
        public string Comment { get; set; }
        public string Date {
            get {
                return this.DateInfo.ToString();
            }
        }
        public string Server {
            get {
                return SenkaRepository.Servers[this.Player.ServerId].NickName;
            }
        }
    }

    public class PlayerSuggestResult
    {
        public string Name { get; set; }
        public string Server { get; set; }
        public string Comment { get; set; }
        public string Id { get; set; }
    }
}
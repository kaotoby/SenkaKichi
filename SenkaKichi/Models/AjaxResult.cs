using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SenkaKichi.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace SenkaKichi.Models
{
    public class AjaxResult<TResult> : ActionResult
    {
        public bool Success { get; set; }
        public TResult Data { get; set; }

        public override void ExecuteResult(ControllerContext context) {
            if (context == null) throw new ArgumentNullException("context");

            var response = context.HttpContext.Response;
            response.ContentType = "application/json";
            response.ContentEncoding = Encoding.UTF8;

            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = new SenkaContextContractResolver();
#if DEBUG
            serializerSettings.Formatting = Formatting.Indented;
#endif
            response.Write(JsonConvert.SerializeObject(Data, serializerSettings));
        }

        public AjaxResult() { }

        public AjaxResult(bool success) {
            Success = success;
        }

        public AjaxResult(bool success, TResult data) {
            Success = success;
            Data = data;
        }
    }

    public class PlayerSearchResult
    {
        [JsonIgnore]
        public DateInfo DateInfo { get; set; }
        public Player Player { get; set; }
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

    public class SenkaContextContractResolver : DefaultContractResolver
    {
        private static Regex _req = new Regex(@"((?<=.)[A-Z][a-zA-Z]*)|((?<=[a-zA-Z])\d+)");

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization) {
            return base.CreateProperties(type, memberSerialization)
                .Where(property =>
                {
                    var p = type.GetProperty(property.PropertyName);
                    if (p == null || p.GetMethod == null || p.GetMethod.IsVirtual) {
                        return false;
                    }
                    return true;
                })
                .ToList();
        }
    }
}
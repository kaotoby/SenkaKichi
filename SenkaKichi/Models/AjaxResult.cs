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
        public bool success { get; set; }
        public TResult data { get; set; }

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
            response.Write(JsonConvert.SerializeObject(this, serializerSettings));
        }

        public AjaxResult() { }

        public AjaxResult(bool success) {
            this.success = success;
        }

        public AjaxResult(bool success, TResult data) {
            this.success = success;
            this.data = data;
        }
    }

    public class SenkaContextContractResolver : DefaultContractResolver
    {
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
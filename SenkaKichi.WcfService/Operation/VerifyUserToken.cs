using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SenkaKichi.DbModels;
using SenkaKichi.WcfService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;

namespace SenkaKichi.WcfService
{
    public partial class ServiceManager
    {
        public ServiceResult VerifyUserToken(int userId) {
            using (var db = new SenkaContext()) {
                AspNetUser user = db.AspNetUsers.Include(u => u.Player).FirstOrDefault(u => u.Id == userId);
                Player player = user.Player;
                SenkaData data = player.SenkaDatas.OrderByDescending(d => d.DateId).First();
                ServerInfo info = Servers[player.ServerId];
                if (!info.Enabled || info.IsUpdating) {
                    return ServiceResult.Unavailable;
                }

                if (data.Comment == user.PlayerVerifyToken) {
                    user.IsPlayerVerified = true;
                    user.PlayerVerifyEndTime = null;
                    db.SaveChanges();
                    return ServiceResult.Success;
                }

                user.PlayerVerifyEndTime = DateTime.Now.AddHours(2);
                db.SaveChanges();

                string jsonResult = "";
                var postDic = new Dictionary<string, object> {
                    { "api_pageno", data.Ranking / 10 + 1 },
                    { "api_verno", 1 },
                    { "api_token", info.ApiToken }
                };
                info.HttpHelper.CTRHttp(info.FullPath, info.SwfReferer, postDic, ref jsonResult);

                jsonResult = jsonResult.Replace("svdata=", "");
                JObject jsonData = JObject.Parse(jsonResult);
                int apiResult = (int)jsonData["api_result"];

                if (apiResult == 1) {
                    var results = jsonData["api_data"]["api_list"].Children();
                    foreach (var result in results) {
                        var senka = JsonConvert.DeserializeObject<ApiSenkaResult>(result.ToString());
                        if (senka.api_member_id == user.PlayerId) {
                            if (senka.api_comment == user.PlayerVerifyToken) {
                                user.IsPlayerVerified = true;
                                user.PlayerVerifyEndTime = null;
                                db.SaveChanges();
                                return ServiceResult.Success;
                            } else {
                                return ServiceResult.Fail;
                            }
                        }
                    }
                }
            }
            return ServiceResult.UnknowError;
        }
    }
}

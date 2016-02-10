using MvcSiteMapProvider;
using SenkaKichi.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SenkaKichi.SiteMap.Server
{
    public class InfoDynamicNodeProvider : DynamicNodeProviderBase
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node) {
            using (var db = new SenkaContext()) {
                foreach (var server in db.Servers) {
                    DynamicNode dynamicNode = new DynamicNode("Server" + server.ServerId.ToString(), server.Name);
                    dynamicNode.ChangeFrequency = ChangeFrequency.Daily;
                    dynamicNode.RouteValues.Add("id", server.ServerId);
                    dynamicNode.ParentKey = "Home";
                    dynamicNode.Protocol = "https";

                    yield return dynamicNode;
                }
            }

            DynamicNode serverNode = new DynamicNode("Server", "全サーバ");
            serverNode.ChangeFrequency = ChangeFrequency.Daily;
            serverNode.RouteValues.Add("id", 0);
            serverNode.ParentKey = "Home";
            serverNode.Protocol = "https";
            yield return serverNode;
        }
    }

    public class RankingDynamicNodeProvider : DynamicNodeProviderBase
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node) {
            using (var db = new SenkaContext()) {
                var servers = db.Servers.ToArray();
                foreach (var server in servers) {
                    DynamicNode dynamicNode = new DynamicNode("Ranking" + server.ServerId.ToString(), string.Format("ランキング {0}", server.Name));
                    dynamicNode.ChangeFrequency = ChangeFrequency.Hourly;
                    dynamicNode.UpdatePriority = UpdatePriority.High;
                    dynamicNode.RouteValues.Add("id", server.ServerId);
                    dynamicNode.ParentKey = "Server" + server.ServerId.ToString();
                    dynamicNode.Protocol = "https";

                    yield return dynamicNode;
                }
            }

            DynamicNode serverNode = new DynamicNode("Ranking", "ランキング (全サーバ)");
            serverNode.ChangeFrequency = ChangeFrequency.Daily;
            serverNode.RouteValues.Add("id", 0);
            serverNode.ParentKey = "Server";
            serverNode.Protocol = "https";
            yield return serverNode;
        }
    }
}
using MvcSiteMapProvider;
using SenkaKichi.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace SenkaKichi.SiteMap.Player
{
    public class InfoDynamicNodeProvider : DynamicNodeProviderBase
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node) {
            using (var db = new SenkaContext()) {
                var players = db.Players.Include(p => p.Server);
                foreach (var player in players) {
                    DynamicNode dynamicNode = new DynamicNode("Player" + player.PlayerId.ToString(), player.Name);
                    dynamicNode.ParentKey = "Server" + player.ServerId.ToString();
                    dynamicNode.ChangeFrequency = ChangeFrequency.Daily;
                    dynamicNode.RouteValues.Add("id", player.PlayerId);
                    dynamicNode.Protocol = "https";

                    yield return dynamicNode;
                }
            }
        }
    }

    public class ActivityDynamicNodeProvider : DynamicNodeProviderBase
    {
        public override IEnumerable<DynamicNode> GetDynamicNodeCollection(ISiteMapNode node) {
            using (var db = new SenkaContext()) {
                var players = db.Players.Include(p => p.Server);
                foreach (var player in players) {
                    DynamicNode dynamicNode = new DynamicNode("Activity" + player.PlayerId.ToString(), player.Name);
                    dynamicNode.ParentKey = "Player" + player.PlayerId.ToString();
                    dynamicNode.ChangeFrequency = ChangeFrequency.Daily;
                    dynamicNode.RouteValues.Add("id", player.PlayerId);
                    dynamicNode.Protocol = "https";

                    yield return dynamicNode;
                }
            }
        }
    }
}

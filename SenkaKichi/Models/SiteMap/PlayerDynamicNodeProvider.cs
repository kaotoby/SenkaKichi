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
                foreach (var player in db.Players) {
                    DynamicNode dynamicNode = new DynamicNode("Player" + player.PlayerId.ToString(), player.Name);
                    dynamicNode.Description = string.Format("{0}({1})の最新の艦これ戦果情報と分析。", player.Name, player.Server.Name);
                    dynamicNode.ParentKey = "Server" + player.ServerId.ToString();
                    dynamicNode.ChangeFrequency = ChangeFrequency.Daily;
                    dynamicNode.UpdatePriority = UpdatePriority.Absolute_040; //Below Normal
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
                foreach (var player in db.Players) {
                    DynamicNode dynamicNode = new DynamicNode("Activity" + player.PlayerId.ToString(), player.Name);
                    dynamicNode.Description = string.Format("{0}({1})のアクティビティ。", player.Name, player.Server.Name);
                    dynamicNode.ParentKey = "Player" + player.PlayerId.ToString();
                    dynamicNode.ChangeFrequency = ChangeFrequency.Daily;
                    dynamicNode.UpdatePriority = UpdatePriority.Absolute_040; //Below Normal
                    dynamicNode.RouteValues.Add("id", player.PlayerId);
                    dynamicNode.Protocol = "https";
                    dynamicNode.VisibilityProvider = "MvcSiteMapProvider.FilteredSiteMapNodeVisibilityProvider, MvcSiteMapProvider";

                    yield return dynamicNode;
                }
            }
        }
    }
}

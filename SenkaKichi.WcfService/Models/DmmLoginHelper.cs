using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using SenkaKichi.DbModels;
using log4net;

namespace SenkaKichi.WcfService.Models
{
    public class DmmLoginHelper
    {
        private ServerAuthorize _server;
        private HttpHelper _helper;
        private static ILog log = LogManager.GetLogger(typeof(DmmLoginHelper).FullName);

        private const string ServerConstPath = "/gadget/js/kcs_const.js";
        private const string ApiPath = "/kcsapi/api_auth_member/dmmlogin/";
        private const string LoginPagePath = "Sg9VTQFXDFcX";
        private const string LoginPage = "https://www.dmm.com/my/-/login/=/path=" + LoginPagePath;
        private const string LoginAjax = "https://www.dmm.com/my/-/login/ajax-get-token/";
        private const string LoginAuth = "https://www.dmm.com/my/-/login/auth/";
        private const string NetGamePage = "http://www.dmm.com/netgame/";
        private const string GamePage = NetGamePage + "social/-/gadgets/=/app_id=854854/";
        private const string GadgetPage = "http://osapi.dmm.com/gadgets/makeRequest";
        private string JSPage = "";
        private string IfreamPage = "";
        private string IfreamReferer = "";

        public DmmLoginHelper(ServerAuthorize serverAuthorizeData, HttpHelper helper) {
            _server = serverAuthorizeData;
            _helper = helper;
            InitializeCookies();
        }

        public Dictionary<byte, string> GetIp() {
            var dic = new Dictionary<byte, string>();
            string pageToken, dmmToken;
            DmmAjaxResult formToken;

            GetPageTokens(out pageToken, out dmmToken);
            formToken = GetFormTokens(pageToken, dmmToken);
            LoginDMM(formToken);

            string gameResult = "", jsResult = "", userid;
            _helper.CTRHttp(GamePage, NetGamePage, ref gameResult);

            Regex ifreamReg = new Regex("name=\"game_frame\" src=\"((.+)#rpctoken=\\d+)\" width");
            Regex xmlReg = new Regex("url=(http://(.+)/gadget.xml)");
            Regex stReg = new Regex("ST\\s+: \"(.+)\"");
            Regex useridReg = new Regex("OWNER_ID\\s+: (\\d+),");
            Regex ipReg = new Regex("ConstServerInfo.World_(\\d+)\\s+= \"http://(.+)/\";");
            Match ifreamMatch = ifreamReg.Match(gameResult);
            IfreamReferer = ifreamMatch.Groups[1].Value;
            IfreamPage = ifreamMatch.Groups[2].Value;
            userid = useridReg.Match(gameResult).Groups[1].Value;

            Match xmlMatch = xmlReg.Match(Uri.UnescapeDataString(IfreamPage));
            JSPage = "http://" + xmlMatch.Groups[2].Value + ServerConstPath;
            _helper.CTRHttp(JSPage, IfreamReferer, ref jsResult);
            foreach (Match match in ipReg.Matches(jsResult)) {
                try {
                    byte id = byte.Parse(match.Groups[1].Value);
                    string ip = match.Groups[2].Value;
                    dic[id] = ip;
                    log.Info(string.Format("[ServerId {0}] Ip address updated. Ip = {1}", id, ip));
                } catch (Exception) { }
	        }
            return dic;
        }

        public string GetToken() {
            string pageToken, dmmToken;
            string gadgetURL, gadgetST, gadgetGADGET;
            DmmAjaxResult formToken;

            GetPageTokens(out pageToken, out dmmToken);
            formToken = GetFormTokens(pageToken, dmmToken);
            LoginDMM(formToken);
            GetGadgetTokens(out gadgetURL, out gadgetST, out gadgetGADGET);

            var postDic = new Dictionary<string, object>();
            var customHeader = new Dictionary<string, string>();
            customHeader["Cache-Control"] = "no-cache";
            customHeader["Pragma"] = "no-cache";
            postDic["url"] = gadgetURL;
            postDic["httpMethod"] = "GET";
            postDic["headers"] = "";
            postDic["postData"] = "";
            postDic["authz"] = "signed";
            postDic["st"] = gadgetST;
            postDic["contentType"] = "JSON";
            postDic["numEntries"] = "3";
            postDic["getSummaries"] = "false";
            postDic["signOwner"] = "true";
            postDic["signViewer"] = "true";
            postDic["gadget"] = gadgetGADGET;
            postDic["container"] = "dmm";
            postDic["bypassSpecCache"] = "";
            postDic["getFullHeaders"] = "false";
            postDic["oauthState"] = "";
            
            string jsonResult = "";
            _helper.CTRHttp(GadgetPage, IfreamReferer, postDic, customHeader, ref jsonResult);
            jsonResult = jsonResult.Substring(jsonResult.IndexOf('{'));
            string bodyData = (string)JObject.Parse(jsonResult).First.First["body"];
            JObject jsonData = JObject.Parse(bodyData.Replace("svdata=", ""));
            string token = (string)jsonData["api_token"];
            if (!string.IsNullOrEmpty(token)) {
                log.Debug(string.Format("[ServerId {0}] GetToken succeed.", _server.ServerId));
            }
            return token;
        }

        private void GetPageTokens(out string pageToken, out string dmmToken) {
            string result = "";
            Regex regPageToken = new Regex("\"token\": \"([0-9a-f]+)\"");
            Regex regDmmToken = new Regex("\"DMM_TOKEN\", \"([0-9a-f]+)\"");
            _helper.CTRHttp(LoginPage, ref result);
            pageToken = regPageToken.Match(result).Groups[1].Value;
            dmmToken = regDmmToken.Match(result).Groups[1].Value;

            log.Debug(string.Format("[ServerId {0}] GetPageTokens done", _server.ServerId));
        }

        private DmmAjaxResult GetFormTokens(string pageToken, string dmmToken) {
            var postDic = new Dictionary<string, object>();
            var customHeader = new Dictionary<string, string>();
            customHeader["X-Requested-With"] = "XMLHttpRequest";
            customHeader["DMM_TOKEN"] = dmmToken;
            postDic["token"] = pageToken;

            string jsonResult = "";
            _helper.CTRHttp(LoginAjax, LoginPage, postDic, customHeader, ref jsonResult);
            DmmAjaxResult ajaxResult = JsonConvert.DeserializeObject<DmmAjaxResult>(jsonResult);

            return ajaxResult;
        }

        private void LoginDMM(DmmAjaxResult formToken) {
            string loginResult = "";
            var postDic = new Dictionary<string, object>();
            postDic["token"] = formToken.token;
            postDic["login_id"] = _server.Username;
            postDic["save_login_id"] = "0";
            postDic["password"] = _server.Password;
            postDic["save_password"] = "0";
            postDic["use_auto_login"] = "0";
            postDic[formToken.login_id] = _server.Username;
            postDic[formToken.password] = _server.Password;
            postDic["path"] = LoginPagePath;
            postDic["prompt"] = "";
            postDic["client_id"] = "";
            postDic["display"] = "";

            _helper.CTRHttp(LoginAuth, LoginPage, postDic, ref loginResult);
            log.Debug(string.Format("[ServerId {0}] LoginDMM request sent", _server.ServerId));
        }

        private void GetGadgetTokens(out string url, out string st, out string gadget) {
            string gameResult = "", jsResult = "", ipaddress, userid;
            _helper.CTRHttp(GamePage, NetGamePage, ref gameResult);

            Regex ifreamReg = new Regex("name=\"game_frame\" src=\"((.+)#rpctoken=\\d+)\" width");
            Regex xmlReg = new Regex("url=(http://(.+)/gadget.xml)");
            Regex stReg = new Regex("ST\\s+: \"(.+)\"");
            Regex useridReg = new Regex("OWNER_ID\\s+: (\\d+),");
            Regex ipReg = new Regex(string.Format("ConstServerInfo.World_{0}\\s+= \"http://(.+)/\";", _server.ServerId));
            Match ifreamMatch = ifreamReg.Match(gameResult);
            IfreamReferer = ifreamMatch.Groups[1].Value;
            IfreamPage = ifreamMatch.Groups[2].Value;
            userid = useridReg.Match(gameResult).Groups[1].Value;

            Match xmlMatch = xmlReg.Match(Uri.UnescapeDataString(IfreamPage));
            JSPage = "http://" + xmlMatch.Groups[2].Value + ServerConstPath;
            _helper.CTRHttp(JSPage, IfreamReferer, ref jsResult);
            ipaddress = ipReg.Match(jsResult).Groups[1].Value;

            gadget = xmlMatch.Groups[1].Value;
            st = stReg.Match(gameResult).Groups[1].Value;
            url = string.Format("http://{0}{1}{2}/1/{3}", ipaddress, ApiPath, userid, DateTime.UtcNow.ToUnixTimestamp());
        }

        private void InitializeCookies() {
            Cookie[] Cookies = new[] {
                new Cookie("cklg", "ja")
            };

            foreach (var cookie in Cookies) {
                cookie.Domain = "dmm.com";
                cookie.Expires = cookie.TimeStamp.AddYears(1);
                _helper.Coa.Add(cookie);
            }
        }
    }

    public class DmmAjaxResult
    {
        public string token { get; set; }
        public string login_id { get; set; }
        public string password { get; set; }
    }
}

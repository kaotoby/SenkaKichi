using log4net;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SenkaKichi.WcfService.Models
{
    public class HttpHelper
    {
        public CookieContainer Coa { get; set; }

        private static readonly ILog log = LogManager.GetLogger(typeof(HttpHelper).FullName);

        public HttpHelper() {
            Coa = new CookieContainer();
        }

        public void CTRHttp(string url, ref string result) {
            CTRHttp(url, null, null, null, ref result);
        }

        public void CTRHttp(string url, string referer, ref string result) {
            CTRHttp(url, referer, null, null, ref result);
        }

        public void CTRHttp(string url, string referer, IDictionary<string, object> postDict, ref string result) {
            CTRHttp(url, referer, postDict, null, ref result);
        }

        public void CTRHttp(string url, string referer, IDictionary<string, object> postDict, IDictionary<string, string> customHeader, ref string result) {
            GC.Collect();
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.KeepAlive = true;
            req.AllowAutoRedirect = true;
            req.Accept = "*/*";
            req.AllowWriteStreamBuffering = true;
            req.Credentials = CredentialCache.DefaultCredentials;
            req.MaximumResponseHeadersLength = -1;
            req.ProtocolVersion = HttpVersion.Version10;
            req.Headers.Add("Accept-Language", "ja-jp");
            req.Headers.Add("Accept-Encoding", "gzip, deflate");
            req.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            req.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; rv:34.0) Gecko/20100101 Firefox/34.0";
            req.Proxy = null;
            req.CookieContainer = Coa;
            if (referer != null) req.Referer = referer;
            if (customHeader != null) {
                foreach (var pair in customHeader) {
                    req.Headers.Add(pair.Key, pair.Value);
                }
            }
            #if DEBUG
            req.Proxy = new WebProxy("few.moe", 52333);
            #endif

            try {
                if (postDict != null) {
                    string postDataStr = "";
                    req.Method = "POST";
                    req.ContentType = "application/x-www-form-urlencoded";
                    postDataStr = quoteParas(postDict);
                    byte[] postBytes = Encoding.UTF8.GetBytes(postDataStr);
                    req.ContentLength = postBytes.Length;
                    Stream postDataStream = req.GetRequestStream();
                    postDataStream.Write(postBytes, 0, postBytes.Length);
                    postDataStream.Close();
                } else {
                    req.Method = "GET";
                }

                using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
                using (Stream stream = resp.GetResponseStream()) {
                    var sr = new StreamReader(stream, Encoding.GetEncoding("UTF-8"));
                    Coa.ToList();
                    result = WebUtility.HtmlDecode(sr.ReadToEnd());
                }
            } catch (WebException ex) {
                log.Error("An Error Occurred During HttpWebRequest", ex);
                throw;
            }
        }

        private string quoteParas(IDictionary<string, object> paras) {
            var paraBuilder = new StringBuilder();
            foreach (var para in paras) {
                paraBuilder.AppendFormat("{0}={1}&", para.Key, Uri.EscapeDataString(para.Value.ToString()));
            }
            paraBuilder.Length--;

            return paraBuilder.ToString(); ;
        }
    }

    public static class CookieContainerExtension
    {
        public static List<Cookie> ToList(this CookieContainer container) {
            var cookies = new List<Cookie>();

            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable",
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.GetField |
                                                                    BindingFlags.Instance,
                                                                    null,
                                                                    container,
                                                                    new object[] { });

            foreach (var key in table.Keys) {

                Uri uri = null;

                var domain = key as string;

                if (domain == null)
                    continue;

                if (domain.StartsWith("."))
                    domain = domain.Substring(1);

                var address = string.Format("http://{0}/", domain);

                if (Uri.TryCreate(address, UriKind.RelativeOrAbsolute, out uri) == false)
                    continue;

                foreach (Cookie cookie in container.GetCookies(uri)) {
                    cookies.Add(cookie);
                }
            }

            return cookies;
        }
    }
}

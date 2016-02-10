using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Newtonsoft.Json;
using SenkaKichi.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace SenkaKichi.OAuthApi.Twitter
{
    public class TwitterApiManager : IOAuthApiManager
    {
#if DEBUG
        // 戦果基地Debug https://apps.twitter.com/app/8177786
        //public const string ConsumerKey = "SSrmgTBR577Q5xRyccp6o4Unv";
        //public const string ConsumerSecret = "WGi1ePvHDVG9zjZa8Y2zQLk6B5Dcqj8HFU5Ns5FemBpnOqgstL";
        public const string ConsumerKey = "uDx3MGVi7wOCBTvVGapFCeQ59";
        public const string ConsumerSecret = "zcUYtS5gXsxNmbSdfVrAXkMAVgAk3cX7U8pXeuMWSMhczl9nP1";
#else
        // 戦果基地 https://apps.twitter.com/app/8177780
        public const string ConsumerKey = "uDx3MGVi7wOCBTvVGapFCeQ59";
        public const string ConsumerSecret = "zcUYtS5gXsxNmbSdfVrAXkMAVgAk3cX7U8pXeuMWSMhczl9nP1";
#endif
        public const int MaxContentLength = 140;
        public static int ShortUrlLength = 23;
        
        private readonly HttpClient _httpClient;

        public string ApiEndPoint {
            get { return "https://api.twitter.com/1.1/"; }
        }

        public TwitterApiManager() {
            _httpClient = new HttpClient();
            if (ShortUrlLength == default(int)) {
#if DEBUG
                ShortUrlLength = 23;
#else
                ///Task.Run(async () => await UpdateShortUrlLength()).Wait();
#endif
            }
        }

        public static TwitterApiManager Create(IdentityFactoryOptions<TwitterApiManager> options, IOwinContext context) {
            return new TwitterApiManager();
        }

        private async Task UpdateShortUrlLength() {
            var dblogin = await FindUserLoginAsync(1);
            string result = await MakeRequestAsync(dblogin, "GET", "help/configuration");
            JObject jsonData = JObject.Parse(result);
            ShortUrlLength = (int)jsonData["short_url_length_https"];
        }

        /// <summary>
        ///     GET users/show
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<TwitterUser> GetUsersShowAsync(int userId) {
            var dblogin = await FindUserLoginAsync(userId);

            var para = new Dictionary<string, object> {
                { "user_id", dblogin.ProviderKey }
            };
            string result = await MakeRequestAsync(dblogin, "GET", "users/show", para);
            var twitterUser = JsonConvert.DeserializeObject<TwitterUser>(result,
                new JsonSerializerSettings {
                    DateFormatString = "ddd MMM dd HH:mm:ss zzz yyyy"
            });
            return twitterUser;
        }

        /// <summary>
        ///     POST statuses/update
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task PostStatusesUpdateAsync(int userId, string content) {
            var dblogin = await FindUserLoginAsync(userId);

            Regex httpReg = new Regex(@"http://\S+");
            Regex httpsReg = new Regex(@"https://\S+");
            string replaced = content;
            replaced = httpReg.Replace(replaced, new string('a', ShortUrlLength));
            replaced = httpsReg.Replace(replaced, new string('a', ShortUrlLength));
            if (replaced.Length > MaxContentLength) {
                throw new ArgumentException(string.Format("Content length over {0}", MaxContentLength), "content");
            }

            var para = new Dictionary<string, object> {
                { "status", content }
            };
            await MakeRequestAsync(dblogin, "POST", "statuses/update", para);
        }

        private Task<AspNetUserLogin> FindUserLoginAsync(int userId) {
            using (var db = new SenkaContext()) {
                return db.AspNetUserLogins
                    .FirstOrDefaultAsync(userlogin =>
                        userlogin.UserId == userId &&
                        userlogin.LoginProviderId == 1);
            }
        }

        /// <summary>
        ///     Make api request to provider
        /// </summary>
        /// <param name="logins">The oauth login infomations</param>
        /// <param name="method">"GET" or "POST"</param>
        /// <param name="path">Reference path to the end point. For example: "users/show"</param>
        /// <returns></returns>
        public Task<string> MakeRequestAsync(AspNetUserLogin login, string method, string path) {
            return MakeRequestAsync(login, method, path, new Dictionary<string, object>());
        }

        /// <summary>
        ///     Make api request to provider
        /// </summary>
        /// <param name="logins">The oauth login infomations</param>
        /// <param name="method">"GET" or "POST"</param>
        /// <param name="path">Reference path to the end point. For example: "users/show"</param>
        /// <param name="paras">Additional url parameters</param>
        /// <returns></returns>
        public Task<string> MakeRequestAsync(AspNetUserLogin login, string method, string path, IDictionary<string, object> paras) {
            if (login == null) throw new ArgumentNullException("logins");

            string endPoint = ApiEndPoint + path + ".json";

            HttpMethod httpMethod;
            switch (method.ToUpper()) {
                case "GET":
                    httpMethod = HttpMethod.Get;
                    break;
                case "POST":
                    httpMethod = HttpMethod.Post;
                    break;
                default:
                    throw new ArgumentException("Invaliad methood");
            }
            return MakeRequestAsync(login.AccessToken, login.AccessTokenSecret, httpMethod, endPoint, paras);
        }

        /// <summary>
        ///     Make api request to provider
        /// </summary>
        /// <param name="oauthToken">Also known as access token</param>
        /// <param name="oauthTokenSecret">Also known as access token secret</param>
        /// <param name="method">Http method</param>
        /// <param name="endPoint">The full path the request made to</param>
        /// <param name="paras">Additional url parameters</param>
        /// <returns></returns>
        public async Task<string> MakeRequestAsync(string accessToken, string accessTokenSecret, HttpMethod method, string endPoint, IDictionary<string, object> paras) {
            string nonce = Guid.NewGuid().ToString("N");
            string timestamp = DateTime.UtcNow.ToUnixTimestamp();

            var authorizationParts = new SortedDictionary<string, string>()
            {
                { "oauth_consumer_key", ConsumerKey },
                { "oauth_nonce", nonce },
                { "oauth_signature_method", "HMAC-SHA1" },
                { "oauth_timestamp", timestamp },
                { "oauth_token", accessToken },
                { "oauth_version", "1.0" }
            };

            var signatureParameter = new SortedDictionary<string, string>();

            foreach (var item in authorizationParts) {
                signatureParameter.Add(item.Key, item.Value);
            }
            foreach (var item in paras) {
                signatureParameter.Add(item.Key, item.Value.ToString());
            }
            // Bulid Signature base OAuth parameter
            var parameterBuilder = new StringBuilder();
            foreach (var authorizationKey in signatureParameter) {
                parameterBuilder.AppendFormat("{0}={1}&", Uri.EscapeDataString(authorizationKey.Key), Uri.EscapeDataString(authorizationKey.Value));
            }
            parameterBuilder.Length--;
            string parameterString = parameterBuilder.ToString();

            // Bulid signature base string
            var canonicalizedRequestBuilder = new StringBuilder();
            canonicalizedRequestBuilder.Append(method.Method);
            canonicalizedRequestBuilder.Append("&");
            canonicalizedRequestBuilder.Append(Uri.EscapeDataString(endPoint));
            canonicalizedRequestBuilder.Append("&");
            canonicalizedRequestBuilder.Append(Uri.EscapeDataString(parameterString));

            // Append signature
            string signature = ComputeSignature(ConsumerSecret, accessTokenSecret, canonicalizedRequestBuilder.ToString());
            authorizationParts.Add("oauth_signature", signature);

            // Bulid Header OAuth parameter
            var authorizationHeaderBuilder = new StringBuilder();
            authorizationHeaderBuilder.Append("OAuth ");
            foreach (var authorizationPart in authorizationParts) {
                authorizationHeaderBuilder.AppendFormat(
                    "{0}=\"{1}\", ", authorizationPart.Key, Uri.EscapeDataString(authorizationPart.Value));
            }
            authorizationHeaderBuilder.Length -= 2;

            // Bulid Url parameter
            var urlParameterBuilder = new StringBuilder();
            if (paras != null && paras.Count > 0) {
                foreach (var para in paras) {
                    urlParameterBuilder.AppendFormat(
                        "{0}={1}&", para.Key, Uri.EscapeDataString(para.Value.ToString()));
                }
                urlParameterBuilder.Length -= 1;
            }
            string urlParameter = urlParameterBuilder.ToString();
            if (urlParameter != "") {
                urlParameter = "?" + urlParameter;
            }

            // Make request
            var request = new HttpRequestMessage(method, endPoint + urlParameter);
            request.Headers.Add("Authorization", authorizationHeaderBuilder.ToString());

            HttpResponseMessage response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        private static string ComputeSignature(string consumerSecret, string tokenSecret, string signatureData) {
            using (var algorithm = new HMACSHA1()) {
                algorithm.Key = Encoding.ASCII.GetBytes(string.Format("{0}&{1}",
                        Uri.EscapeDataString(consumerSecret),
                        Uri.EscapeDataString(tokenSecret)));
                byte[] hash = algorithm.ComputeHash(Encoding.ASCII.GetBytes(signatureData));
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        ///     Dispose this object
        /// </summary>
        public void Dispose() {
            _httpClient.Dispose();
        }
    }
}
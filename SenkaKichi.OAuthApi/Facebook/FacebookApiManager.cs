using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using SenkaKichi.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SenkaKichi.OAuthApi.Facebook
{
    public class FacebookApiManager : IOAuthApiManager
    {
        public const string APIVersion = "v2.3";
        #if DEBUG
        // 戦果基地Debug https://developers.facebook.com/apps/509472409208152/
        public const string AppID = "509472409208152";
        public const string AppSecret = "a60fe91b289b3ebff9ed301a90d37476";
#else
        // 戦果基地 https://developers.facebook.com/apps/509470212541705/
        public const string AppID = "509470212541705";
        public const string AppSecret = "7dcaf7039d2c5e3efcd4146fef89dad1";
#endif

        private SenkaContext _db;
        private readonly HttpClient _httpClient;

        public string ApiEndPoint {
            get { return ""; }
        }

        public FacebookApiManager(SenkaContext database) {
            _httpClient = new HttpClient();
            _db = database;
        }

        public static FacebookApiManager Create(IdentityFactoryOptions<FacebookApiManager> options, IOwinContext context) {
            return new FacebookApiManager(context.Get<SenkaContext>());
        }

        public Task<string> MakeRequestAsync(string accessToken, string accessTokenSecret, HttpMethod method, string endPoint, IDictionary<string, object> paras) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            _httpClient.Dispose();
        }
    }
}

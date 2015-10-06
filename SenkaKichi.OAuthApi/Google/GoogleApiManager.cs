using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using SenkaKichi.DbModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SenkaKichi.OAuthApi.Google
{
    public class GoogleApiManager : IOAuthApiManager
    {
        //SenkaKichi https://console.developers.google.com/project/senka-kichi
#if DEBUG
        public const string ClientID = "971619605114-3cl3rnle84ghk15r42b5fsngqp25vm7f.apps.googleusercontent.com";
        public const string ClientSecret = "msaswe0RR9bvicdzueTjTwOE";
#else
        public const string ClientID = "971619605114-o8o5pctj7avu8qvntaeo8qf2v2r64j4m.apps.googleusercontent.com";
        public const string ClientSecret = "zrGGYl9q5bK8buG8VJjMYfJu";
#endif

        private SenkaContext _db;
        private readonly HttpClient _httpClient;

        public string ApiEndPoint {
            get { return ""; }
        }

        public GoogleApiManager(SenkaContext database) {
            _httpClient = new HttpClient();
            _db = database;
        }

        public static GoogleApiManager Create(IdentityFactoryOptions<GoogleApiManager> options, IOwinContext context) {
            return new GoogleApiManager(context.Get<SenkaContext>());
        }

        public Task<string> MakeRequestAsync(string accessToken, string accessTokenSecret, HttpMethod method, string endPoint, IDictionary<string, object> paras) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            _httpClient.Dispose();
        }
    }
}

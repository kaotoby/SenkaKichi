using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SenkaKichi.OAuthApi
{
    public interface IOAuthApiManager : IDisposable
    {
        string ApiEndPoint { get; }

        /// <summary>
        ///     Make api request to provider
        /// </summary>
        /// <param name="oauthToken">Also known as access token</param>
        /// <param name="oauthTokenSecret">Also known as access token secret</param>
        /// <param name="method">Http method</param>
        /// <param name="endPoint">The full path the request made to</param>
        /// <param name="paras">Additional url parameters</param>
        /// <returns></returns>
        Task<string> MakeRequestAsync(string accessToken, string accessTokenSecret, HttpMethod method, string endPoint, IDictionary<string, object> paras);
    }
}

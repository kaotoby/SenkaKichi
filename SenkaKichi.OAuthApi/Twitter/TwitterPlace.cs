using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SenkaKichi.OAuthApi.Twitter
{
    /// <summary>
    /// A Twitter place. See <see cref="!:https://dev.twitter.com/overview/api/places" />
    /// </summary>
    public class TwitterPlace
    {
        public IDictionary<string, string> Attributes { get; set; }
        public TwitterPlaceBoundingBox Bounding_box { get; set; }
        public string Country { get; set; }
        public string Country_code { get; set; }
        public string Full_name { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Place_type { get; set; }
        public string Url { get; set; }
    }

    /// <summary>
    /// A Twitter place boundingbox. See <see cref="!:https://dev.twitter.com/overview/api/places#obj-boundingbox" />
    /// </summary>
    public class TwitterPlaceBoundingBox
    {
        public object[] Coordinates { get; set; }
        public string Type { get; set; }
    }
}
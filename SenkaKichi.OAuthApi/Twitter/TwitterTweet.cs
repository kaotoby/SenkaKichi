using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SenkaKichi.OAuthApi.Twitter
{
    /// <summary>
    /// A Twitter Tweet. See <see cref="!:https://dev.twitter.com/overview/api/tweets" />
    /// </summary>
    public class TwitterTweet
    {
        public DateTime Created_at { get; set; } //"Sun Apr 05 20:12:41 +0000 2015",
        public long Id { get; set; } //584810911073828864,
        public string Id_str { get; set; } //"584810911073828864",
        public string Text { get; set; } //"\u30b3\u30fc\u30d7\u30b9\u30d1\u30fc\u30c6\u30a3\u30fc BLOOD DRIVE\n\u5b8c\u7d50(Platinum) #PS4share http:\/\/t.co\/W2UOshBp10",
        public string Source { get; set; } //"\u003ca href=\"http:\/\/www.playstation.com\" rel=\"nofollow\"\u003ePlayStation(R)4\u003c\/a\u003e",
        public bool Truncated { get; set; } //false,
        public long? In_reply_to_status_id { get; set; } //null,
        public string In_reply_to_status_id_str { get; set; } //null,
        public long? In_reply_to_user_id { get; set; } //null,
        public string In_reply_to_user_id_str { get; set; } //null,
        public string In_reply_to_screen_name { get; set; } //null,
        public object Geo { get; set; } //null,
        public TwitterCoordinate Coordinates { get; set; } //null,
        public TwitterPlace Place { get; set; } //null,
        public TwitterTweetContributor[] Contributors { get; set; } //null,
        public int Retweet_count { get; set; } //0,
        public int Favorite_count { get; set; } //0,
        public TwitterTweetEntities Entities { get; set; } //{},
        public bool? Favorited { get; set; } //false,
        public bool Retweeted { get; set; } //false,
        public bool? Possibly_sensitive { get; set; } //false,
        public bool? Withheld_copyright { get; set; } //false,
        public string[] Withheld_in_countries { get; set; }
        public string Withheld_scope { get; set; } //false,
        public string Lang { get; set; } //"ja"
        public string Filter_level { get; set; }
        public object Scopes { get; set; }
        public TwitterTweet Retweeted_status { get; set; }
        public TwitterUser User { get; set; }
    }

    /// <summary>
    /// A Twitter Tweet contributor. See <see cref="!:https://dev.twitter.com/overview/api/tweets#obj-contributors" />
    /// </summary>
    public class TwitterTweetContributor
    {
        public long Id { get; set; }
        public string Id_str { get; set; }
        public string Screen_name { get; set; }
    }

    /// <summary>
    /// A Twitter coordinate. See <see cref="!:https://dev.twitter.com/overview/api/tweets#obj-coordinates" />
    /// </summary>
    public class TwitterCoordinate
    {
        public float[] Coordinates { get; set; }
        public string Type { get; set; }
    }

    /// <summary>
    /// Twitter entities used in Tweets. See <see cref="!:https://dev.twitter.com/overview/api/entities" />
    /// </summary>
    public class TwitterTweetEntities
    {
        public TwitterHashTag[] Hashtags { get; set; }
        public string[] Symbols { get; set; } //not done
        public TwitterUserMention[] User_mentions { get; set; }
        public TwitterUrl[] Urls { get; set; }
        public TwitterMedia[] Media { get; set; }
    }
}
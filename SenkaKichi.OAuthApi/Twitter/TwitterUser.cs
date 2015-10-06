using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SenkaKichi.OAuthApi.Twitter
{
    /// <summary>
    /// A Twitter user. See <see cref="!:https://dev.twitter.com/overview/api/users" />
    /// </summary>
    public class TwitterUser
    {
        public long Id { get; set; } //278814296,
        public string Id_str { get; set; } //"278814296",
        public string Name { get; set; } //"\u9b54\u8853\u306e\u9ad8@\u4f50\u4f2f\u6e7e",
        public string Screen_name { get; set; } //"kaotoby",
        public string Location { get; set; } //"Taichung, Taiwan",
        public string Profile_location { get; set; } //null,
        public string Description { get; set; } //"\u4e2d\u6587\u7b80\u4f53,\u7e41\u9ad4 \/\/ English \/\/ \u65e5\u672c\u8a9e",
        public string Url { get; set; } //"https:\/\/t.co\/xz80utNlaX",
        public TwitterUserEntities Entities { get; set; } //{},
        public bool Protected { get; set; } //false,
        public int Followers_count { get; set; } //28,
        public int Friends_count { get; set; } //40,
        public int Listed_count { get; set; } //0,
        public DateTime Created_at { get; set; } //"Fri Apr 08 01:18:33 +0000 2011",
        public int Favourites_count { get; set; } //3,
        public int? Utc_offset { get; set; } //-14400,
        public string Time_zone { get; set; } //"Eastern Time (US & Canada)",
        public bool Geo_enabled { get; set; } //true,
        public bool Verified { get; set; } //false,
        public int Statuses_count { get; set; } //188,
        public string Lang { get; set; } //"en",
        public TwitterTweet Status { get; set; } //{},
        public bool Contributors_enabled { get; set; } //false,
        public bool Is_translator { get; set; } //false,
        public bool Is_translation_enabled { get; set; } //false,
        public string Profile_background_color { get; set; } //"C1EBC1",
        public string Profile_background_image_url { get; set; } //"http:\/\/pbs.twimg.com\/profile_background_images\/373328626\/Rewrite2.jpg",
        public string Profile_background_image_url_https { get; set; } //"https:\/\/pbs.twimg.com\/profile_background_images\/373328626\/Rewrite2.jpg",
        public bool Profile_background_tile { get; set; } //false,
        public string Profile_image_url { get; set; } //"http:\/\/pbs.twimg.com\/profile_images\/1500263710\/404_normal.jpg",
        public string Profile_image_url_https { get; set; } //"https:\/\/pbs.twimg.com\/profile_images\/1500263710\/404_normal.jpg",
        public string Profile_banner_url { get; set; } //"https:\/\/pbs.twimg.com\/profile_banners\/278814296\/1425918006",
        public string Profile_link_color { get; set; } //"ABB8C2",
        public string Profile_sidebar_border_color { get; set; } //"FFFFFF",
        public string Profile_sidebar_fill_color { get; set; } //"DAF0D8",
        public string Profile_text_color { get; set; } //"6B360E",
        public bool Profile_use_background_image { get; set; } //true,
        public bool Default_profile { get; set; } //false,
        public bool Default_profile_image { get; set; } //false,
        public bool? Following { get; set; } //true,
        public bool? Follow_request_sent { get; set; } //false,
        public bool Notifications { get; set; } //false
    }

    /// <summary>
    /// Get other size of profile image. See <see cref="!:https://dev.twitter.com/overview/general/user-profile-images-and-banners" />
    /// </summary>
    public static class TwitterUserExtensions
    {
        public static string GetProfileImageUrlHttpsOriginal(this TwitterUser u) {
            return u.Profile_image_url_https.Replace("_normal.", ".");
        }

        public static string GetProfileImageUrlHttpsMini(this TwitterUser u) {
            return u.Profile_image_url_https.Replace("_normal.", "_mini.");
        }

        public static string GetProfileImageUrlHttpsBigger(this TwitterUser u) {
            return u.Profile_image_url_https.Replace("_normal.", "_bigger.");
        }
    }
}
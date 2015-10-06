using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SenkaKichi.OAuthApi.Twitter
{
    /// <summary>
    /// Part of Twitter entities. See <see cref="!:https://dev.twitter.com/overview/api/entities#obj-hashtags" />
    /// </summary>
    public class TwitterHashTag
    {
        public string Text { get; set; }
        public int[] indices { get; set; }
    }

    /// <summary>
    /// Part of Twitter entities. See <see cref="!:https://dev.twitter.com/overview/api/entities#obj-usermention" />
    /// </summary>
    public class TwitterUserMention
    {
        public string Name { get; set; }
        public string Screen_name { get; set; }
        public int[] Indices { get; set; }
        public long Id { get; set; }
        public string Id_str { get; set; }
    }

    /// <summary>
    /// Part of Twitter entities. See <see cref="!:https://dev.twitter.com/overview/api/entities#obj-url" />
    /// </summary>
    public class TwitterUrl
    {
        public string Expanded_url { get; set; } //http://nbcnews.to/NtkRTJ",
        public string Url { get; set; }//http://t.co/f8ivBrVd",
        public string Display_url { get; set; }//nbcnews.to",
        public int[] Indices { get; set; }
    }

    /// <summary>
    /// Part of Twitter entities. See <see cref="!:https://dev.twitter.com/overview/api/entities#obj-media" />
    /// </summary>
    public class TwitterMedia
    {
        public long Id { get; set; }
        public long Source_status_id { get; set; }
        public string Source_status_id_str { get; set; }
        public string Id_str { get; set; }
        public string Type { get; set; }
        public int[] Indices { get; set; }
        public string Media_url { get; set; }
        public string Media_url_https { get; set; }
        public string Url { get; set; }
        public string Display_url { get; set; }
        public string Expanded_url { get; set; }
        public TwitterSizes Sizes { get; set; }
    }

    /// <summary>
    /// Part of Twitter entities. See <see cref="!:https://dev.twitter.com/overview/api/entities#obj-size" />
    /// </summary>
    public class TwitterSize
    {
        public int W { get; set; }
        public int H { get; set; }
        public string Resize { get; set; }
    }

    /// <summary>
    /// Part of Twitter entities. See <see cref="!:https://dev.twitter.com/overview/api/entities#obj-sizes" />
    /// </summary>
    public class TwitterSizes
    {
        public TwitterSize Large { get; set; }
        public TwitterSize Small { get; set; }
        public TwitterSize Thumb { get; set; }
        public TwitterSize Medium { get; set; }
    }

    public class TwitterUserEntities
    {
        public TwitterUserEntitiesUrl Url { get; set; }
        public TwitterUserEntitiesDescription Description { get; set; }
    }

    public class TwitterUserEntitiesDescription
    {
        public TwitterUrl[] Urls { get; set; }
    }

    public class TwitterUserEntitiesUrl
    {
        public TwitterUrl[] Urls { get; set; }
    }
}
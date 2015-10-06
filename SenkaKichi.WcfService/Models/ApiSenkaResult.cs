using System;
using System.Collections.Generic;

namespace SenkaKichi.WcfService.Models
{
    public class ApiSenkaResult
    {
        public string api_comment { get; set; }
        public string api_comment_id { get; set; }
        public int api_experience { get; set; }
        public string api_flag { get; set; }
        public byte api_level { get; set; }
        public short api_medals { get; set; }
        public int api_member_id { get; set; }
        public string api_nickname { get; set; }
        public string api_nickname_id { get; set; }
        public short api_no { get; set; }
        public byte api_rank { get; set; }
        public short api_rate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.Models
{
        public class Football
        {
            public Result[] results { get; set; }
            public int success { get; set; }
            public Pager pager { get; set; }
        }

        public class Pager
        {
            public int page { get; set; }
            public int total { get; set; }
            public int per_page { get; set; }
        }

        public class Result
        {
            public string our_event_id { get; set; }
            public string id { get; set; }
            public string time_status { get; set; }
            public string time { get; set; }
            public string sport_id { get; set; }
            public string updated_at { get; set; }
            public string ss { get; set; }
            public Away away { get; set; }
            public Home home { get; set; }
            public League league { get; set; }
        }

        public class Away
        {
            public string name { get; set; }
            public string id { get; set; }
        }

        public class Home
        {
            public string name { get; set; }
            public string id { get; set; }
        }

        public class League
        {
            public string name { get; set; }
            public string id { get; set; }
        }

}

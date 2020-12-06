using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.Models
{
    public class FROdds
    {
        public int success { get; set; }
        public Result4[] results { get; set; }
       
    }
    public class Result4
    {
        public Asian_Lines asian_lines { get; set; }
        public Goals goals { get; set; }
        public string FI { get; set; }

        public string event_id { get; set; }

    }
    public class Goals
    {
        public Sp4 sp { get; set; }
        public string updated_at { get; set; }
    }

    public class Sp4
    {
        public Goals_Over_Under goals_over_under { get; set; }
    }

    public class Goals_Over_Under
    {
        public string id { get; set; }
        public string name { get; set; }

        public Odd8[] odds { get; set; }

    }

    public class Odd8
    {
        public string name { get; set; }
        public string odds { get; set; }
        public string header { get; set; }
        public string id { get; set; }
    }

}

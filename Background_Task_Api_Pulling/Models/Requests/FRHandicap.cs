using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.Models
{
    public class FRHandicap
    {
        public int success { get; set; }
        public Result5[] results { get; set; }
    }
    public class Result5
    {
        public Asian_Lines asian_lines { get; set; }
        public Goals1 goals { get; set; }
        public string FI { get; set; }

        public string event_id { get; set; }
    }
    public class Goals1
    {
        public Sp5 sp { get; set; }
        public string updated_at { get; set; }
    }

    public class Sp5
    {
        public Goals_Over_Under1 goals_over_under { get; set; }
    }

    public class Goals_Over_Under1
    {
        public string id { get; set; }
        public string name { get; set; }

        public Odd10[] odds { get; set; }

    }

    public class Odd10
    {
        public string handicap { get; set; }
        public string odds { get; set; }
        public string header { get; set; }
        public string id { get; set; }
    }
}

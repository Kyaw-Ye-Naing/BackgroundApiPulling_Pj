using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.Models
{





    public class Handicap
    {
        public int success { get; set; }
        public Result3[] results { get; set; }
    }

    public class Result3
    {
        public Asian_Lines1 asian_lines { get; set; }
        public Main main { get; set; }
        public string event_id { get; set; }
        public Schedule schedule { get; set; }

        public string FI { get; set; }


    }

    public class Asian_Lines1
    {
        public Sp3 sp { get; set; }
        public string updated_at { get; set; }
    }

    public class Sp3
    {
        public Asian_Handicap2 asian_handicap { get; set; }


        public Goal_Line goal_line { get; set; }
    }

    public class Asian_Handicap2
    {
        public string name { get; set; }
        public Odd9[] odds { get; set; }
        public string id { get; set; }
    }

    public class Odd9
    {
        public string handicap { get; set; }
        public string odds { get; set; }
        public string header { get; set; }
        public string id { get; set; }
    }

}

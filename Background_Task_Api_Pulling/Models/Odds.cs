using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.Models
{

    public class Odds
    {
        public int success { get; set; }
        public Result2[] results { get; set; }
    }

    public class Result2
    {
        public Asian_Lines asian_lines { get; set; }
        public string event_id { get; set; }
        public Schedule schedule { get; set; }
        public string FI { get; set; }
        public Main1 main { get; set; }
    }

    public class Asian_Lines
    {
        public Sp sp { get; set; }
        public string updated_at { get; set; }
    }

    public class Sp
    {
        public Asian_Handicap asian_handicap { get; set; }
        public Goal_Line goal_line { get; set; }
    }

    public class Asian_Handicap
    {
        public string name { get; set; }
        public Odd[] odds { get; set; }
        public string id { get; set; }
    }

    public class Odd
    {
        public string name { get; set; }
        public string odds { get; set; }
        public string header { get; set; }
        public string id { get; set; }
    }

    public class Goal_Line
    {
        public string name { get; set; }
        public Odd1[] odds { get; set; }
        public string id { get; set; }
    }

    public class Odd1
    {
        public string name { get; set; }
        public string odds { get; set; }
        public string header { get; set; }
        public string id { get; set; }
    }

    public class Schedule
    {
        public Sp1 sp { get; set; }
        public string updated_at { get; set; }
    }

    public class Sp1
    {
        public Main[] main { get; set; }
    }

    public class Main
    {
        public string odds { get; set; }
    }

    public class Main1
    {
        public Sp2 sp { get; set; }
        public string updated_at { get; set; }
    }

    public class Sp2
    {
        public Asian_Handicap1 asian_handicap { get; set; }
        public Full_Time_Result full_time_result { get; set; }
        public Goal_Line1 goal_line { get; set; }
    }

    public class Asian_Handicap1
    {
        public string name { get; set; }
        public Odd2[] odds { get; set; }
        public string id { get; set; }
    }

    public class Odd2
    {
        public string name { get; set; }
        public string odds { get; set; }
        public string header { get; set; }
        public string id { get; set; }
    }

    public class Full_Time_Result
    {
        public string name { get; set; }
        public Odd3[] odds { get; set; }
        public string id { get; set; }
    }

    public class Odd3
    {
        public string odds { get; set; }
        public string name { get; set; }
        public string id { get; set; }
    }

    public class Goal_Line1
    {
        public string name { get; set; }
        public Odd4[] odds { get; set; }
        public string id { get; set; }
    }

    public class Odd4
    {
        public string name { get; set; }
        public string odds { get; set; }
        public string header { get; set; }
        public string id { get; set; }
    }


}


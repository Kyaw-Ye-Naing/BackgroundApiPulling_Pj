using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.Models
{

    public class GoalResult
    {
        public int success { get; set; }
        public Result1[] results { get; set; }
    }

    public class Result1
    {
        public string bet365_id { get; set; }
        public string confirmed_at { get; set; }
        public int has_lineup { get; set; }
        public string id { get; set; }
        public string time_status { get; set; }
        public Event[] events { get; set; }
        public string inplay_created_at { get; set; }
        public string inplay_updated_at { get; set; }
        public string time { get; set; }
        public Scores scores { get; set; }
        public Stats stats { get; set; }
        public string sport_id { get; set; }
        public string ss { get; set; }
        public Away2 away { get; set; }
        public Home2 home { get; set; }
        public League2 league { get; set; }
    }

    public class Scores
    {
        public _2 _2 { get; set; }
        public _1 _1 { get; set; }
    }

    public class _2
    {
        public string home { get; set; }
        public string away { get; set; }
    }

    public class _1
    {
        public string home { get; set; }
        public string away { get; set; }
    }

    public class Stats
    {
        public string[] yellowcards { get; set; }
        public string[] on_target { get; set; }
        public string[] dangerous_attacks { get; set; }
        public string[] penalties { get; set; }
        public string[] corners { get; set; }
        public string[] substitutions { get; set; }
        public string[] redcards { get; set; }
        public string[] attacks { get; set; }
        public string[] ball_safe { get; set; }
        public string[] corner_h { get; set; }
        public string[] off_target { get; set; }
        public string[] goals { get; set; }
    }

    public class Away2
    {
        public object cc { get; set; }
        public string name { get; set; }
        public string image_id { get; set; }
        public string id { get; set; }
    }

    public class Home2
    {
        public object cc { get; set; }
        public string name { get; set; }
        public string image_id { get; set; }
        public string id { get; set; }
    }

    public class League2
    {
        public object cc { get; set; }
        public string name { get; set; }
        public string id { get; set; }
    }

    public class Event
    {
        public string text { get; set; }
        public string id { get; set; }
    }

}

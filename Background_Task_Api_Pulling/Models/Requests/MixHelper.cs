using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.Models
{
    public class MixHelper
    {
        public decimal GamblingId { get; set; }
        public int Amount { get; set; }
        public decimal User { get; set; }
        public int Count { get; set; }
        public List<Details> Details { get; set; }
    }
    public class Details
    {
        public decimal GamblingId { get; set; }     
        public decimal? FootballTeamId { get; set; }
        public bool? Under { get; set; }
        public bool? Overs { get; set; }
        public string BodyOdd { get; set; }
        public string GoalOdd { get; set; }
        public bool? IsHome { get; set; }
        public decimal? RapidEventId { get; set; }
        public int HomeResult { get; set; }
        public int AwayResult { get; set; } 
        public bool IsHomeBodyOdds { get; set; }
    }
}

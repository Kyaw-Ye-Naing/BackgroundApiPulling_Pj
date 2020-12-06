using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models
{
    public partial class TblGamblingDetails
    {
        public decimal GamblingDetailsId { get; set; }
        public decimal? GamblingId { get; set; }
        public decimal? LeagueId { get; set; }
        public decimal? FootballTeamId { get; set; }
        public bool? Under { get; set; }
        public bool? Overs { get; set; }
        public string BodyOdd { get; set; }
        public string GoalOdd { get; set; }
        public bool? Home { get; set; }
        public bool? Away { get; set; }
        public bool? IsHome { get; set; }
        public decimal? RapidEventId { get; set; }
        public decimal? UpcomingEventId { get; set; }
        public decimal? OppositeTeamId { get; set; }
        public bool? IsHomeBodyOdd { get; set; }
    }
}

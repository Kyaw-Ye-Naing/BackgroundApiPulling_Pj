using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models
{
    public partial class ViewUpcomingEventDetails
    {
        public decimal UpcomingEventId { get; set; }
        public decimal? RapideventId { get; set; }
        public decimal? LeagueId { get; set; }
        public string LeagueName { get; set; }
        public decimal? RapidLeagueId { get; set; }
        public decimal? HomeTeamId { get; set; }
        public decimal? HomeRapidTeamId { get; set; }
        public string HomeFootballTeam { get; set; }
        public string HomeFootballTeamMyan { get; set; }
        public decimal? AwayTeamId { get; set; }
        public decimal? AwayRapidTeamId { get; set; }
        public string AwayFootballTeam { get; set; }
        public string AwayFootballTeamMyan { get; set; }
        public DateTime? EventDate { get; set; }
        public DateTime? EventTime { get; set; }
        public bool? Active { get; set; }
        public decimal? HomeOdd { get; set; }
        public decimal? HomeHandicap { get; set; }
        public decimal? AwayOdd { get; set; }
        public decimal? AwayHandicap { get; set; }
    }
}

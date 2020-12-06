using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models
{
    public partial class TblLeague
    {
        public decimal LeagueId { get; set; }
        public string LeagueName { get; set; }
        public decimal? RapidLeagueId { get; set; }
        public bool? Active { get; set; }
    }
}

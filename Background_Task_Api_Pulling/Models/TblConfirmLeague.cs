using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models
{
    public partial class TblConfirmLeague
    {
        public decimal? ConfirmLeagueId { get; set; }
        public decimal? LeagueId { get; set; }
        public decimal? RapidLeagueId { get; set; }
        public bool? Active { get; set; }
    }
}

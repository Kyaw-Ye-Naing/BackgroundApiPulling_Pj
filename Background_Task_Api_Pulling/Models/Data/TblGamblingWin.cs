using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models.Data
{
    public partial class TblGamblingWin
    {
        public decimal GamblingWinId { get; set; }
        public decimal? GamblingId { get; set; }
        public decimal? UserId { get; set; }
        public decimal? GoalResultId { get; set; }
        public decimal? WinAmount { get; set; }
        public int? GamblingTypeId { get; set; }
        public bool? Active { get; set; }
        public decimal? LoseAmount { get; set; }
        public decimal? Wlpercent { get; set; }
        public decimal? BetAmount { get; set; }
    }
}

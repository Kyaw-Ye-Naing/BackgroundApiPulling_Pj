using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models
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
    }
}

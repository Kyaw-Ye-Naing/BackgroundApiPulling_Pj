using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models.Data
{
    public partial class TblUserCommissionType
    {
        public int CommissionTypeId { get; set; }
        public int? GamblingTypeId { get; set; }
        public int? BetTeamCount { get; set; }
        public string CommissionType { get; set; }
        public decimal? DefaultCommission { get; set; }
    }
}

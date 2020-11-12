using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models
{
    public partial class TblGoalResult
    {
        public decimal GoalResultId { get; set; }
        public string RapidEventId { get; set; }
        public int? HomeResult { get; set; }
        public int? AwayResult { get; set; }
        public DateTime? EventDatetime { get; set; }
        public DateTime? EventDate { get; set; }
    }
}

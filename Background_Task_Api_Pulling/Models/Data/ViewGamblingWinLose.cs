using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models.Data
{
    public partial class ViewGamblingWinLose
    {
        public decimal GamblingWinId { get; set; }
        public decimal? UserId { get; set; }
        public string Username { get; set; }
        public int? RoleId { get; set; }
        public string Role { get; set; }
        public int? GamblingTypeId { get; set; }
        public int? TeamCount { get; set; }
        public decimal? GoalResultId { get; set; }
        public decimal? BetAmount { get; set; }
        public decimal? WinAmount { get; set; }
        public decimal? LoseAmount { get; set; }
        public decimal? Wlpercent { get; set; }
        public int CommissionTypeId { get; set; }
        public decimal? AgentId { get; set; }
        public int AgentRoleId { get; set; }
        public string AgentRole { get; set; }
        public decimal? AgentCom { get; set; }
        public decimal? UserCom { get; set; }
        public decimal? DefaultCom { get; set; }
        public DateTime? CreatedDate { get; set; }
        public decimal? GamblingId { get; set; }
    }
}

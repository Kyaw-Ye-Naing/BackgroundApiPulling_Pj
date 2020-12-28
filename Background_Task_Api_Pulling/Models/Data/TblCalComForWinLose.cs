using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models.Data
{
    public partial class TblCalComForWinLose
    {
        public decimal CalComId { get; set; }
        public string PostingNo { get; set; }
        public decimal? UserId { get; set; }
        public int? UserRoleId { get; set; }
        public decimal? SubUserId { get; set; }
        public int? SubRoleId { get; set; }
        public decimal? SubWl { get; set; }
        public decimal? SubCom { get; set; }
        public decimal? SubWlPm { get; set; }
        public decimal? UserWl { get; set; }
        public decimal? UserCom { get; set; }
        public decimal? UserWlPm { get; set; }
        public decimal? UpUserId { get; set; }
        public int? UpRoleId { get; set; }
        public decimal? UpWl { get; set; }
        public decimal? UpCom { get; set; }
        public decimal? UpWlPm { get; set; }
        public decimal? GamblingWinId { get; set; }
        public int? GamblingTypeId { get; set; }
        public decimal? GoalResultId { get; set; }
        public decimal? BetAmount { get; set; }
        public decimal? WinAmount { get; set; }
        public decimal? LoseAmount { get; set; }
        public decimal? WlPercent { get; set; }
        public int? CommissionTypeId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public decimal? DefaultCom { get; set; }
    }
}

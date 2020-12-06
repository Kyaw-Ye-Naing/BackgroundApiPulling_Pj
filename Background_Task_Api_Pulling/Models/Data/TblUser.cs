using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models
{
    public partial class TblUser
    {
        public decimal UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool? Lock { get; set; }
        public int? RoleId { get; set; }
        public string Mobile { get; set; }
        public decimal? SharePercent { get; set; }
        public decimal? BetLimitForMix { get; set; }
        public decimal? BetLimitForSingle { get; set; }
        public decimal? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

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
        public decimal? SingleBetCommission5 { get; set; }
        public decimal? SingleBetCommission8 { get; set; }
        public decimal? MixBetCommission2count15 { get; set; }
        public decimal? MixBetCommission3count20 { get; set; }
        public decimal? MixBetCommission4count20 { get; set; }
        public decimal? MixBetCommission5count20 { get; set; }
        public decimal? MixBetCommission6count20 { get; set; }
        public decimal? MixBetCommission7count20 { get; set; }
        public decimal? MixBetCommission8count20 { get; set; }
        public decimal? MixBetCommission9count25 { get; set; }
        public decimal? MixBetCommission10count25 { get; set; }
        public decimal? MixBetCommission11count25 { get; set; }
        public decimal? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

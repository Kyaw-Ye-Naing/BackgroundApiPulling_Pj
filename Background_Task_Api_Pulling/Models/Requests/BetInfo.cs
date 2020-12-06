using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.Models.Requests
{
    public class BetInfo
    {
        public decimal BetGambling { get; set; }
        public decimal? BetType { get; set; }
        public int BetTeamCount { get; set; }
        public decimal BetTeam { get; set; }
        public bool BetOver { get; set; }
        public bool BetUnder { get; set; }
        public bool? BetIsHome { get; set; }
        public int? BetAmount { get; set; }
        public string BetBody { get; set; }
        public string BetGoal { get; set; }
        public decimal? BetUser { get; set; }
        public decimal? BetRapid { get; set; }
    }
}

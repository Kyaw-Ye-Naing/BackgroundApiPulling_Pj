using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.Models.Requests
{
    public class CalculatedCommissionData
    {
        public string postingNo { get; set; }
        public decimal userId { get; set; }
        public decimal subUserId { get; set; }
        public decimal subWL { get; set; }
        public decimal subCom { get; set; }
        public decimal subWL_PM { get; set; }
        public decimal userWL { get; set; }
        public decimal userCom { get; set; }
        public decimal userWL_PM { get; set; }
        public decimal upUserId { get; set; }
        public decimal upWL { get; set; }
        public decimal upCom { get; set; }
        public decimal upWL_PM { get; set; }
        public decimal gamblingWinId { get; set; }
        public decimal gamblingTypeId { get; set; }
        public decimal goalResultId { get; set; }
        public decimal betAmount { get; set; }
        public decimal winAmount { get; set; }
        public decimal loseAmount { get; set; }
        public decimal wlPercent { get; set; }
        public decimal commissionTypeId { get; set; }
        public DateTime createdDate { get; set; }
        public decimal defaultCom { get; set; }
        public int userRoleId { get; set; }
        public int subRoleId { get; set; }
        public int upRoleId { get; set; }
        public decimal gamblingId { get; set; }

    }
}
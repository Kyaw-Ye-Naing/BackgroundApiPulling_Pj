using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models
{
    public partial class TblCredit
    {
        public decimal CreditId { get; set; }
        public string PostingNo { get; set; }
        public decimal? UserId { get; set; }
        public int? Amount { get; set; }
        public bool? Active { get; set; }
        public decimal? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models.Data
{
    public partial class TblUserPosting
    {
        public decimal UserPostingId { get; set; }
        public string PostingNo { get; set; }
        public int? TransactionTypeId { get; set; }
        public decimal? GamblingId { get; set; }
        public decimal? UserId { get; set; }
        public decimal? Inward { get; set; }
        public decimal? Outward { get; set; }
        public bool? Active { get; set; }
        public decimal? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

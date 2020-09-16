using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models
{
    public partial class TblUserPosting
    {
        public decimal UserPostingId { get; set; }
        public string PostingNo { get; set; }
        public decimal? UserId { get; set; }
        public decimal? Inward { get; set; }
        public decimal? Outward { get; set; }
        public bool? Active { get; set; }
        public decimal? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

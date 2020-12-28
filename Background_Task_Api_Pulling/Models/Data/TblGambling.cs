using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models.Data
{
    public partial class TblGambling
    {
        public decimal GamblingId { get; set; }
        public int? TransactionTypeId { get; set; }
        public string PostingNo { get; set; }
        public int? GamblingTypeId { get; set; }
        public int? TeamCount { get; set; }
        public int? Amount { get; set; }
        public bool? Active { get; set; }
        public decimal? UserId { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

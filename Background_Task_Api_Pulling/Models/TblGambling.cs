using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models
{
    public partial class TblGambling
    {
        public decimal GamblingId { get; set; }
        public string PostingNo { get; set; }
        public int? GamblingTypeId { get; set; }
        public decimal? EventId { get; set; }
        public decimal? RapidEventId { get; set; }
        public int? TeamCount { get; set; }
        public int? Amount { get; set; }
        public bool? Active { get; set; }
    }
}

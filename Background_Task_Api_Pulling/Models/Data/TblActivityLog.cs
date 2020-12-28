using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models.Data
{
    public partial class TblActivityLog
    {
        public decimal ActivityLogId { get; set; }
        public string PageName { get; set; }
        public decimal? NewUser { get; set; }
        public string OldData { get; set; }
        public string NewData { get; set; }
        public string Action { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Remark { get; set; }
    }
}

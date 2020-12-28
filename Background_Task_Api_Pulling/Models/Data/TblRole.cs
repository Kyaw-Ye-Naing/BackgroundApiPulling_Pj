using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models.Data
{
    public partial class TblRole
    {
        public int RoleId { get; set; }
        public string Role { get; set; }
        public bool? Active { get; set; }
        public string Discription { get; set; }
        public decimal? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}

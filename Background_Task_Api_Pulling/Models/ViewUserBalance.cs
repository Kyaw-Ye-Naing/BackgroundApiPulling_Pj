using System;
using System.Collections.Generic;

namespace Background_Task_Api_Pulling.Models
{
    public partial class ViewUserBalance
    {
        public decimal? UserId { get; set; }
        public decimal? Inward { get; set; }
        public decimal? Outward { get; set; }
        public decimal? Balance { get; set; }
    }
}

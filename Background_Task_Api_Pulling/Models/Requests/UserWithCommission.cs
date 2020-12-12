using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.Models.Requests
{
    public class UserWithCommission
    {
        public decimal UserId { get; set; }
        public decimal Commission{ get; set; }
    }
}

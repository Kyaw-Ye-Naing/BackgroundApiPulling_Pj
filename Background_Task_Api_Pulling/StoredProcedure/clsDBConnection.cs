using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.StoredProcedure
{
    public class clsDBConnection
    {
        public SqlConnection sql;
        public clsDBConnection()
        {
            sql= new SqlConnection(clsPublicVariable.connectionString);            
        }
    }
}

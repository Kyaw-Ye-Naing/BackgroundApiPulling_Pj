using Background_Task_Api_Pulling.Models.Requests;
using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace Background_Task_Api_Pulling.StoredProcedure.ReportsSP
{
    public class spReports:clsDBConnection
    {
        public DataTable GetWinLoseDataWithDate(DateTime calculateDate)
        {
            DataTable dt = new DataTable();
            if (sql.State == ConnectionState.Closed)
            {
                sql.Open();
            }
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("SP_GetWinLoseDataWithDate", sql);
            sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
            sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@calculateDate", calculateDate.ToString("yyyy-MM-dd"));
            sqlDataAdapter.Fill(dt);
            return dt;
        }

        public DataTable GetWinLoseDataWithGamblingWinId(decimal gamblingWinId)
        {
            DataTable dt = new DataTable();
            if (sql.State == ConnectionState.Closed)
            {
                sql.Open();
            }
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("SP_GetWinLoseDataWithGamblingWinId", sql);
            sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
            sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@gamblingWinId", gamblingWinId);
            sqlDataAdapter.Fill(dt);
            return dt;
        }

        public void SaveCalculatedCommissionData(CalculatedCommissionData data)
        {
            try
            {
                if (sql.State==ConnectionState.Closed)
                {
                    sql.Open();
                }
                SqlCommand cmd = new SqlCommand("SP_SaveCalculatedCommissionData",sql);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@postingNo", data.postingNo);
                cmd.Parameters.AddWithValue("@userId", data.userId);
                cmd.Parameters.AddWithValue("@userRoleId", data.userRoleId);
                cmd.Parameters.AddWithValue("@subUserId", data.subUserId);
                cmd.Parameters.AddWithValue("@subRoleId", data.subRoleId);
                cmd.Parameters.AddWithValue("@subWL", data.subWL);
                cmd.Parameters.AddWithValue("@subCom", data.subCom);
                cmd.Parameters.AddWithValue("@subWL_PM", data.subWL_PM);
                cmd.Parameters.AddWithValue("@userWL", data.userWL);
                cmd.Parameters.AddWithValue("@userCom", data.userCom);
                cmd.Parameters.AddWithValue("@userWL_PM", data.userWL_PM);
                cmd.Parameters.AddWithValue("@upUserId", data.upUserId);
                cmd.Parameters.AddWithValue("@upRoleId", data.upRoleId);
                cmd.Parameters.AddWithValue("@upWL", data.upWL);
                cmd.Parameters.AddWithValue("@upCom", data.upCom);
                cmd.Parameters.AddWithValue("@upWL_PM", data.upWL_PM);
                cmd.Parameters.AddWithValue("@gamblingWinId", data.gamblingWinId);
                cmd.Parameters.AddWithValue("@gamblingTypeId", data.gamblingTypeId);
                cmd.Parameters.AddWithValue("@goalResultId", data.goalResultId);
                cmd.Parameters.AddWithValue("@betAmount", data.betAmount);
                cmd.Parameters.AddWithValue("@winAmount", data.winAmount);
                cmd.Parameters.AddWithValue("@loseAmount", data.loseAmount);
                cmd.Parameters.AddWithValue("@wlPercent", data.wlPercent);
                cmd.Parameters.AddWithValue("@commissionTypeId", data.commissionTypeId);
                cmd.Parameters.AddWithValue("@createdDate", data.createdDate);
                cmd.Parameters.AddWithValue("@defaultCom", data.defaultCom);
                cmd.Parameters.AddWithValue("@gamblingId", data.gamblingId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("spReport SaveCalculatedCommissionData>"+ex.Message);
            }
            finally
            {
                sql.Close();
            }
        }

        public DataTable FindCreatedUserAndCommission(decimal userId,decimal userCommissionTypeId)
        {
            DataTable dt = new DataTable();
            try
            {
                if (sql.State == ConnectionState.Closed)
                {
                    sql.Open();
                }
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter("SP_FindCreatedUserAndCommission", sql);
                sqlDataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@userId", userId);
                sqlDataAdapter.SelectCommand.Parameters.AddWithValue("@userCommissionTypeId", userCommissionTypeId);
                sqlDataAdapter.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                Console.WriteLine("spReport FindCreatedUserAndCommission>"+ex.Message);
            }
            finally
            {
                sql.Close();
            }
            return dt;
        }


        public void SaveUserPosting(string postingNo,int transactionTypeIdIn,int transactionTypeIdOut,decimal gamblingId,decimal userIdIn,decimal amount,decimal userIdOut)
        {
            try
            {
                if (sql.State == ConnectionState.Closed)
                {
                    sql.Open();
                }
                SqlCommand cmd = new SqlCommand("SP_SaveUserPosting", sql);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@postingNo", postingNo);
                cmd.Parameters.AddWithValue("@transactionTypeIdIn", transactionTypeIdIn);
                cmd.Parameters.AddWithValue("@transactionTypeIdOut", transactionTypeIdOut);
                cmd.Parameters.AddWithValue("@gamblingId", gamblingId);
                cmd.Parameters.AddWithValue("@userIdIn", userIdIn);
                cmd.Parameters.AddWithValue("@amount", amount);
                cmd.Parameters.AddWithValue("@userIdOut", userIdOut);
                cmd.Parameters.AddWithValue("@createdDate", DateTime.Now);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine("spReport SaveUserPosting>" + ex.Message);
            }
            finally
            {
                sql.Close();
            }
        }


    }
}

using Background_Task_Api_Pulling.Models.Requests;
using Background_Task_Api_Pulling.StoredProcedure.ReportsSP;
using System;
using System.Data;
using System.Linq;

namespace Background_Task_Api_Pulling.CommonClass
{
    public class CalculateComm
    {
        //Calculation Commission From User Win or Lose
        public bool CalculateCommissionForWinLose(decimal gid)
        {
            try
            {
                decimal gamblingWinId = gid;
                spReports sp = new spReports();
                //dt = sp.GetWinLoseDataWithDate(calDate);
                DataTable dt = sp.GetWinLoseDataWithGamblingWinId(gamblingWinId);

                foreach (DataRow row in dt.Rows)
                {
                    //if (row["agentRoleId"].ToString()=="1")
                    //{
                    //    CalculateCommissionForAdmin(row);
                    //}
                    //else
                    //{
                    CalculateCommissionForOtherRole(row);
                    //}
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return true;
        }

        public decimal CalCommissionPercent(string commission, decimal wlPercent)
        {
            return Convert.ToDecimal(commission) / 100 * wlPercent;
        }

        public decimal CalCommissionAmount(decimal betAmt, decimal commmissionPercent)
        {
            return betAmt / 100 * commmissionPercent;
        }

        public void CalculateCommissionForOtherRole(DataRow row)
        {
            spReports _spReport = new spReports();

            decimal betAmount = Convert.ToDecimal(row["betAmount"].ToString());//100
            decimal wlPercent = Convert.ToDecimal(row["wlpercent"].ToString());//50
            decimal betWinLoseAmount = wlPercent == 0 ? betAmount : betAmount / 100 * wlPercent;//50


            decimal subUserCommissionPercent = CalCommissionPercent(row["userCom"].ToString(), wlPercent);//20/100*50=10
            decimal defaultCommsiionPercent = CalCommissionPercent(row["defaultCom"].ToString(), wlPercent);//20/100*50=10
            decimal userCommissionPercent = CalCommissionPercent(row["agentCom"].ToString(), wlPercent);//0/100*50=0

            decimal defaultCommissionAmount = CalCommissionAmount(betAmount, defaultCommsiionPercent);//100/100*10=10
            decimal userCommissionAmount = CalCommissionAmount(betAmount, userCommissionPercent);//100/100*0=0 
            decimal subUserCommissionAmount = CalCommissionAmount(betAmount, subUserCommissionPercent);//100/100*10=10
            bool isWin = Convert.ToDecimal(row["winAmount"].ToString()) == 0 ? false : true;
            string voucherNo = "COM" + row["userId"].ToString() + row["roleId"].ToString() + row["gamblingWinId"].ToString() +
                row["goalResultId"].ToString() + Convert.ToDateTime(row["createdDate"].ToString()).ToString("yyyyMMddhhmmss");

            decimal commissionTypeId = Convert.ToDecimal(row["commissionTypeId"].ToString());
            decimal userId = Convert.ToDecimal(row["agentId"].ToString());

            DataTable dtUpUser = _spReport.FindCreatedUserAndCommission(userId, commissionTypeId);

            int userRoleId = int.Parse(row["agentRoleId"].ToString());
            decimal subUserId = Convert.ToDecimal(row["userId"].ToString());
            int subRoleId = int.Parse(row["roleId"].ToString());
            decimal subWL = isWin ? betWinLoseAmount : betWinLoseAmount * -1;
            decimal subCom = subUserCommissionAmount;
            decimal userWL = userRoleId == 1 ? isWin ? betWinLoseAmount * -1 : betWinLoseAmount : 0;
            decimal userCom = userRoleId == 1 ? subCom * -1 : userCommissionAmount - subUserCommissionAmount;
            decimal upUserId = dtUpUser.Rows.Count > 0 ? Convert.ToDecimal(dtUpUser.Rows[0]["userId"].ToString()) : 0;
            int upRoleId = dtUpUser.Rows.Count > 0 ? int.Parse(dtUpUser.Rows[0]["roleId"].ToString()) : 0;
            decimal upWL = dtUpUser.Rows.Count > 0 ? isWin ? betWinLoseAmount * -1 : betWinLoseAmount : 0;
            decimal upCom = dtUpUser.Rows.Count > 0 ? userCom * -1 : 0;

            CalculatedCommissionData calculated = new CalculatedCommissionData();
            //add data to cal class
            calculated.postingNo = voucherNo;

            calculated.subUserId = subUserId;//under user
            calculated.subRoleId = subRoleId;
            calculated.subWL = subWL;
            calculated.subCom = subCom;
            calculated.subWL_PM = calculated.subWL + calculated.subCom;

            calculated.userId = userId;//view user
            calculated.userRoleId = userRoleId;
            calculated.userWL = userWL;
            calculated.userCom = userCom;
            calculated.userWL_PM = calculated.userWL + calculated.userCom;

            calculated.upUserId = upUserId;
            calculated.upRoleId = upRoleId;
            calculated.upWL = upWL;
            calculated.upCom = upCom;
            calculated.upWL_PM = dtUpUser.Rows.Count > 0 ? calculated.upWL + calculated.upCom : 0;

            calculated.gamblingWinId = Convert.ToDecimal(row["gamblingWinId"].ToString());
            calculated.gamblingTypeId = Convert.ToDecimal(row["gamblingTypeId"].ToString());
            calculated.goalResultId = Convert.ToDecimal(row["goalResultId"].ToString());
            calculated.betAmount = betAmount;
            calculated.wlPercent = wlPercent;
            calculated.winAmount = Convert.ToDecimal(row["winAmount"].ToString());
            calculated.loseAmount = Convert.ToDecimal(row["loseAmount"].ToString());
            calculated.commissionTypeId = commissionTypeId;
            calculated.createdDate = Convert.ToDateTime(row["createdDate"].ToString());
            calculated.gamblingId = Convert.ToDecimal(row["gamblingId"].ToString());
            calculated.defaultCom = defaultCommsiionPercent;
            _spReport.SaveCalculatedCommissionData(calculated);

            //while (dtUpUser.Rows.Count > 0)
            //{
            //    calculated.postingNo = voucherNo;

            //    calculated.subUserId = userId;//under user
            //    calculated.subRoleId = userRoleId;
            //    calculated.subWL = userWL;
            //    calculated.subCom = Convert.ToDecimal(dtUpUser.Rows[0]["subUserCommission"].ToString());
            //    calculated.subWL_PM = calculated.subWL + calculated.subCom;

            //    calculated.userId = upUserId;//view user
            //    calculated.userRoleId = upRoleId;
            //    calculated.userWL = 0;
            //    calculated.userCom = CalCommissionAmount(betAmount, Convert.ToDecimal(dtUpUser.Rows[0]["userCommission"].ToString()) -
            //                         Convert.ToDecimal(dtUpUser.Rows[0]["subUserCommission"].ToString()));
            //    calculated.userWL_PM = calculated.userWL + calculated.userCom;

            //    dtUpUser = new DataTable();
            //    dtUpUser = _spReport.FindCreatedUserAndCommission(calculated.userId, commissionTypeId);

            //    calculated.upUserId = dtUpUser.Rows.Count > 0 ? Convert.ToDecimal(dtUpUser.Rows[0]["userId"].ToString()) : 0; ;
            //    calculated.upRoleId = dtUpUser.Rows.Count > 0 ? int.Parse(dtUpUser.Rows[0]["roleId"].ToString()) : 0;
            //    calculated.upWL = dtUpUser.Rows.Count > 0 ? isWin ? betWinLoseAmount * -1 : betWinLoseAmount : 0;
            //    calculated.upCom = dtUpUser.Rows.Count > 0 ? calculated.userCom * -1 : 0;
            //    calculated.upWL_PM = dtUpUser.Rows.Count > 0 ? calculated.upWL + calculated.upCom : 0;
            //    _spReport.SaveCalculatedCommissionData(calculated);


            //}
            while (dtUpUser.Rows.Count > 0)
            {
                calculated.postingNo = voucherNo;

                calculated.subUserId = userId;//under user
                calculated.subRoleId = userRoleId;
                calculated.subWL = userWL;
                calculated.subCom = Convert.ToDecimal(dtUpUser.Rows[0]["subUserCommission"].ToString());
                calculated.subWL_PM = calculated.subWL + calculated.subCom;

                calculated.userId = upUserId;//view user
                calculated.userRoleId = upRoleId;
                calculated.userWL = 0;
                calculated.userCom = CalCommissionAmount(betAmount, Convert.ToDecimal(dtUpUser.Rows[0]["userCommission"].ToString()) -
                                     Convert.ToDecimal(dtUpUser.Rows[0]["subUserCommission"].ToString()));
                calculated.userWL_PM = calculated.userWL + calculated.userCom;

                dtUpUser.Rows.Clear();//edit akn 5-1-2020
                dtUpUser = _spReport.FindCreatedUserAndCommission(calculated.userId, commissionTypeId);

                calculated.upUserId = dtUpUser.Rows.Count > 0 ? Convert.ToDecimal(dtUpUser.Rows[0]["userId"].ToString()) : 0;
                calculated.upRoleId = dtUpUser.Rows.Count > 0 ? int.Parse(dtUpUser.Rows[0]["roleId"].ToString()) : 0;
                calculated.upWL = dtUpUser.Rows.Count > 0 ? isWin ? betWinLoseAmount * -1 : betWinLoseAmount : 0;
                calculated.upCom = dtUpUser.Rows.Count > 0 ? calculated.userCom * -1 : 0;
                calculated.upWL_PM = dtUpUser.Rows.Count > 0 ? calculated.upWL + calculated.upCom : 0;
                _spReport.SaveCalculatedCommissionData(calculated);

                //add new akn 5-1-2020
                userId = calculated.userId;
                userRoleId = calculated.userRoleId;
                userWL = dtUpUser.Rows.Count > 0 ? isWin ? betWinLoseAmount : betWinLoseAmount * -1 : 0;

                upUserId = calculated.upUserId;
                upRoleId = calculated.upRoleId;
                //add new akn 5-1-2020
            }

            // add here user repaid for win lose logic
            //transaction type Id 7=win 8=lose 1000 500 =1500
            //decimal repayAmount = isWin ? betAmount + betWinLoseAmount - defaultCommissionAmount : betWinLoseAmount - defaultCommissionAmount;
            if (isWin)
            {
                decimal repayAmount = isWin ? calculated.winAmount - defaultCommissionAmount : calculated.loseAmount - defaultCommissionAmount;
                _spReport.SaveUserPosting(voucherNo, 7, 8, calculated.gamblingId, calculated.subUserId, repayAmount, calculated.userId);
            }
        }

        public void CalculateCommissionForAdmin(DataRow row)
        {
            CalculatedCommissionData calculated = new CalculatedCommissionData();
            decimal betAmount = Convert.ToDecimal(row["betAmount"].ToString());//100
            decimal wlPercent = Convert.ToDecimal(row["wlpercent"].ToString());//50
            decimal betWinLoseAmount = betAmount / 100 * wlPercent;//50

            decimal userCommissionPercent = Convert.ToDecimal(row["userCom"].ToString()) / 100 * wlPercent;//20/100*50=10
            decimal defaultCommsiionPercent = Convert.ToDecimal(row["defaultCom"].ToString()) / 100 * wlPercent;//20/100*50=10
            decimal adminCommissionPercent = Convert.ToDecimal(row["agentCom"].ToString()) / 100 * wlPercent;//0/100*50=0

            decimal defaultCommissionAmount = betAmount / 100 * defaultCommsiionPercent;//100/100*10=10
            decimal adminCommissionAmount = betAmount / 100 * (adminCommissionPercent +
                Convert.ToDecimal(row["agentRoleId"].ToString()) == 1 ? (defaultCommsiionPercent - userCommissionPercent) : 0);//100/100*0+10-=0 //(remark defaultCommsiionPercent-userCommissionPercent is only for admin
            decimal userCommissionAmount = betAmount / 100 * userCommissionPercent;//100/100*10=10
            bool isWin = Convert.ToDecimal(row["winAmount"].ToString()) == 0 ? false : true;

            string voucherNo = "COM" + row["userId"].ToString() + row["roleId"].ToString() + row["gamblingWinId"].ToString() +
                row["goalResultId"].ToString() + Convert.ToDateTime(row["createdDate"].ToString()).ToString("yyyyMMddhhmmss");

            //add data to cal class
            calculated.postingNo = voucherNo;
            calculated.userId = Convert.ToDecimal(row["agentId"].ToString());//view user
            calculated.userRoleId = int.Parse(row["roleId"].ToString());
            calculated.subUserId = Convert.ToDecimal(row["userId"].ToString());//under user
            calculated.subRoleId = int.Parse(row["agentRoleId"].ToString());
            calculated.subWL = isWin ? betWinLoseAmount : betWinLoseAmount * -1;
            calculated.subCom = userCommissionAmount;
            calculated.subWL_PM = (isWin ? betWinLoseAmount : betWinLoseAmount * -1) + userCommissionAmount;
            calculated.userWL = isWin ? betWinLoseAmount * -1 : betWinLoseAmount;
            calculated.userCom = adminCommissionAmount - userCommissionAmount;
            calculated.userWL_PM = (isWin ? betWinLoseAmount * -1 : betWinLoseAmount) + (userCommissionAmount * -1);
            calculated.upUserId = 0;
            calculated.upRoleId = 0;
            calculated.upWL = 0;
            calculated.upCom = 0;
            calculated.upWL_PM = 0;
            calculated.gamblingWinId = Convert.ToDecimal(row["gamblingWinId"].ToString());
            calculated.gamblingTypeId = Convert.ToDecimal(row["gamblingTypeId"].ToString());
            calculated.goalResultId = Convert.ToDecimal(row["goalResultId"].ToString());
            calculated.betAmount = Convert.ToDecimal(row["betAmount"].ToString());
            calculated.winAmount = Convert.ToDecimal(row["winAmount"].ToString());
            calculated.loseAmount = Convert.ToDecimal(row["loseAmount"].ToString());
            calculated.wlPercent = Convert.ToDecimal(row["wlPercent"].ToString());
            calculated.commissionTypeId = Convert.ToDecimal(row["commissionTypeId"].ToString());
            calculated.createdDate = Convert.ToDateTime(row["createdDate"].ToString());
            calculated.defaultCom = Convert.ToDecimal(row["defaultCom"].ToString());

            spReports _spReport = new spReports();
            _spReport.SaveCalculatedCommissionData(calculated);
        }
    }
}

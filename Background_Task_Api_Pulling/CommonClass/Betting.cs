using System;

namespace Background_Task_Api_Pulling.CommonClass
{
    public class Betting
    {
        //Method of Calculation Win or Lose for Over Betting 
        public int WinOrLoseOver(int goalUnit, int diff, decimal unit, int? betAmount)
        {
            int result;
            int tempAmount;
            if (diff == goalUnit)
            {
                tempAmount = (int)(betAmount * Math.Round(unit / 100, 2));
                result = (int)(betAmount + tempAmount);
            }
            else if (diff > goalUnit)
            {
                result = (int)(betAmount * 2);
            }
            else
            {
                result = 0;
            }

            return result;
        }

        //Method of Calculation Win or Lose for Under Betting
        public int WinOrLoseUnder(int goalUnit, int diff, decimal unit, int? betAmount)
        {
            int result;
            int tempAmount;
            if (diff == goalUnit)
            {
                tempAmount = (int)(betAmount * Math.Round(unit / 100, 2));
                result = (int)(betAmount + tempAmount);
            }
            else if (diff < goalUnit)
            {
                result = (int)(betAmount * 2);
            }
            else
            {
                result = 0;
            }

            return result;
        }

    }
}

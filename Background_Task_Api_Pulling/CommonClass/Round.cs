using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.CommonClass
{
    public class Round
    {
        //Calculation odds to take the nearest value
        public int RoundValue(int odds)
        {
            int result;
            // int odds = Convert.ToInt32(oddValue);
            int remainder;
            if (odds<0)
            {
                odds= Math.Abs(odds);
                remainder = odds % 10;
                if (remainder <= 2)
                {
                    result = (odds - remainder)*-1;
                }
                else if (remainder >= 3 && remainder <= 7)
                {
                    int tempInt = 5 - remainder;
                    result = (odds+tempInt)*-1;
                }
                else
                {
                    int tempValue = 10 - remainder;
                    result = (odds + tempValue)*-1;
                }
            }
            else
            {
                remainder = odds % 10;
                if (remainder <= 2)
                {
                    result = odds - remainder;
                }
                else if (remainder >= 3 && remainder <= 7)
                {
                    int tempInt = 5 - remainder;
                    result = tempInt + odds;
                }
                else
                {
                    int tempValue = 10 - remainder;
                    result = odds + tempValue;
                }
            }
            return result;
        }
    }
}

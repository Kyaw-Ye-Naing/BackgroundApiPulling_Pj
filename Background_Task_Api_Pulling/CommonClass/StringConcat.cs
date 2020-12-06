using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Background_Task_Api_Pulling.CommonClass
{
    public class StringConcat
    {
        //Concatanation goal and unit as a string body
        public string Body(int unit, int goal)
        {
            string result;
            if (unit<0)
            {
                result = goal.ToString() + unit.ToString();
            }
            else
            {
                result = goal.ToString() + "+" + unit.ToString();
            }
            return result;
        }

        //Concatanation unit as a string body
        public string Draw(int unit)
        {
            string result;
            if (unit < 0)
            {
                result = "="+unit.ToString();
            }
            else if (unit == 0)
            {
                result = "="+"D";
            }
            else
            {
                result = "="+"+" + unit.ToString();
            }
            return result;
        }

        //Method of Spliting String Handicap to Int Array
        public int[] CutBodyHandicap(string betBody)
        {
            int[] result = new int[2];
            if (betBody.StartsWith("="))
            {  
                if (betBody.Length==2)
                {
                    result[0] = 0;
                    result[1] = 0;
                }
                else
                {
                    string unit = betBody.Remove(0, 1);
                    result[0] = 0;
                    result[1] = Convert.ToInt32(unit);
                }

            }
            else
            {
                    result[0] = Convert.ToInt32(betBody.Substring(0,1));
                    result[1] = Convert.ToInt32(betBody.Remove(0, 1));
            }
            return result;
        }
    }
}

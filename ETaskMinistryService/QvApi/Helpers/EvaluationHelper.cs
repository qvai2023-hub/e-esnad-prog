using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QvApi.Helpers
{
    public class EvaluationHelper
    {
        public static decimal CountMiniuts(decimal TimeCount, int timeunit)
        {
            decimal result = 0;
            switch (timeunit)
            {
                case 1:
                    result = TimeCount * 60 * 8 * 5 * 4;
                    break;
                case 2:
                    result = TimeCount * 60 * 8 * 5;
                    break;
                case 3:
                    result = TimeCount * 60 * 8;
                    break;
                case 4:
                    result = TimeCount * 60;
                    break;
                case 5:
                    result = TimeCount;
                    break;
                default:
                    break;

            }
            return result;
        }
        public static List<int> getInts(int i)
        {
            List<int> getInt = new List<int>();
            for (int z = 0; z < 10; z++)
            {
                getInt.Add(z);
            }
            return getInt;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagementModel
{
    public static class GenericDelete
    {
        public static String ConnectionString;

        /// <summary>
        /// Generic Delete .
        /// </summary>
        /// <param name="iID">Item ID</param>
        /// <param name="strCurrenClass">Item Class</param>
        /// <param name="strConnectionString">Connection String</param>
        /// <returns>Boolean</returns>
        public static Boolean Delete(int iID, string strCurrenClass, String strConnectionString)
        {
            bool result = false;

            if (iID != 0)
            {
                ConnectionString = strConnectionString;

                Type CurrentRep =
                    typeof (GenericRepository<>).MakeGenericType(Type.GetType("TaskManagementModel." + strCurrenClass));

                var methodInfo = CurrentRep.GetMethod("Delete", new[] {typeof (int)});

                if (methodInfo != null)
                    result = (bool) methodInfo.Invoke(Activator.CreateInstance(CurrentRep), new object[] {iID});

            }

            return result;
        }
    }
}
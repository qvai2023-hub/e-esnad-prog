using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EtaskMinstry
{
    public enum Actions
    {
        Index = 1,
        Create = 2,
        Edit = 3,
        Delete = 4,
        Login = 5
    }

    public enum ArabicActions
    {
        مشاهدة = 1,
        اضافة = 2,
        تعديل = 3,
        حذف = 4,
        دخول = 5
    }


    public enum UserRole
    {
        Administrator = 1,
        User = 2
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EtaskMinstry.Models;
using EtaskMinstry.Models.Company;
using TaskManagementModel;
using TaskManagementModel.Extentions;

namespace EtaskMinstry.AppCode
{
    public enum DashBoaedTaskType{       
    
        EmpReject =0,
        Empfinish, //NeedApprove
        NeedAssign,//comp only
        FinishToday, // for both emp  & comp
        Susspended ,//for both emp  & comp
        Delayed , // for both emp  & comp
        NewAssigned, //emp only
        Doing , //emp only
        NeedApprove,//comp only     
        NeedProgrees ,// emp only
        New //comp only
    }





 
}
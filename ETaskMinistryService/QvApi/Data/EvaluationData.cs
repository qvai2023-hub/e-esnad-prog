using QvApi.Entities;
using QvApi.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Transactions;
using QvApi.App_Data;
namespace QvApi.Data
{
    public class EvaluationData
    {
        //[return: System.Xml.Serialization.XmlElementAttribute("Evaluations")]
       // [return: System.Xml.Serialization.XmlElement(ElementName="Evaluations")]

        public static EvaluationResponseMessage GetEvaluation(DateTime PeriodStart, DateTime PeriodEnd)
        {
           using (TransactionScope ts = new TransactionScope())
            {            
                DataClassesDataContext db = new DataClassesDataContext();
                List<Evaluation> EmployeesEvaluation = new List<Evaluation>();
                var dbemps = db.Employees.ToList();

                /*  //The period end can't equal the period start so I commented this code
                //If the start date equals the end date, we increment the end date because the enddate is exclusive so that the result will include the PeriodStart
                if (PeriodStart == PeriodEnd)
                {
                    PeriodEnd = PeriodEnd.AddDays(1);
                }
                  */
           
                foreach (var emp in dbemps)
                {
                    Evaluation evalemp = new Evaluation();
                    evalemp.IdNumber = !string.IsNullOrEmpty(emp.NationalID) ? long.Parse(emp.NationalID) : 0;

                    //evalemp.AssignedTasks = emp.Tasks.Count(tsk => (tsk.StartDate >= PeriodStart && tsk.StartDate < PeriodEnd) || (tsk.EndDate >= PeriodStart && tsk.EndDate < PeriodEnd));
              
                    /*
                    evalemp.AssignedTasks = emp.Tasks.Count(tsk => 
                        (tsk.StartDate < PeriodEnd && tsk.EndDate >= PeriodStart) //The tasks whose their start date is before the period end date and their end date is more than or equal the period start date
                        ||  //Or
                        (tsk.EndDate < PeriodStart 
                        &&

                        (!tsk.TaskTLogs.Any(tl => tl.EmpID == emp.EmpID && tl.StatusID == 5 && tl.CreatedDate<PeriodStart)) //The tasks whose end date is before the period start date but their status was not completed before the period start.
                       // (tsk.StatusID == 1 || tsk.StatusID == 2)
                    
                        ));
                    */
                    //

                    //because period end is inclusive 
                   PeriodEnd= PeriodEnd.AddDays(1);

                    var assignedTasks = db.Tasks.Where(

                    
                                   
                        tsk =>

                        //The start date of the task is before the period end (the task will not start in future)
                        (tsk.StartDate<PeriodEnd )
                    
                        &&
                        //The tasks whose end date is before the period start date but their status was not completed before the period start.   
                         tsk.TaskTLogs.Any(tlg => tlg.EmpID == emp.EmpID && (tlg.StatusID == 1 || tlg.StatusID == 2) && tlg.CreatedDate < PeriodEnd )
                     
                        &&
                        //The tasks whose end date is before the period start date but their status was not completed before the period start.
                        (!tsk.TaskTLogs.Any(tlg => tlg.EmpID == emp.EmpID && tlg.StatusID == 5 && tlg.CreatedDate < PeriodStart)) 
                        &&

                        //The tasks  whose last status before the start date was not pending And there was no status change between the period start and period end
                        //this will also include the tasks that belonged to the employee but were reassigned to another employee before the period start date
                        !(
                            //The tasks  whose last status before the start date was not pending 
                            (
                                tsk.TaskTLogs.Where(tlg=>tlg.EmpID==emp.EmpID && tlg.CreatedDate<PeriodStart).OrderByDescending(tlg=>tlg.CreatedDate).FirstOrDefault()== null ? false : //if there are no records then the whole expression shall be true (so we should return false)
                              tsk.TaskTLogs.Where(tlg => tlg.EmpID == emp.EmpID && tlg.CreatedDate < PeriodStart).OrderByDescending(tlg => tlg.CreatedDate).FirstOrDefault().StatusID == 3
                            )
                            && //And there was no status change between the period start and period end
                            (
                                !tsk.TaskTLogs.Any(tlg => tlg.EmpID == emp.EmpID && tlg.CreatedDate >= PeriodStart && tlg.CreatedDate < PeriodEnd)
                            )
                        )
                        && //And the tasks whose last status was not "refused" before the period end
                        !(
                            tsk.TaskTLogs.Where(tlg => tlg.CreatedDate < PeriodEnd && tlg.EmpID == emp.EmpID).OrderByDescending(tlg => tlg.CreatedDate).FirstOrDefault() == null ? false : //if there are no records then the whole expression shall be true (so we should return false)
                            tsk.TaskTLogs.Where(tlg => tlg.CreatedDate < PeriodEnd && tlg.EmpID == emp.EmpID).OrderByDescending(tlg => tlg.CreatedDate).FirstOrDefault().StatusID == 9
                         )
                        ).ToList();

                    evalemp.AssignedTasks = assignedTasks.Count();

                   // evalemp.CompletedTasks = emp.Tasks.Count(tsk => tsk.StatusID == 5 && ((tsk.StartDate >= PeriodStart && tsk.StartDate < PeriodEnd) || (tsk.EndDate >= PeriodStart && tsk.EndDate < PeriodEnd)));
                    evalemp.CompletedTasks = emp.TaskTLogs.Count(l => l.CreatedDate >= PeriodStart && l.CreatedDate < PeriodEnd && l.StatusID == 5); //The count of the tasks that the employee has flagged as done within the specified period
                
                    evalemp.EstLaborOfficeId = emp.LaborOfficeID.HasValue ? emp.LaborOfficeID.Value : 0;
                    evalemp.EstSequenceNumber = emp.SequenceNumber.HasValue ? emp.SequenceNumber.Value : 0;
                    evalemp.LoginCount = emp.LogGenerals.Count(log => log.ActionID == 5 && (log.ActionTime >= PeriodStart && log.ActionTime < PeriodEnd));
                    evalemp.LogoutCount = emp.LogGenerals.Count(log => log.ActionID == 9 && (log.ActionTime >= PeriodStart && log.ActionTime < PeriodEnd));
                    evalemp.TotalWorkTime = emp.TaskTLogs.Where(i => i.CreatedDate >= PeriodStart && i.CreatedDate < PeriodEnd).Sum(i => EvaluationHelper.CountMiniuts(i.TimeCount ?? 0, i.TimUnitID ?? 0));

                    var assignedtaskestime = assignedTasks.Sum(i => EvaluationHelper.CountMiniuts(i.ExpectedTime ?? 0, i.TimeUnitID));  //The sum of expected time for all assigned tasks (same condition as assigned tasks) calculated in minutes
 
                    
                    evalemp.ActivityLevel = Math.Round(assignedtaskestime > 0 ? (((evalemp.TotalWorkTime*100) / assignedtaskestime) % 100) : 0.00m , 2);
                   // evalemp.ActivityLevel = Math.Round(assignedtaskestime > 0 ? (((evalemp.TotalWorkTime * 100) / assignedtaskestime)>100?100.00m:(((evalemp.TotalWorkTime * 100) / assignedtaskestime) % 100)) : 0.00m, 2);
                    EmployeesEvaluation.Add(evalemp);
                }
                EvaluationResponseMessage result = new EvaluationResponseMessage();
                result.Evaluations = EmployeesEvaluation;
                return result;
           }
        }
    }
}
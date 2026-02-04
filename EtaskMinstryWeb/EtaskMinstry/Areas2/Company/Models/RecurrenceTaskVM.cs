using Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using TaskManagementModel;

namespace EtaskMinstry.Areas.Company.Models
{
    public class RecurrenceTaskVM
    {

        public int? Id { get; set; }
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [RegularExpression(ValidationResource.RevString150, ErrorMessageResourceName = "ValidRegularFullName150",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "العنوان ")]
        public string Title { get; set;}
        [Display(Name = "الملخص")]
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [RegularExpression(ValidationResource.RevString350, ErrorMessageResourceName = "ValidRegularSummary350",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string Summary { get; set; }
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "التفاصيل")]        
        public string Description { get; set; }
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        [Display(Name = "المشروع")]  
        public int ProjectID { get; set; }
        [Display(Name = "التكرار")]  
        public int RecurrenceTypeID { get; set; }
        [Display(Name = "تاريخ البداية")]  
        public string StartRec { get; set; }
        [Display(Name = "تاريخ النهاية")]  
        public string EndRec { get; set; }
        [Required(ErrorMessageResourceName = "ValidRequired",
            ErrorMessageResourceType = typeof(ValidationMessage.ValidationMessages))]
        public string RecValue { get; set; }
        public int CompanyID { get; set; }
        [Display(Name = " الموظفين")]  
        public List<Emps> Employees { get; set; }
        public bool IsChecked { get; set; }

          private UnitOfWork _unitOfWork;

          public RecurrenceTaskVM()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }


          public List<Project> GetAllProjects()
          {
              return _unitOfWork.ProjectRepository.Get().ToList();
          }

          public List<TaskManagementModel.RecurrenceType> GetAllRecurrenceType()
          {
              return _unitOfWork.RecurrenceType.Get().ToList();
          }

          public List<Emps> GetAllEmployees()
          {

              List<Emps> eTaskList = _unitOfWork.Employee.Get().Select(a => new
            {
               employee_Id= a.EmpID,
               Employee_Name= a.Name,
               company_Id= a.CompanyID,
               IsAssigned= false,
               IsETask=true
            }).AsEnumerable().Select(a => new Emps
            {
                employee_Id = a.employee_Id,
                Name = a.Employee_Name,
                company_Id = a.company_Id.Value,
                IsAssigned= a.IsAssigned,
                IsETask = a.IsETask
            }).ToList();

              //List<Emps> eDawamList = _unitOfWork.Employees.Get().Select(a => new
              //{
              //    employee_Id = a.company_Id,
              //    Employee_Name = a.Employee_Name,
              //    company_Id = a.company_Id,
              //    IsAssigned = false,
              //    IsETask=false
              //}).AsEnumerable().Select(a => new Emps
              //{
              //    employee_Id = a.employee_Id,
              //    Name = a.Employee_Name,
              //    company_Id = a.company_Id,
              //    IsAssigned = a.IsAssigned,
              //    IsETask=a.IsETask
              //}).ToList();

              for (int x = 0; x < eTaskList.Count(); x++)
              {
                  eTaskList[x].employee_Id= eTaskList[x].employee_Id;
                  //eDawamList.Add(eTaskList[x]);
              }


              return eTaskList;
          }
          public Boolean Save()
          {
              Boolean bReuslt = false;
              if (Id == 0)
              {
                 
                  TaskManagementModel.RecurrenceTask obj = new TaskManagementModel.RecurrenceTask()
                  {
                      Title= Title,
                      Summary= Summary,
                      Description= Description,
                      ProjectID= ProjectID,
                      RecurrenceTypeID=RecurrenceTypeID,
                      StartRec = EtaskMinstry.Extentions.HijriToGregDates(StartRec.Split(' ')[0]),
                      EndRec=EtaskMinstry.Extentions.HijriToGregDates(EndRec.Split(' ')[0]),
                      RecValue=RecValue,
                      CompanyID = MvcApplication.userData.userId
                      
                      

                      
                  };

                  _unitOfWork.RecurrenceTask.Insert(obj);
              
                  _unitOfWork.Save();

               
                  foreach (var item in Employees)
                  {
                      if(item.IsAssigned==true)
                      {
                      RecurrenceTaskEmp recurrenceTaskEmp = new RecurrenceTaskEmp();
                      if (item.IsETask == true)
                          recurrenceTaskEmp.EmpID =item.employee_Id;
                      else
                          recurrenceTaskEmp.EmpID = item.employee_Id;
                      
                      
                      recurrenceTaskEmp.RecurrenceTaskID = obj.RecurrenceTaskID;
                      _unitOfWork.RecurrenceTaskEmp.Insert(recurrenceTaskEmp);
                      }
                  }
                 
                  _unitOfWork.Save();
                  bReuslt = obj.RecurrenceTaskID > 0 ? true : false;
              }
              else
              {
                    var obj = _unitOfWork.RecurrenceTask.GetByID(Id);

                    if (obj != null)
                    {
                        obj.Title = Title;
                        obj.Summary = Summary;
                        obj.Description = Description;
                        obj.ProjectID = (int)ProjectID;
                        obj.RecurrenceTypeID = (int)RecurrenceTypeID;
                        obj.StartRec = EtaskMinstry.Extentions.HijriToGregDates(StartRec.Split(' ')[0]);
                        obj.EndRec = EtaskMinstry.Extentions.HijriToGregDates(EndRec.Split(' ')[0]);
                        obj.RecValue = RecValue;

                        _unitOfWork.RecurrenceTask.Update(obj);

                        _unitOfWork.Save();
                    }
              }

              return bReuslt;
          }


          public class Emps
          {
              public string Name { get; set; }
              public int employee_Id { get; set; }
              public int company_Id { get; set; }
              public bool IsAssigned { get; set; }
              public bool IsETask { get; set; }
          }

    }

   
}
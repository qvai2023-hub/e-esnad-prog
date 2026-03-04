using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace TaskManagementModel
{

    public class UnitOfWork : IDisposable
    {
        #region Repository declaration

        private EtaskMinstryEntities _context;

        private GenericRepository<Attachment> _attachmentRepository;
        private GenericRepository<Priority> _priorityRepository;
        private GenericRepository<Project> _projectRepository;
        private GenericRepository<Status> _statusRepository;
        private GenericRepository<Task> _taskRepository;
        private GenericRepository<TaskComment> _taskCommentRepository;
        private GenericRepository<TaskLog> _taskLogRepository;
        private GenericRepository<TimeUnit> _timUnitRepository;
        private GenericRepository<TaskTLog> _taskTimeStatuseLog;
        private GenericRepository<HoliDay> _holidayRepository;
        private GenericRepository<WeekEnd> _weekEndRepository;
        private GenericRepository<EmployeesNames> _Employees;
        
        private GenericRepository<RecurrenceTask> _recurrenceTask;
        private GenericRepository<RecurrenceType> _recurrenceType;
        private GenericRepository<RecurrenceTaskEmp> _recurrenceTaskEmp;
        //For employee added using eTask
        private GenericRepository<Employee> _EmployeeCompany;
        private GenericRepository<Company> _Company;
        private GenericRepository<UserAccount> _UserAccount;
        private GenericRepository<SIGNAL_R_SESSION> _SIGNAL_R_SESSIONs;
        private GenericRepository<Notification> _Notifications;
        private GenericRepository<NotificationType> _NotificationTypes;
        private GenericRepository<NotificationCollection> _NotificationCollections;
        private GenericRepository<Setting> _settings;
        private GenericRepository<TotalLog> _totallog;
        private GenericRepository<LogGeneral> _LogGeneral;
        private GenericRepository<ControllerName> _ControllerName;
        private GenericRepository<sp_ProjectTasks_Result> _sp_ProjectTasks_Result;
        private GenericRepository<sp_CompanyTasks_Result> _sp_CompanyTasks_Result;
        private GenericRepository<sp_TotalEmployeeTasks_Result> _sp_TotalEmployeeTasks_Result;
        private GenericRepository<ContactU> _ContactU;
        private GenericRepository<Attendance> _attendanceRepository;
        
        private GenericRepository<sp_Attendance_Result> _sp_Attendance_Result;
        private GenericRepository<sp_EmployeePerformanceReport_Result> _sp_EmployeePerformanceReport_Result;
        private GenericRepository<ActivityLog> _activityLogRepository;

        public UnitOfWork(String strConnectionString)
        {
            _context = new EtaskMinstryEntities(strConnectionString); 

        }

        #endregion

        #region Repository Getters


        /// <summary>
        /// LogGeneral get Accessor.
        /// </summary>

        public GenericRepository<LogGeneral> LogGeneralRepository
        {
            get
            {
                return _LogGeneral ?? (_LogGeneral = new GenericRepository<LogGeneral>(_context));
            }
        }

        /// <summary>
        /// LogGeneral get Accessor.
        /// </summary>

        public GenericRepository<TotalLog> TotalLogRepository
        {
            get
            {
                return _totallog ?? (_totallog = new GenericRepository<TotalLog>(_context));
            }
        }

        /// <summary>
        /// ControllerName get Accessor.
        /// </summary>

        public GenericRepository<ControllerName> ControllerNameRepository
        {
            get
            {
                return _ControllerName ?? (_ControllerName = new GenericRepository<ControllerName>(_context));
            }
        }







        /// <summary>
        /// Attachment get Accessor.
        /// </summary>
        public GenericRepository<Attachment> AttachmentRepository
        {
            get
            {
                return _attachmentRepository ?? (_attachmentRepository = new GenericRepository<Attachment>(_context));
            }
        }

        /// <summary>
        /// AttachWeekEndment get Accessor.
        /// </summary>
        public GenericRepository<WeekEnd> WeekEndRepository
        {
            get
            {
                return _weekEndRepository ?? (_weekEndRepository = new GenericRepository<WeekEnd>(_context));
            }
        }

        /// <summary>
        /// HoliDay get Accessor.
        /// </summary>
        public GenericRepository<HoliDay> HolidayRepository
        {
            get
            {
                return _holidayRepository ?? (_holidayRepository = new GenericRepository<HoliDay>(_context));
            }
        }

        /// <summary>
        /// Priority get Accessor.
        /// </summary>
        public GenericRepository<Priority> PriorityRepository
        {
            get
            {
                return _priorityRepository ?? (_priorityRepository = new GenericRepository<Priority>(_context));
            }
        }

        /// <summary>
        /// Project get Accessor.
        /// </summary>
        public GenericRepository<Project> ProjectRepository
        {
            get
            {
                return _projectRepository ?? (_projectRepository = new GenericRepository<Project>(_context));
            }
        }


        /// <summary>
        /// _ContactU get Accessor.
        /// </summary>
        public GenericRepository<ContactU> ContactURepository
        {
            get
            {
                return _ContactU ?? (_ContactU = new GenericRepository<ContactU>(_context));
            }
        }
        /// <summary>
        /// Status get Accessor.
        /// </summary>
        public GenericRepository<Status> StatusRepository
        {
            get
            {
                return _statusRepository ?? (_statusRepository = new GenericRepository<Status>(_context));
            }
        }

        /// <summary>
        /// Task get Accessor.
        /// </summary>
        public GenericRepository<Task> TaskRepository
        {
            get
            {
                return _taskRepository ?? (_taskRepository = new GenericRepository<Task>(_context));
            }
        }

        /// <summary>
        /// Task Comment get Accessor.
        /// </summary>
        public GenericRepository<TaskComment> TaskCommentRepository
        {
            get
            {
                return _taskCommentRepository ?? (_taskCommentRepository = new GenericRepository<TaskComment>(_context));
            }
        }

        /// <summary>
        /// Status get Accessor.
        /// </summary>
        public GenericRepository<TaskLog> TaskLogRepository
        {
            get
            {
                return _taskLogRepository ?? (_taskLogRepository = new GenericRepository<TaskLog>(_context));
            }
        }

        /// <summary>
        /// Status get Accessor.
        /// </summary>
        //public GenericRepository<TaskTimeLog> TaskTimeLogRepository
        //{
        //    get
        //    {
        //        return _taskTimeLogRepository ?? (_taskTimeLogRepository = new GenericRepository<TaskTimeLog>(_context));
        //    }
        //}

        /// <summary>
        /// Status get Accessor.
        /// </summary>
        public GenericRepository<TimeUnit> TimUnitRepository
        {
            get
            {
                return _timUnitRepository ?? (_timUnitRepository = new GenericRepository<TimeUnit>(_context));
            }
        }

        //<summary>
        //TaskTLog get Accessor.
        //</summary>
        public GenericRepository<TaskTLog> TaskStatuseLog
        {
            get
            {
                return _taskTimeStatuseLog ?? (_taskTimeStatuseLog = new GenericRepository<TaskTLog>(_context));
            }
        }

        //<summary>
        //Employees get Accessor.
        //</summary>
        public GenericRepository<EmployeesNames> Employees
        {
            get
            {
                return _Employees ?? (_Employees = new GenericRepository<EmployeesNames>(_context));
            }
        }

        //<summary>
        //Employees get Accessor.
        //</summary>
        //For employee added using eTask
        public GenericRepository<Employee> Employee
        {
            get
            {
                return _EmployeeCompany ?? (_EmployeeCompany = new GenericRepository<Employee>(_context));
            }
        }


        //<summary>
        //Company get Accessor.
        //</summary>
        //For employee added using eTask
        public GenericRepository<Company> Company
        {
            get
            {
                return _Company ?? (_Company = new GenericRepository<Company>(_context));
            }
        }

        /// <summary>
        /// RecurrenceTask get Accessor.
        /// </summary>
        public GenericRepository<RecurrenceTask> RecurrenceTask
        {
            get
            {
                return _recurrenceTask ?? (_recurrenceTask = new GenericRepository<RecurrenceTask>(_context));
            }
        }

        public GenericRepository<RecurrenceType> RecurrenceType
        {
            get
            {
                return _recurrenceType ?? (_recurrenceType = new GenericRepository<RecurrenceType>(_context));
            }
        }

        public GenericRepository<RecurrenceTaskEmp> RecurrenceTaskEmp
        {
            get
            {
                return _recurrenceTaskEmp ?? (_recurrenceTaskEmp = new GenericRepository<RecurrenceTaskEmp>(_context));
            }
        }


        public GenericRepository<UserAccount> UserAccount
        {
            get
            {
                return _UserAccount ?? (_UserAccount = new GenericRepository<UserAccount>(_context));
            }
        }
        //<summary>
        //PushNotification get Accessor.
        //</summary>
        public GenericRepository<SIGNAL_R_SESSION> SIGNAL_R_SESSIONs
        {
            get
            {
                return _SIGNAL_R_SESSIONs ?? (_SIGNAL_R_SESSIONs = new GenericRepository<SIGNAL_R_SESSION>(_context));
            }


        }


        //<summary>
        //Notification get Accessor.
        //</summary>
        public GenericRepository<Notification> Notifications
        {
            get
            {
                return _Notifications ?? (_Notifications = new GenericRepository<Notification>(_context));
            }
        }


        //<summary>
        //Notification get Accessor.
        //</summary>
        public GenericRepository<NotificationType> NotificationTypes
        {
            get
            {
                return _NotificationTypes ?? (_NotificationTypes = new GenericRepository<NotificationType>(_context));
            }
        }


        //<summary>
        //NotificationCollections get Accessor.
        //</summary>
        public GenericRepository<NotificationCollection> NotificationCollections
        {
            get
            {
                return _NotificationCollections ?? (_NotificationCollections = new GenericRepository<NotificationCollection>(_context));
            }
        }

        //<summary>
        //Settings get Accessor.
        //</summary>
        public GenericRepository<Setting> Settings
        {
            get
            {
                return _settings ?? (_settings = new GenericRepository<Setting>(_context));
            }
        }
        /// <summary>
        /// Report SP
        /// </summary>
        public GenericRepository<sp_ProjectTasks_Result> SP_ProjecetTasksResults
        {
            get
            {
                return _sp_ProjectTasks_Result ?? (_sp_ProjectTasks_Result = new GenericRepository<sp_ProjectTasks_Result>(_context));
            }
        }
        /// <summary>
        /// Report SP
        /// </summary>
        public GenericRepository<sp_CompanyTasks_Result> SP_CompanyTasksResults
        {
            get
            {
                return _sp_CompanyTasks_Result ?? (_sp_CompanyTasks_Result = new GenericRepository<sp_CompanyTasks_Result>(_context));
            }
        }

        /// <summary>
        /// Report SP
        /// </summary>
        public GenericRepository<sp_TotalEmployeeTasks_Result> SP_TotalEmployeeTasks_Result
        {
            get
            {
                return _sp_TotalEmployeeTasks_Result ?? (_sp_TotalEmployeeTasks_Result = new GenericRepository<sp_TotalEmployeeTasks_Result>(_context));
            }
        }

        public GenericRepository<Attendance> AttendanceRepository
        {
            get
            {
                return _attendanceRepository ?? (_attendanceRepository = new GenericRepository<Attendance>(_context));
            }
        }

        public GenericRepository<sp_Attendance_Result> Sp_AttendanceResult
        {
            get
            {
                return _sp_Attendance_Result ?? (_sp_Attendance_Result = new GenericRepository<sp_Attendance_Result>(_context));
            }
        }
        public GenericRepository<sp_EmployeePerformanceReport_Result> sp_EmployeePerformanceReportResult
        {
            get
            {
                return _sp_EmployeePerformanceReport_Result ?? (_sp_EmployeePerformanceReport_Result = new GenericRepository<sp_EmployeePerformanceReport_Result>(_context));
            }
        }

        public GenericRepository<ActivityLog> ActivityLogRepository
        {
            get
            {
                return _activityLogRepository ?? (_activityLogRepository = new GenericRepository<ActivityLog>(_context));
            }
        }
        #endregion

        #region Repository Manage

        /// <summary>
        /// Save any change on the context 
        /// </summary>
        public void Save()
        {
            _context.SaveChanges();
        }

        private bool _disposed = false;

        /// <summary>
        /// check if the context is not disposed
        /// then dispose it.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
                if (disposing)
                    _context.Dispose();

            _disposed = true;
        }

        /// <summary>
        /// call it to dispose context.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}

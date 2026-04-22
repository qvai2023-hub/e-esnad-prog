using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskManagementModel;
namespace EtaskMinstry.AppCode
{
    public class TaskWorkflow
    {
        /// <summary>
        /// Change the task status according to the taken action. It also validates whether or not the task is eligible for the taken action.
        /// </summary>
        /// <param name="iTaskId">The Id of the task that needs to change the status for </param>
        /// <param name="action">The taken action on the task (ex Accept, reject, approve ..etc) </param>
        /// <returns>The result of the taken action. The result object contains a bool whether or not the status has changed, and contains both the old and new status values</returns>
        public StatusChangeResult ChangeTaskStatus(int iTaskId, TaskWorkFlowActions action = TaskWorkFlowActions.UpdateStatus)
        {

            bool isStatusChanged = false; //the value returned by the method


            UnitOfWork _unitOfWork = new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
            Task task = _unitOfWork.TaskRepository.GetByID(iTaskId);
            if (task != null) //check if Task not null
            {
                var beforeUpdateObj = task.Clone<TaskManagementModel.Task>(); //for logging purpose
                TaskStatus currentTaskStatus = (TaskStatus)task.StatusID; // The current action status

                switch (action)
                {
                    //Check if the task status should change due to time change.
                    case (TaskWorkFlowActions.UpdateStatus):
                        {

                            if (currentTaskStatus == TaskStatus.Accepted)
                            {
                                if (DateTime.Now.Date >= task.StartDate)
                                {
                                    task.StatusID = (int)TaskStatus.Inprogress;
                                    isStatusChanged = true;
                                }
                            }
                            break;
                        }

                    //employee accepts a task
                    case TaskWorkFlowActions.Accept:
                        {
                            if (currentTaskStatus == TaskStatus.New) //accept the status only if the current status is "New"
                            {
                                //the current date is before the task start date -> set the task as accepted
                                if (DateTime.Now.Date < task.StartDate)
                                {
                                    task.StatusID = (int)TaskStatus.Accepted;
                                    isStatusChanged = true;
                                }
                                else //the current date is after the task start date -> set the task as in progress
                                {
                                    task.StatusID = (int)TaskStatus.Inprogress;
                                    isStatusChanged = true;
                                }
                            }
                            //allow employee to resume work on a disapproved task
                            else if (currentTaskStatus == TaskStatus.NotAproved)
                            {
                                task.StatusID = (int)TaskStatus.Inprogress;
                                isStatusChanged = true;
                            }
                            break;
                        }

                    //Employee Rejects a task
                    case TaskWorkFlowActions.Reject:
                        {
                            if (currentTaskStatus == TaskStatus.New)
                            {
                                task.StatusID = (int)TaskStatus.Rejected;
                                isStatusChanged = true;
                            }
                            break;
                        }
                    //Employee completes a task;
                    case TaskWorkFlowActions.Complete:
                        {
                            if (currentTaskStatus == TaskStatus.Inprogress)
                            {
                                task.StatusID = (int)TaskStatus.Done;
                                isStatusChanged = true;
                            }
                            break;
                        }
                    //Company apporves a task;
                    case TaskWorkFlowActions.Approve:
                        {
                            if (currentTaskStatus == TaskStatus.Done)
                            {
                                task.StatusID = (int)TaskStatus.Approved;
                                isStatusChanged = true;
                            }
                            break;
                        }
                    //Company disapproves a task
                    case TaskWorkFlowActions.Disapprove:
                        {
                            if (currentTaskStatus == TaskStatus.Done)
                            {
                                task.StatusID = (int)TaskStatus.NotAproved;
                                isStatusChanged = true;
                            }
                            break;
                        }

                    //company archive  atask
                    case TaskWorkFlowActions.Archive:
                        {
                            if (currentTaskStatus == TaskStatus.Approved) //Can only archive the task if it has been approved.
                            {
                                task.IsArchived = true;
                                isStatusChanged = true;
                            }
                            break;
                        }

                    //Company reassigns a task
                    case TaskWorkFlowActions.Reassign:
                        {
                            if (currentTaskStatus != TaskStatus.Done && task.IsArchived == false) //can NOT re-assign the task if it's done or archived
                            {
                                task.StatusID = (int)TaskStatus.Pending;
                                isStatusChanged = true;
                            }
                            break;
                        }

                    //company pauses a task
                    case TaskWorkFlowActions.Pause:
                        {
                            if (currentTaskStatus != TaskStatus.Done && currentTaskStatus != TaskStatus.Approved && task.IsArchived == false)
                            {
                                task.StatusID = (int)TaskStatus.Pending;
                                isStatusChanged = true;

                            }
                            break;
                        }

                    //Company reopens the task
                    case (TaskWorkFlowActions.Reopen):
                        {
                            if (currentTaskStatus == TaskStatus.Pending)
                            {
                                //Get the Last TaskStatusLog record for the task where it was paused.
                                var pendingTaskLog = _unitOfWork.TaskStatuseLog.Get(l => (l.TaskID == iTaskId) && (l.StatusID == (int)(TaskStatus.Pending))).OrderByDescending(l => l.TaskTLogID).FirstOrDefault();
                                //Get the previous TaskStatusLog record for the task (it contains the status before pausing the task)
                                var previousTaskLog = _unitOfWork.TaskStatuseLog.Get((l => l.TaskID == iTaskId && l.TaskTLogID < pendingTaskLog.TaskTLogID)).OrderByDescending(l => l.TaskTLogID).FirstOrDefault();
                                //Get the last status for the task before pausing it.
                                TaskStatus previousTaskStatus = (TaskStatus)previousTaskLog.StatusID;
                                task.StatusID = (int)previousTaskStatus;
                                isStatusChanged = true;
                                ChangeTaskStatus(iTaskId, TaskWorkFlowActions.UpdateStatus);
                            }
                            break;
                        }


                } //end switch case

                if (isStatusChanged) //Save the changes if there was any.
                {
                    //Log the status task changes

                    //Clone Task Object for Log purpose

                    EtaskMinstry.AppCode.LogTask.Log(task, beforeUpdateObj);

                    //log in the TaskTLog only if there was a status update (i.e not isArchived)
                    if ((int)currentTaskStatus != task.StatusID)
                    {
                        _unitOfWork.TaskStatuseLog.Insert(new TaskTLog()
                        {
                            TaskID = iTaskId,
                            CreatedDate = DateTime.Now,
                            EmpID = task.EmpID,
                            StatusID = task.StatusID
                        });
                    }

                    //Save the changes
                    _unitOfWork.Save();
                }


                //return the result
                return new StatusChangeResult { IsChanged = isStatusChanged, NewStatus = (TaskStatus)task.StatusID, OldStatus = currentTaskStatus };

            }
            else
                return new StatusChangeResult { IsChanged = isStatusChanged };
        }

    }

    public class StatusChangeResult
    {

        public bool IsChanged { get; set; }
        public TaskStatus OldStatus { get; set; }
        public TaskStatus NewStatus { get; set; }
    }

    public enum TaskWorkFlowActions
    {
        UpdateStatus, //to auto update the status according to the current time (inprogress, delayed,..etc)
        Accept, // Employee accept a task
        Reject, //Employe Reject a task
        Complete, //Employee Complete task
        Approve, //Company approve a completed taask
        Disapprove, //company disapproves a completed task
        Archive, //Archive a approved task
        Pause, //Pauses the task(pend)
        Reassign, //re-assign a task
        Reopen //reopen a paused (pending) task (it will trigger the same action as updatestatus)
    }
}
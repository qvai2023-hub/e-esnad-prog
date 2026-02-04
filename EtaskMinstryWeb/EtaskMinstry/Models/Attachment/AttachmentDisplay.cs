using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TaskManagementModel;

namespace EtaskMinstry.Models.Attachment
{
    public class AttachmentDisplay
    {
        #region "Properties"

        public int ID { get; set; }
        public String FileName { get; set; }
        public String Description { get; set; }

        public int? TaskID { get; set; }
        public int? CommentID { get; set; }

        private UnitOfWork _unitOfWork;

        public AttachmentDisplay()
        {
            _unitOfWork =
                new UnitOfWork(System.Configuration.ConfigurationManager.ConnectionStrings["ETaskEntities"].ToString());
        }

        #endregion

        #region "Task Manage"

        /// <summary>
        /// Get All Priorities .
        /// </summary>
        /// <returns></returns>
        public List<AttachmentDisplay> Get()
        {
            return _unitOfWork.AttachmentRepository.Get()
                              .Select(i => new AttachmentDisplay()
                                  {
                                      ID = i.AttachmentID,
                                      FileName = i.FileName,
                                      Description = i.Description
                                  })
                              .ToList();
        }

        /// <summary>
        /// Get AttachmentDisplay By FileName .
        /// </summary>
        /// <returns></returns>
        public AttachmentDisplay GetAttachment(String strFileName)
        {
            return _unitOfWork.AttachmentRepository.Get()
                              .Select(i => new AttachmentDisplay()
                              {
                                  ID = i.AttachmentID,
                                  FileName = i.FileName,
                                  Description = i.Description,
                                  TaskID = i.TaskID,
                              }).FirstOrDefault(
                                      i => i.FileName == strFileName);
        }

        /// <summary>
        /// Get All Priorities .
        /// </summary>
        /// <returns></returns>
        public void Insert(AttachmentDisplay obj)
        {
            _unitOfWork.AttachmentRepository.Insert(new TaskManagementModel.Attachment()
                {
                    Description = obj.Description,
                    FileName = obj.FileName,
                    TaskID = obj.TaskID
                });
            _unitOfWork.Save();
        }

        /// <summary>
        /// Get All Priorities .
        /// </summary>
        /// <returns></returns>
        public void Delete(AttachmentDisplay obj)
        {
            _unitOfWork.AttachmentRepository.Delete(new TaskManagementModel.Attachment()
            {
                Description = obj.Description,
                FileName = obj.FileName,
                TaskID = obj.TaskID,
                AttachmentID = obj.ID
            });
            _unitOfWork.Save();
        }

        public AttachmentDisplay GetByID(int id)
        {
            return _unitOfWork.AttachmentRepository.Get()
                              .Select(i => new AttachmentDisplay()
                              {
                                  ID = i.AttachmentID,
                                  FileName = i.FileName,
                                  Description = i.Description,
                                  TaskID = i.TaskID,
                              }).FirstOrDefault(
                                      i => i.ID == id);
        }


        #endregion
    }
}
using System;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using EtaskMinstry.Api.Dtos.Common;
using EtaskMinstry.Api.Filters;
using EtaskMinstry.Api.Mapping;
using TaskManagementModel;

namespace EtaskMinstry.Api.Controllers
{
    /// <summary>
    /// Mobile API notifications endpoints. URL prefix: /api/v1/notifications
    ///   GET  /api/v1/notifications?page=1&amp;pageSize=20
    ///   POST /api/v1/notifications/{id}/read       (id = NotificationCollection.ID)
    ///   POST /api/v1/notifications/read-all
    ///
    /// Notifications are stored per-user as NotificationCollection rows. The
    /// `id` parameter on /{id}/read is the NotificationCollection row id
    /// (not the shared Notification id) — that's the row that carries IsSeen.
    /// </summary>
    [JwtAuthorize]
    public class NotificationsController : ApiController
    {
        private const int DefaultPageSize = 20;
        private const int MaxPageSize = 100;

        [HttpGet]
        public HttpResponseMessage List(int page = 1, int pageSize = DefaultPageSize)
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));

            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = DefaultPageSize;
            if (pageSize > MaxPageSize) pageSize = MaxPageSize;

            int userTypeId = userData.UserTypeId;
            int userId = userData.userId;

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);

            var query = uow.NotificationCollections.Get(
                filter: c => c.InstanceID == userId && c.UserTypeID == userTypeId,
                orderBy: q => q.OrderByDescending(c => c.ID),
                includeProperties: "Notification,Notification.NotificationType");

            int totalCount = query.Count();
            var rows = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            var dtos = rows.Select(NotificationMapper.ToDto).ToList();

            return Request.CreateResponse(HttpStatusCode.OK,
                PagedResponse.Build(dtos, page, pageSize, totalCount));
        }

        [HttpPost]
        [ActionName("read")]
        public HttpResponseMessage Read(int id)
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));

            int userTypeId = userData.UserTypeId;
            int userId = userData.userId;

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var row = uow.NotificationCollections.GetByID(id);
            if (row == null)
                return Request.CreateResponse(HttpStatusCode.NotFound,
                    ApiResponse.Fail("لم يتم العثور على الإشعار", "NOTIFICATION_NOT_FOUND"));

            // Authorization: you can only mark your own notifications read.
            if (row.InstanceID != userId || row.UserTypeID != userTypeId)
                return Request.CreateResponse(HttpStatusCode.Forbidden,
                    ApiResponse.Fail("غير مصرح", "NOTIFICATION_FORBIDDEN"));

            if (!row.IsSeen)
            {
                row.IsSeen = true;
                row.SeenDate = DateTime.Now;
                uow.NotificationCollections.Update(row);
                uow.Save();
            }

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(new { id = row.ID, isSeen = true }, "تم تعليم الإشعار كمقروء"));
        }

        [HttpPost]
        [ActionName("read-all")]
        public HttpResponseMessage ReadAll()
        {
            var userData = MvcApplication.userData;
            if (userData == null)
                return Request.CreateResponse(HttpStatusCode.Unauthorized, ApiResponse.Fail("غير مصرح"));

            int userTypeId = userData.UserTypeId;
            int userId = userData.userId;
            DateTime now = DateTime.Now;

            var uow = new UnitOfWork(ConfigurationManager.ConnectionStrings["ETaskEntities"].ConnectionString);
            var unread = uow.NotificationCollections.Get(
                filter: c => c.InstanceID == userId
                             && c.UserTypeID == userTypeId
                             && !c.IsSeen
            ).ToList();

            int markedCount = 0;
            foreach (var row in unread)
            {
                row.IsSeen = true;
                row.SeenDate = now;
                uow.NotificationCollections.Update(row);
                markedCount++;
            }
            if (markedCount > 0) uow.Save();

            return Request.CreateResponse(HttpStatusCode.OK,
                ApiResponse.Ok(new { marked = markedCount }, "تم تعليم جميع الإشعارات كمقروءة"));
        }
    }
}

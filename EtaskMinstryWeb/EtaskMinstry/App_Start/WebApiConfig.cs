using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using EtaskMinstry.Api.Configuration;
using EtaskMinstry.Api.Filters;

namespace EtaskMinstry
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Explicit /api/v1/me → MeController.Get (no {action} segment).
            // Must come BEFORE the generic action-based route below.
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_Me",
                routeTemplate: "api/v1/me",
                defaults: new { controller = "Me", action = "Get" }
            );

            // GET /api/v1/tasks → TasksController.List
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_TasksList",
                routeTemplate: "api/v1/tasks",
                defaults: new { controller = "Tasks", action = "List" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) }
            );

            // POST /api/v1/tasks → TasksController.Create  (Slice 5)
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_TasksCreate",
                routeTemplate: "api/v1/tasks",
                defaults: new { controller = "Tasks", action = "Create" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) }
            );

            // GET /api/v1/tasks/{id} (numeric) → TasksController.Detail
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_TaskDetail",
                routeTemplate: "api/v1/tasks/{id}",
                defaults: new { controller = "Tasks", action = "Detail" },
                constraints: new
                {
                    id = @"^\d+$",
                    httpMethod = new HttpMethodConstraint(HttpMethod.Get)
                }
            );

            // DELETE /api/v1/tasks/{id} → TasksController.Delete  (Slice 5, soft delete)
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_TaskDelete",
                routeTemplate: "api/v1/tasks/{id}",
                defaults: new { controller = "Tasks", action = "Delete" },
                constraints: new
                {
                    id = @"^\d+$",
                    httpMethod = new HttpMethodConstraint(HttpMethod.Delete)
                }
            );

            // /api/v1/tasks/{id}/{action} → TasksController.{accept|reject|complete|time|approve|disapprove}
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_TaskAction",
                routeTemplate: "api/v1/tasks/{id}/{action}",
                defaults: new { controller = "Tasks" },
                constraints: new { id = @"^\d+$" }
            );

            // /api/v1/projects → ProjectsController.List
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_Projects",
                routeTemplate: "api/v1/projects",
                defaults: new { controller = "Projects", action = "List" }
            );

            // /api/v1/employees → EmployeesController.List
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_Employees",
                routeTemplate: "api/v1/employees",
                defaults: new { controller = "Employees", action = "List" }
            );

            // ─────────────── Slice 6 — Notifications + Device Tokens ───────────────

            // GET /api/v1/notifications → NotificationsController.List
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_NotificationsList",
                routeTemplate: "api/v1/notifications",
                defaults: new { controller = "Notifications", action = "List" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Get) }
            );

            // POST /api/v1/notifications/read-all → NotificationsController.ReadAll
            // Must come BEFORE the {id}/read route so "read-all" isn't parsed as id="read-all".
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_NotificationsReadAll",
                routeTemplate: "api/v1/notifications/read-all",
                defaults: new { controller = "Notifications", action = "read-all" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) }
            );

            // POST /api/v1/notifications/{id}/read → NotificationsController.Read
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_NotificationRead",
                routeTemplate: "api/v1/notifications/{id}/read",
                defaults: new { controller = "Notifications", action = "read" },
                constraints: new
                {
                    id = @"^\d+$",
                    httpMethod = new HttpMethodConstraint(HttpMethod.Post)
                }
            );

            // POST /api/v1/device-tokens → DeviceTokensController.Register
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_DeviceTokensRegister",
                routeTemplate: "api/v1/device-tokens",
                defaults: new { controller = "DeviceTokens", action = "Register" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Post) }
            );

            // DELETE /api/v1/device-tokens/{token} → DeviceTokensController.Unregister
            // The token can contain non-numeric / non-trivial characters,
            // so no constraint beyond verb. URL-encode if it contains slashes.
            config.Routes.MapHttpRoute(
                name: "MobileApiV1_DeviceTokensUnregister",
                routeTemplate: "api/v1/device-tokens/{token}",
                defaults: new { controller = "DeviceTokens", action = "Unregister" },
                constraints: new { httpMethod = new HttpMethodConstraint(HttpMethod.Delete) }
            );

            // Mobile API v1 — convention routing.
            // URL shape: /api/v1/{controller}/{action}/{id}
            // (WebApi 1 in this project has no attribute routing, so we use the
            //  classic MapHttpRoute approach.)
            config.Routes.MapHttpRoute(
                name: "MobileApiV1",
                routeTemplate: "api/v1/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Existing convention route — kept untouched for any future
            // non-versioned Web API endpoints.
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // camelCase JSON + ISO 8601 dates + drop XML.
            // Applies to Web API only — MVC's Json() action result is unaffected.
            ApiJsonFormatter.Apply(config.Formatters);

            // CORS headers for /api/v1/ — mobile clients ignore CORS but devtools
            // and PWA previews need it. Override origin via Web.config key
            // "ApiCorsAllowedOrigin" (defaults to "*").
            ApiCorsConfig.Install(config);

            // Global filters for the Mobile API pipeline:
            //   ApiResponseFilter   - wrap raw DTOs in ApiResponse envelope
            //   ApiExceptionFilter  - sanitized 500 errors in Arabic
            // JwtAuthorize is applied per-action (or per-controller via the
            // attribute), so /auth/login can stay open.
            config.Filters.Add(new ApiResponseFilter());
            config.Filters.Add(new ApiExceptionFilter());
        }
    }
}

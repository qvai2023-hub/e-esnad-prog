using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using TaskManagementModel;

namespace EtaskMinstry.Api.Services
{
    /// <summary>
    /// Fire-and-forget FCM dispatcher. Called from inside NotificationHub.Send
    /// (AppCode/Notification.cs) AFTER the SignalR broadcast — the single
    /// brief-sanctioned edit to existing AppCode.
    ///
    /// Design notes:
    ///  - Runs on a ThreadPool thread so the original web/API request returns
    ///    immediately (Risk R4 mitigation from the slice plan).
    ///  - Uses HttpWebRequest (sync, available in .NET Fx 4.8 without async).
    ///  - All exceptions are caught and logged to Debug — must NEVER propagate
    ///    back into the SignalR / web request flow.
    ///  - Uses a fresh UnitOfWork inside the worker thread so we don't leak
    ///    entities across threads (Risk R5).
    ///  - Looks up MobileDeviceToken rows by (UserID), then POSTs to FCM
    ///    legacy HTTP API. On InvalidRegistration / NotRegistered the token
    ///    is marked IsActive=0.
    /// </summary>
    public static class FcmDispatcher
    {
        private const string FcmEndpoint = "https://fcm.googleapis.com/fcm/send";

        /// <summary>
        /// Entry point called from NotificationHub.Send. Resolves the user
        /// list from the collection of NotificationCollection rows that were
        /// just inserted, then queues an FCM POST per device-token.
        ///
        /// Parameters mirror what NotificationHub.Send already has locally so
        /// the injection in Notification.cs is genuinely one line.
        /// </summary>
        public static void Dispatch(
            IList<NotificationCollection> recipients,
            int notificationId,
            string typeName,
            string message,
            string linkToGo)
        {
            if (recipients == null || recipients.Count == 0) return;

            // Snapshot inputs as plain values so the worker thread doesn't
            // touch the original UoW or HttpContext.
            var userIds = recipients.Select(r => r.InstanceID).Distinct().ToList();
            string title = ConfigurationManager.AppSettings["ApplicationName"] ?? "Telesak";
            string body = ShortenForPush(message);
            int notifId = notificationId;
            string type = typeName ?? string.Empty;
            string link = linkToGo ?? string.Empty;

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    string serverKey = ConfigurationManager.AppSettings["FcmServerKey"];
                    if (string.IsNullOrEmpty(serverKey) || serverKey.StartsWith("REPLACE_"))
                    {
                        // No key configured — silently skip. This is the
                        // expected state in dev until someone wires up FCM.
                        return;
                    }

                    var tokenService = new DeviceTokenService();
                    foreach (int userId in userIds)
                    {
                        var tokens = tokenService.GetActive(userId);
                        foreach (var t in tokens)
                        {
                            PostOne(serverKey, t.DeviceToken, title, body, type, link, notifId, tokenService);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        "[FcmDispatcher] Top-level error: {0}", ex.Message);
                }
            });
        }

        private static void PostOne(string serverKey, string token, string title, string body,
            string type, string link, int notifId, DeviceTokenService tokenService)
        {
            try
            {
                var payload = new
                {
                    to = token,
                    notification = new { title = title, body = body },
                    data = new
                    {
                        notificationId = notifId,
                        type = type,
                        link = link
                    }
                };
                string json = JsonConvert.SerializeObject(payload);
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                var req = (HttpWebRequest)WebRequest.Create(FcmEndpoint);
                req.Method = "POST";
                req.ContentType = "application/json";
                req.Headers["Authorization"] = "key=" + serverKey;
                req.Timeout = 10_000;
                req.ContentLength = bytes.Length;
                using (var stream = req.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                using (var resp = (HttpWebResponse)req.GetResponse())
                using (var reader = new StreamReader(resp.GetResponseStream() ?? Stream.Null))
                {
                    string respBody = reader.ReadToEnd();
                    HandleFcmResponse(token, respBody, tokenService);
                }
            }
            catch (WebException wex)
            {
                // Read the error body anyway so we can spot expected 4xx (token
                // problems) vs unexpected 5xx (FCM outage).
                string respBody = string.Empty;
                if (wex.Response != null)
                {
                    using (var reader = new StreamReader(wex.Response.GetResponseStream() ?? Stream.Null))
                    {
                        respBody = reader.ReadToEnd();
                    }
                }
                System.Diagnostics.Debug.WriteLine(
                    "[FcmDispatcher] FCM error: {0}, body={1}", wex.Message, respBody);
                HandleFcmResponse(token, respBody, tokenService);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "[FcmDispatcher] Per-token error: {0}", ex.Message);
            }
        }

        /// <summary>
        /// FCM legacy API returns a JSON body whose `results[]` entry indicates
        /// per-token outcome. We only care about the obvious "stale token"
        /// signals — InvalidRegistration / NotRegistered → deactivate.
        /// </summary>
        private static void HandleFcmResponse(string token, string body, DeviceTokenService tokenService)
        {
            if (string.IsNullOrWhiteSpace(body)) return;
            if (body.IndexOf("NotRegistered", StringComparison.Ordinal) >= 0
                || body.IndexOf("InvalidRegistration", StringComparison.Ordinal) >= 0
                || body.IndexOf("MismatchSenderId", StringComparison.Ordinal) >= 0)
            {
                tokenService.MarkInvalid(token);
                System.Diagnostics.Debug.WriteLine(
                    "[FcmDispatcher] Token deactivated by FCM error: {0}", Mask(token));
            }
        }

        private static string ShortenForPush(string message)
        {
            if (string.IsNullOrEmpty(message)) return string.Empty;
            // Push body has a practical limit; keep it tight.
            const int max = 180;
            return message.Length <= max ? message : message.Substring(0, max) + "…";
        }

        private static string Mask(string token)
        {
            if (string.IsNullOrEmpty(token) || token.Length < 12) return "***";
            return token.Substring(0, 6) + "…" + token.Substring(token.Length - 4);
        }
    }
}

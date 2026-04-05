using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EtaskMinstry.Controllers
{
    public class ChatController : BaseController
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        [HttpPost]
        public async Task<ActionResult> Send()
        {
            try
            {
                // 1. Read JSON body
                string body;
                using (var reader = new StreamReader(Request.InputStream, Encoding.UTF8))
                {
                    Request.InputStream.Position = 0;
                    body = await reader.ReadToEndAsync();
                }

                var requestObj = JObject.Parse(body);
                string message = (string)requestObj["message"];
                var history = requestObj["history"] as JArray ?? new JArray();

                if (string.IsNullOrWhiteSpace(message))
                {
                    return Json(new { success = false, answer = "الرجاء إدخال رسالة.", source = "" });
                }

                // 2. Get user info from session
                var userData = MvcApplication.userData;
                string userName = userData != null ? userData.userName : "مستخدم";
                string userRole = userData != null ? (userData.isCompany ? "شركة" : "موظف") : "غير معروف";
                int userId = userData != null ? userData.userId : 0;

                // 3. Try Q&A match
                string qaAnswer = TryMatchQA(message);
                if (qaAnswer != null)
                {
                    return Json(new { success = true, answer = qaAnswer, source = "qa" });
                }

                // 4. Call Claude API
                string apiKey = ConfigurationManager.AppSettings["ClaudeApiKey"];
                if (string.IsNullOrWhiteSpace(apiKey) || apiKey == "REPLACE_WITH_YOUR_KEY")
                {
                    return Json(new { success = false, answer = "لم يتم تكوين مفتاح API. يرجى التواصل مع المسؤول.", source = "" });
                }

                string systemPrompt =
                    "أنت مساعد ذكي لنظام تيلي ساك (Telesak) لإدارة المهام. " +
                    "تيلي ساك هو نظام إلكتروني لإدارة ومتابعة المهام والمشاريع، " +
                    "يتيح للشركات إسناد المهام للموظفين ومتابعة التقدم والتفاعل والحضور والانصراف. " +
                    "المستخدم الحالي: " + userName + " (الدور: " + userRole + "، المعرّف: " + userId + "). " +
                    "أجب باللغة العربية بشكل مختصر ومفيد. " +
                    "إذا كان السؤال خارج نطاق النظام، وجّه المستخدم بلطف.";

                // Build messages array
                var messages = new JArray();
                foreach (var item in history)
                {
                    string role = (string)item["role"];
                    string content = (string)item["content"];
                    if (role == "user" || role == "assistant")
                    {
                        messages.Add(new JObject { ["role"] = role, ["content"] = content });
                    }
                }
                messages.Add(new JObject { ["role"] = "user", ["content"] = message });

                var requestBody = new JObject
                {
                    ["model"] = "claude-haiku-4-5-20251001",
                    ["max_tokens"] = 1024,
                    ["system"] = systemPrompt,
                    ["messages"] = messages
                };

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
                httpRequest.Headers.Add("x-api-key", apiKey);
                httpRequest.Headers.Add("anthropic-version", "2023-06-01");
                httpRequest.Content = new StringContent(requestBody.ToString(Formatting.None), Encoding.UTF8, "application/json");

                var httpResponse = await _httpClient.SendAsync(httpRequest);
                string responseBody = await httpResponse.Content.ReadAsStringAsync();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    return Json(new { success = false, answer = "فشل الاتصال بخدمة الذكاء الاصطناعي. يرجى المحاولة لاحقاً.", source = "" });
                }

                var responseObj = JObject.Parse(responseBody);
                var contentArray = responseObj["content"] as JArray;
                string answer = "";
                if (contentArray != null && contentArray.Count > 0)
                {
                    answer = (string)contentArray[0]["text"];
                }

                return Json(new { success = true, answer = answer, source = "ai" });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[ChatController] Error: " + ex.Message);
                return Json(new { success = false, answer = "حدث خطأ أثناء معالجة طلبك. يرجى المحاولة مرة أخرى.", source = "" });
            }
        }

        private string TryMatchQA(string message)
        {
            try
            {
                string qaPath = Server.MapPath("~/App_Data/telesak-qa.json");
                if (!System.IO.File.Exists(qaPath))
                    return null;

                string json = System.IO.File.ReadAllText(qaPath, Encoding.UTF8);
                var qaArray = JArray.Parse(json);
                string lowerMessage = message.ToLowerInvariant();

                // First pass: exact match on triggers
                foreach (var entry in qaArray)
                {
                    var triggers = entry["triggers"] as JArray;
                    if (triggers == null) continue;

                    foreach (var trigger in triggers)
                    {
                        string keyword = ((string)trigger ?? "").ToLowerInvariant().Trim();
                        if (!string.IsNullOrEmpty(keyword) && lowerMessage == keyword)
                        {
                            return (string)entry["answer"];
                        }
                    }
                }

                // Second pass: substring match (fallback)
                foreach (var entry in qaArray)
                {
                    var triggers = entry["triggers"] as JArray;
                    if (triggers == null) continue;

                    foreach (var trigger in triggers)
                    {
                        string keyword = ((string)trigger ?? "").ToLowerInvariant().Trim();
                        if (!string.IsNullOrEmpty(keyword) && lowerMessage.Contains(keyword))
                        {
                            return (string)entry["answer"];
                        }
                    }
                }
            }
            catch
            {
                // Q&A file parse error — fall through to AI
            }

            return null;
        }
    }
}

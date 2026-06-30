# Telesak Mobile API — Reference

**Audience:** mobile developers (React Native / Flutter / native iOS / Android) and QA testers.
**Status:** Slices 1–6 implemented (auth, me, attendance, tasks, projects, employees, notifications, FCM). **Comments (§5.10) and Attachments (§5.11) are specified but NOT yet implemented** — see the banners on those sections. Pending tester sign-off on the implemented slices.
**Backend:** ASP.NET Web API on .NET Framework 4.8, same project as the Telesak web app, same SQL Server database.

This document is everything you need to start consuming the API. If something is unclear, ping the backend team — don't guess.

---

## Table of contents

1. [Quick start](#1-quick-start)
2. [Environments + base URLs](#2-environments--base-urls)
3. [Conventions](#3-conventions)
4. [Authentication & token lifecycle](#4-authentication--token-lifecycle)
5. [Endpoint reference](#5-endpoint-reference)
   - [5.1 Auth](#51-auth)
   - [5.2 Me](#52-me)
   - [5.3 Attendance](#53-attendance)
   - [5.4 Tasks (employee)](#54-tasks-employee)
   - [5.5 Tasks (company)](#55-tasks-company)
   - [5.6 Projects](#56-projects)
   - [5.7 Employees (company)](#57-employees-company)
   - [5.8 Notifications](#58-notifications)
   - [5.9 Device tokens (FCM)](#59-device-tokens-fcm)
   - [5.10 Comments](#510-comments) — ⚠️ NOT YET IMPLEMENTED
   - [5.11 Attachments](#511-attachments) — ⚠️ NOT YET IMPLEMENTED
6. [Error codes — full catalogue](#6-error-codes--full-catalogue)
7. [Push notifications (FCM)](#7-push-notifications-fcm)
8. [Testing checklist (for QA)](#8-testing-checklist-for-qa)
9. [Mobile dev FAQ](#9-mobile-dev-faq)
10. [Out of scope](#10-out-of-scope)

---

## 1. Quick start

### For mobile devs

1. Get a `baseUrl` (e.g. `http://192.168.1.4:9091`).
2. Get a real Telesak username + password from the backend team.
3. Send `POST /api/v1/auth/login` with `{ username, password }`. Save the returned `accessToken` and `refreshToken`.
4. On every subsequent request, add header `Authorization: Bearer <accessToken>`.
5. When you get `401`, call `POST /api/v1/auth/refresh` with `{ refreshToken }` to get a new pair. Replace both.
6. On the user pressing "logout", call `POST /api/v1/auth/logout` with `{ refreshToken, deviceToken? }`. Wipe both tokens from storage.

### For testers

1. Open Postman → File → Import → choose `Telesak-Docs/postman/Telesak-MobileAPI.postman_collection.json`.
2. Open the collection's Variables tab — fill `baseUrl`, `username`, `password`.
3. Run requests in folder order: **01 - Auth → Login** first. The test script auto-captures `accessToken` and `refreshToken` so every subsequent request works without you copying tokens by hand.
4. Each folder is a slice of functionality (auth → me → attendance → tasks → notifications). Run top-to-bottom.

---

## 2. Environments + base URLs

| Environment | Base URL | Tenant | JwtIssuer |
|---|---|---|---|
| Local dev | `http://localhost:43305` | Telesak | `Telesak` |
| UAT | TBD by ops | Telesak | `Telesak-UAT` (or whatever ops set) |
| Telesak prod | `http://<telesak-host>` | Telesak | `Telesak` |
| E-snad prod | `http://<esnad-host>` | E-snad | `E-snad` |

Each environment uses a **different `JwtSecret`**. A token issued in dev will not validate in prod (and vice versa) — this is intentional.

---

## 3. Conventions

### URL
- All endpoints are under `/api/v1/`. The `v1` is the API version — never drop it.
- URLs are lowercase, dash-separated (`/check-in`, `/read-all`, `/change-password`, `/device-tokens`).

### Request format
- `Content-Type: application/json; charset=utf-8`.
- Bodies are JSON. Field names are **camelCase** (e.g. `accessToken`, `refreshToken`, `deviceInfo`).
- Dates are **ISO 8601 Gregorian** with `Z` UTC (e.g. `"2026-05-12T14:30:00Z"`). The mobile app handles any Hijri display on its side.

### Response format — standard envelope
Every endpoint returns an `ApiResponse` envelope (or `PagedResponse` for lists):

```json
{
  "success": true,
  "data": { ... } | [ ... ] | null,
  "message": "Arabic message — may be empty",
  "errors": [ { "field": "...", "message": "..." } ]
}
```

For paginated lists, three extra fields are present:

```json
{
  "success": true,
  "data": [ ... ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 147,
  "totalPages": 8,
  "message": "",
  "errors": []
}
```

On failure responses, the envelope also includes an optional **`code`** field for machine-readable error handling. Use the `code` field — not the Arabic `message` — when branching in code:

```json
{
  "success": false,
  "data": null,
  "message": "تم إيقاف الحساب",
  "code": "ACCOUNT_STOPPED",
  "errors": []
}
```

### HTTP status codes

| Code | Meaning |
|---|---|
| 200 | Success |
| 201 | Created (POST /tasks) |
| 400 | Bad request body or invalid field |
| 401 | Authentication failed (no token / bad token / refresh failure) |
| 403 | Authenticated but not authorized for this resource |
| 404 | Resource not found |
| 409 | Conflict — usually an illegal state transition |
| 500 | Server error (logged to backend; client should retry with backoff) |

### Pagination
- `?page=1&pageSize=20` query params.
- `page` defaults to 1 if omitted; values < 1 are clamped to 1.
- `pageSize` defaults to 20, maximum 100. Higher values are silently clamped to 100.

### Errors language
- All `message` fields are **Arabic only**. The mobile app is responsible for any localization if it ever needs English.
- Error `code` values are always English snake-case (e.g. `INVALID_CREDENTIALS`, `TASK_NOT_FOUND`).

---

## 4. Authentication & token lifecycle

### Tokens

| Token | Lifetime | Purpose |
|---|---|---|
| **Access token** | 60 minutes (configurable) | Sent in `Authorization: Bearer <token>` on every authenticated request. JWT HS256 signed. |
| **Refresh token** | 30 days | Long-lived secret used only to mint a new access token when the current one expires. Stored on the server as SHA-256 hash; plaintext leaves the server exactly once at login/refresh. |

### Roles

The API serves two roles only — the same two the web app serves to end users:

| `userTypeId` (JWT claim) | Role | Description |
|---|---|---|
| `2` | Employee | Can list/accept/reject/complete tasks assigned to them, log time, manage attendance. |
| `3` | Company | Can list company-wide tasks, create + assign + approve + disapprove + soft-delete tasks, view employees. Cannot use attendance endpoints. |

**Admin (`userTypeId = 1`) is NOT exposed via the Mobile API.** Trying to log in with an admin account returns `401 INVALID_CREDENTIALS`.

### JWT payload

The access token is a standard JWT. Decode at [jwt.io](https://jwt.io) if you want to inspect it. Claims:

| Claim | Type | Meaning |
|---|---|---|
| `sub` | string | User id (employee id or company id) |
| `userTypeId` | int | `2` (Employee) or `3` (Company) |
| `companyId` | int? | The user's company id |
| `name` | string | Display name |
| `iss` | string | Issuer (e.g. `Telesak`) |
| `iat` | long | Issued-at (Unix seconds, UTC) |
| `exp` | long | Expiry (Unix seconds, UTC) |
| `jti` | string | Unique token id (prevents caching) |

### Refresh & rotation

- On `POST /auth/refresh`, the OLD refresh token is marked `IsRevoked=1, RevokeReason='Rotated'` and a NEW refresh token is issued. The mobile app must replace both stored tokens.
- **Reuse detection:** if the mobile app accidentally sends an already-rotated refresh token, the server treats it as a token-theft signal and revokes **every** refresh token for that user. The response is `401 REUSE_DETECTED` and the user is forced to log in again.
- Mitigation for legitimate network retries: do NOT retry a failed `/refresh` request without first checking whether the previous one actually succeeded. If you must retry, treat any 401 as "log out and force the user to re-enter credentials".

### Logout

- `POST /auth/logout` revokes the refresh token. The access token remains valid until its natural expiry (typically <60 min) — this is normal for JWT; the cost of a server-side denylist isn't worth the brief tail-end window.
- Pass `deviceToken` in the body to also deactivate the FCM device token for this device. Recommended for **every** logout from a mobile device.

### Sample flow (mobile pseudocode)

```
// Login
POST /api/v1/auth/login { username, password }
→ { accessToken, refreshToken, expiresIn, user }
save(accessToken, refreshToken)

// On every authenticated request
GET /api/v1/tasks
Authorization: Bearer {accessToken}

// On 401 due to expiry
POST /api/v1/auth/refresh { refreshToken }
→ { accessToken: NEW, refreshToken: NEW, ... }
save(NEW)
retry original request

// On 401 REUSE_DETECTED, or 401 INVALID_REFRESH
clear stored tokens
navigate to login screen

// On user logout
POST /api/v1/auth/logout { refreshToken, deviceToken }
clear stored tokens, clear FCM token
```

---

## 5. Endpoint reference

> Every authenticated endpoint requires `Authorization: Bearer <accessToken>`. Endpoints marked **(open)** do not.

---

### 5.1 Auth

#### POST `/api/v1/auth/login` (open)

Request:
```json
{
  "username": "ahmed.ali",
  "password": "P@ssw0rd",
  "deviceInfo": {
    "model": "Pixel 8",
    "platform": "FCM"
  }
}
```

Response 200:
```json
{
  "success": true,
  "data": {
    "accessToken": "eyJhbGc...",
    "refreshToken": "S3cur3Random...",
    "expiresIn": 3600,
    "user": {
      "userId": 42,
      "fullName": "أحمد علي",
      "role": "Employee",
      "companyId": 7,
      "companyName": "...",
      "isStopped": false
    }
  },
  "message": "تم تسجيل الدخول بنجاح",
  "errors": []
}
```

Failure codes: `INVALID_CREDENTIALS` (401), `ACCOUNT_STOPPED` (403), `INVALID_REQUEST` (400).

Side effect: if the user is an Employee, an Attendance row for today is created automatically (mirrors web behavior). The `/attendance/check-in` endpoint is idempotent against this row.

#### POST `/api/v1/auth/refresh` (open)

Request:
```json
{ "refreshToken": "S3cur3Random..." }
```

Response 200:
```json
{
  "success": true,
  "data": {
    "accessToken": "<NEW>",
    "refreshToken": "<NEW>",
    "expiresIn": 3600,
    "user": { ... }
  }
}
```

Failure codes: `INVALID_REFRESH` (401), `REUSE_DETECTED` (401 — all user's refresh tokens are revoked), `ACCOUNT_STOPPED` (401), `INVALID_REQUEST` (400).

#### POST `/api/v1/auth/logout` (auth required)

Request:
```json
{
  "refreshToken": "S3cur3Random...",
  "deviceToken": "fcm-token-from-device"
}
```
`deviceToken` is optional but **strongly recommended** so push stops going to a logged-out phone.

Response 200:
```json
{ "success": true, "data": null, "message": "تم تسجيل الخروج", "errors": [] }
```

---

### 5.2 Me

#### GET `/api/v1/me` (auth)

Response 200:
```json
{
  "success": true,
  "data": {
    "user": {
      "userId": 42,
      "fullName": "أحمد علي",
      "role": "Employee",
      "companyId": 7,
      "companyName": "...",
      "isStopped": false
    },
    "attendance": {
      "hasAttendance": true,
      "attendanceId": 12345,
      "checkIn": "2026-05-12T08:01:23Z",
      "checkOut": null,
      "durationMinutes": 142,
      "isOpen": true
    }
  }
}
```

For Company users, `attendance` is `null`.

#### POST `/api/v1/me/change-password` (auth)

Request:
```json
{
  "oldPassword": "current",
  "newPassword": "newSecret123"
}
```

Response 200: `{ "success": true, "message": "تم تغيير كلمة المرور بنجاح" }`
Failure codes: `INVALID_OLD_PASSWORD` (400), `INVALID_REQUEST` (400).

> If the backend SMTP is unreachable when sending the "password changed" email, the API still returns 200 with message `"تم تغيير كلمة المرور (تعذر إرسال إشعار البريد)"` — the password change itself succeeded.

---

### 5.3 Attendance

> Employee-only. Company users get `403 EMPLOYEE_ONLY` on all POST endpoints. `GET /today` returns `{ "hasAttendance": false }` for Company.

#### POST `/api/v1/attendance/check-in` (auth, Employee)

No body required.

Response 200 (idempotent — returns existing open row if you call again):
```json
{
  "success": true,
  "data": {
    "attendanceId": 12345,
    "checkIn": "2026-05-12T08:01:23Z",
    "isNew": false
  }
}
```

`isNew: false` means an existing open attendance row was returned. `isNew: true` means a new row was just created (e.g. after a previous `/check-out`).

#### POST `/api/v1/attendance/heartbeat` (auth, Employee)

No body required. Updates `LastHeartbeat` on the open attendance row. Call this every 60 seconds while the app is in foreground (mirrors the web's `LogActivity`).

Response 200:
```json
{
  "success": true,
  "data": {
    "attendanceId": 12345,
    "lastHeartbeat": "2026-05-12T10:23:45Z"
  }
}
```

Failure codes: `NO_ACTIVE_ATTENDANCE` (404) if there is no open row today.

> The backend has a job that auto-closes attendance rows where `LastHeartbeat` is older than 15 minutes. If the mobile app crashes or loses connectivity, the user's attendance will be auto-checked-out at their last heartbeat.

#### POST `/api/v1/attendance/check-out` (auth, Employee)

No body required.

Response 200:
```json
{
  "success": true,
  "data": {
    "hasAttendance": true,
    "attendanceId": 12345,
    "checkIn": "2026-05-12T08:01:23Z",
    "checkOut": "2026-05-12T17:32:10Z",
    "durationMinutes": 571,
    "isOpen": false
  },
  "message": "تم تسجيل الخروج"
}
```

Failure codes: `NO_ACTIVE_ATTENDANCE` (404).

#### GET `/api/v1/attendance/today` (auth)

Response 200:
```json
{
  "success": true,
  "data": {
    "hasAttendance": true,
    "attendanceId": 12345,
    "checkIn": "2026-05-12T08:01:23Z",
    "checkOut": null,
    "durationMinutes": 142,
    "isOpen": true
  }
}
```

If no row today: `{ "hasAttendance": false }`. For Company users: same response.

---

### 5.4 Tasks (Employee)

#### GET `/api/v1/tasks` (auth)

Query params:

| Param | Type | Default | Notes |
|---|---|---|---|
| `status` | string | `all` | One of: `all`, `new`, `inprogress`, `done`, `accepted`, `approved`, `notapproved`, `pending`. Unknown → `all`. |
| `page` | int | 1 | 1-based. |
| `pageSize` | int | 20 | Max 100. |

**Scope:** Employee sees tasks where `task.EmpID = self`. Company sees all tasks in `task.CompanyID = self`.

Response 200 (paginated):
```json
{
  "success": true,
  "data": [
    {
      "taskId": 1234,
      "title": "Audit annual report",
      "statusId": 1,
      "statusName": "جديدة",
      "priorityId": 2,
      "priorityName": "متوسط",
      "priorityColor": "#FFAA00",
      "startDate": "2026-05-12T00:00:00Z",
      "endDate": "2026-05-20T00:00:00Z",
      "expectedTime": 8,
      "actualTime": 0,
      "projectName": "...",
      "isDelayed": false,
      "isAccepted": false
    }
  ],
  "page": 1,
  "pageSize": 20,
  "totalCount": 47,
  "totalPages": 3
}
```

#### GET `/api/v1/tasks/{id}` (auth)

Response 200:
```json
{
  "success": true,
  "data": {
    "taskId": 1234,
    "title": "...",
    "description": "...",
    "summary": "...",
    "createdDate": "...",
    "startDate": "...",
    "endDate": "...",
    "deliverDate": null,
    "expectedTime": 8,
    "actualTime": 5.5,
    "timeUnitId": 1,
    "timeUnitName": "ساعة",
    "statusId": 2,
    "statusName": "جارى العمل",
    "priorityId": 2,
    "priorityName": "متوسط",
    "priorityColor": "#FFAA00",
    "empId": 42,
    "empName": "أحمد علي",
    "companyId": 7,
    "companyName": "...",
    "projectId": 99,
    "projectName": "...",
    "isArchived": false,
    "isRecurrence": false,
    "isDelayed": false,
    "statusLog": [
      {
        "logId": 1,
        "statusId": 1,
        "statusName": "جديدة",
        "empId": 42,
        "empName": "أحمد علي",
        "createdDate": "2026-05-01T09:00:00Z",
        "timeCount": null
      }
    ]
  }
}
```

Failure codes: `TASK_NOT_FOUND` (404), `TASK_FORBIDDEN` (403 — not your task).

#### POST `/api/v1/tasks/{id}/accept` (auth, Employee, task assigned to caller)

No body. Wraps `TaskManger.EmpAcceptTask` — sets status to Accepted (or Inprogress if `StartDate <= now`) and sends notifications to employee + company.

Response 200:
```json
{
  "success": true,
  "data": {
    "taskId": 1234,
    "newStatusId": 8,
    "newStatusName": "مقبولة"
  },
  "message": "تم قبول المهمة"
}
```

Failure codes: `TASK_NOT_FOUND` (404), `TASK_NOT_ASSIGNED` (403), `EMPLOYEE_ONLY` (403), `INVALID_STATE` (409 — task not in New status), `ACTION_FAILED` (500).

#### POST `/api/v1/tasks/{id}/reject` (auth, Employee)

Same shape as `/accept`. Wraps `TaskManger.EmpRejectTask` — sets status to Rejected. Only valid from `New` status.

#### POST `/api/v1/tasks/{id}/complete` (auth, Employee)

Same shape. Wraps `TaskManger.EmpFinishTask` — sets status to Done + `DeliverDate = now`. Only valid from `Inprogress` (or `Accepted` — auto-promotes).

#### POST `/api/v1/tasks/{id}/time` (auth, Employee)

Request:
```json
{ "time": 2.5 }
```

Logs `2.5` hours of work for today (updates today's TaskTLog row if it exists, else inserts a new one). Recomputes `Task.ActualTime` server-side as the sum of latest daily logs. Sends notifications.

Response 200 same shape as accept/reject/complete.

Failure codes: `INVALID_REQUEST` (400), `TASK_NOT_FOUND` (404), `TASK_NOT_ASSIGNED` (403), `EMPLOYEE_ONLY` (403).

---

### 5.5 Tasks (Company)

#### POST `/api/v1/tasks` (auth, Company)

Create + assign a new task. The caller's `CompanyID` is implicit (from JWT).

Request:
```json
{
  "title": "Audit Q2 expenses",
  "description": "...",
  "summary": "",
  "briefTaskName": "",
  "priorityId": 2,
  "empId": 42,
  "projectId": 99,
  "startDate": "2026-05-15T08:00:00Z",
  "endDate": "2026-05-25T17:00:00Z",
  "expectedTime": 8,
  "timeUnitId": 1
}
```

Required: `title`, `priorityId`, `empId`. Other fields optional.
Validation: the assigned `empId` must be an active employee in the caller's company; `projectId` (if provided) must belong to the caller's company.

Response 201:
```json
{
  "success": true,
  "data": {
    "taskId": 5678,
    "newStatusId": 1,
    "newStatusName": "جديدة"
  },
  "message": "تم إنشاء المهمة"
}
```

Side effect: notification fires to the assigned employee. TaskTLog row inserted with status=New.

Failure codes: `INVALID_REQUEST` (400), `INVALID_EMPLOYEE` (400), `INVALID_PROJECT` (400), `COMPANY_ONLY` (403).

#### POST `/api/v1/tasks/{id}/approve` (auth, Company)

Wraps `TaskManger.CompanyAcceptTask`. Status: Done → Approved. Notifications sent.
Failure codes: `INVALID_STATE` (409 — only valid from Done), `TASK_FORBIDDEN` (403 — task not in your company), `COMPANY_ONLY`.

#### POST `/api/v1/tasks/{id}/disapprove` (auth, Company)

Wraps `TaskManger.CompanyRejectTask`. Status: Done → NotAproved. Notifications sent.

#### DELETE `/api/v1/tasks/{id}` (auth, Company)

**Soft delete** — sets `Task.IsDeleted = true` and logs to `LogTask`. The task disappears from all `/tasks` list responses (web and mobile) but is preserved in the DB.

Response 200:
```json
{
  "success": true,
  "data": { "taskId": 5678, "newStatusId": ..., "newStatusName": "..." },
  "message": "تم حذف المهمة"
}
```

Failure codes: `TASK_NOT_FOUND` (404 — also returned for already-deleted tasks), `TASK_FORBIDDEN` (403), `COMPANY_ONLY` (403).

---

### 5.6 Projects

#### GET `/api/v1/projects` (auth, both roles)

Returns non-deleted projects in the caller's company. Used as a picker when creating tasks.

Response 200:
```json
{
  "success": true,
  "data": [
    { "projectId": 99, "name": "...", "description": "...", "companyId": 7 }
  ]
}
```

---

### 5.7 Employees (Company)

#### GET `/api/v1/employees` (auth, Company only)

Returns active, non-deleted employees in the caller's company. Used as the assignee picker on the mobile create-task screen.

Response 200:
```json
{
  "success": true,
  "data": [
    { "empId": 42, "name": "...", "jobTitle": "...", "email": "...", "mobile": "..." }
  ]
}
```

Failure codes: `COMPANY_ONLY` (403).

---

### 5.8 Notifications

#### GET `/api/v1/notifications` (auth)

Query: `?page=1&pageSize=20`. Returns the caller's NotificationCollection rows newest-first.

Response 200 (paginated):
```json
{
  "success": true,
  "data": [
    {
      "id": 9999,
      "notificationId": 5555,
      "value": "تم اسناد المهمة 'Audit Q2' لك",
      "link": "/Employee/Tasks/TaskDetails/1234",
      "sendDate": "2026-05-12T10:15:00Z",
      "isSeen": false,
      "type": "NewTask"
    }
  ],
  "page": 1, "pageSize": 20, "totalCount": 47, "totalPages": 3
}
```

`id` is the per-user row id (NotificationCollection.ID). Use this when marking read. `notificationId` is the shared Notification id (you may use it for client-side dedupe across multiple users).

#### POST `/api/v1/notifications/{id}/read` (auth)

Marks the per-user NotificationCollection row as seen. Body not required.

Response 200:
```json
{
  "success": true,
  "data": { "id": 9999, "isSeen": true },
  "message": "تم تعليم الإشعار كمقروء"
}
```

Failure codes: `NOTIFICATION_NOT_FOUND` (404), `NOTIFICATION_FORBIDDEN` (403 — not your notification).

#### POST `/api/v1/notifications/read-all` (auth)

Marks all of the caller's unread notifications as seen.

Response 200:
```json
{
  "success": true,
  "data": { "marked": 12 },
  "message": "تم تعليم جميع الإشعارات كمقروءة"
}
```

---

### 5.9 Device tokens (FCM)

#### POST `/api/v1/device-tokens` (auth)

Register or refresh an FCM device token. Idempotent — calling it twice with the same token updates `LastSeenDate` and reactivates the row if it was previously disabled.

Request:
```json
{
  "token": "fcm-device-token-from-firebase-sdk",
  "platform": "FCM",
  "model": "Pixel 8"
}
```

Response 200: `{ "success": true, "message": "تم تسجيل الجهاز" }`.

Failure codes: `INVALID_REQUEST` (400 if `token` is empty).

> **When to call it:** at app start after a successful login, AND whenever Firebase rotates the token (the SDK fires an `onTokenRefresh` callback).

#### DELETE `/api/v1/device-tokens/{token}` (auth)

Deactivates a specific device token for the calling user. Scoped to the caller — you cannot disable another user's token by guessing the value.

> URL-encode the token if it contains slashes or `+` characters.

Response 200: `{ "success": true, "message": "تم إلغاء تسجيل الجهاز" }`.

---

### 5.10 Comments

> ⚠️ **NOT YET IMPLEMENTED — planned only.** As of 2026-06-30 there is no `CommentsController` and no comment routes in `WebApiConfig.cs`. Every endpoint in this section currently returns **404**. The shape below is the agreed design for when this slice is built — do NOT test against it yet.

> Both roles. Access control mirrors `GET /tasks/{id}` — Employee sees only their own tasks; Company sees all tasks in their company.

#### GET `/api/v1/tasks/{id}/comments` (auth, both roles)

Returns all comments on the task, oldest first. No pagination — comment threads are short.

Response 200:
```json
{
  "success": true,
  "data": [
    {
      "commentId": 1,
      "taskId": 1234,
      "authorId": 42,
      "authorName": "مسؤول الشركة",
      "authorType": "company",
      "body": "dd",
      "createdAt": "2026-05-12T11:16:09Z"
    }
  ],
  "message": "",
  "errors": []
}
```

| Field | Type | Notes |
|---|---|---|
| `commentId` | int | PK |
| `authorType` | string | `"company"` or `"employee"` — use to color the name (gold for company, as in web) |
| `body` | string | Plain text; no HTML |
| `createdAt` | ISO 8601 UTC | Mobile formats for display |

Failure codes: `TASK_NOT_FOUND` (404), `TASK_FORBIDDEN` (403).

#### POST `/api/v1/tasks/{id}/comments` (auth, both roles)

Add a new comment. Author identity is taken from the JWT — no `authorId` in the request body.

Request:
```json
{ "body": "نص التعليق" }
```

`body` is required; empty string → `INVALID_REQUEST` (400).

Response 201:
```json
{
  "success": true,
  "data": {
    "commentId": 2,
    "taskId": 1234,
    "authorId": 42,
    "authorName": "مسؤول الشركة",
    "authorType": "company",
    "body": "نص التعليق",
    "createdAt": "2026-05-12T14:00:00Z"
  },
  "message": "تم إضافة التعليق"
}
```

Failure codes: `INVALID_REQUEST` (400), `TASK_NOT_FOUND` (404), `TASK_FORBIDDEN` (403).

---

### 5.11 Attachments

> ⚠️ **NOT YET IMPLEMENTED — planned only.** As of 2026-06-30 there is no `AttachmentsController` and no attachment routes in `WebApiConfig.cs`. Every endpoint in this section currently returns **404**. The shape below (including the multipart upload contract) is the agreed design for when this slice is built — do NOT test against it yet.

> Both roles. Same access rule as comments — Employee's own tasks only; Company sees all tasks in their company.

#### GET `/api/v1/tasks/{id}/attachments` (auth, both roles)

Returns all attachments on the task.

Response 200:
```json
{
  "success": true,
  "data": [
    {
      "attachmentId": 1,
      "taskId": 1234,
      "fileName": "تقرير مارس 2026.pdf",
      "description": "تقرير مهام شركة عبدالحميد حسن عبدالكريم شهر مارس 2026",
      "fileUrl": "http://<host>/uploads/tasks/1234/abc123.pdf",
      "fileSizeBytes": 204800,
      "uploadedById": 42,
      "uploadedByName": "مسؤول الشركة",
      "uploadedAt": "2026-05-12T11:00:00Z"
    }
  ],
  "message": "",
  "errors": []
}
```

| Field | Type | Notes |
|---|---|---|
| `fileUrl` | string | Full absolute URL — mobile opens it directly with `Linking.openURL()` |
| `fileSizeBytes` | int | Mobile displays as KB / MB |
| `fileName` | string | Original file name including extension |

Failure codes: `TASK_NOT_FOUND` (404), `TASK_FORBIDDEN` (403).

#### POST `/api/v1/tasks/{id}/attachments` (auth, both roles)

Upload a new file. Must be **`multipart/form-data`** — not JSON.

Request fields:

| Field | Required | Notes |
|---|---|---|
| `file` | Yes | Binary file |
| `description` | No | Plain text description (displayed as الوصف) |

Max file size: confirm with backend team (suggest 10 MB). Allowed types: no restriction at the API level — let the mobile file picker guide the user.

Response 201:
```json
{
  "success": true,
  "data": {
    "attachmentId": 2,
    "taskId": 1234,
    "fileName": "report.pdf",
    "description": "تقرير الشهر",
    "fileUrl": "http://<host>/uploads/tasks/1234/xyz789.pdf",
    "fileSizeBytes": 102400,
    "uploadedById": 42,
    "uploadedByName": "مسؤول الشركة",
    "uploadedAt": "2026-05-12T14:05:00Z"
  },
  "message": "تم رفع الملف"
}
```

Failure codes: `INVALID_REQUEST` (400 — no file sent), `FILE_TOO_LARGE` (400), `TASK_NOT_FOUND` (404), `TASK_FORBIDDEN` (403).

---

## 6. Error codes — full catalogue

Always branch on `code`, not on `message`. Codes are stable; Arabic messages may evolve.

| Code | HTTP | Endpoints | What it means | What the app should do |
|---|---|---|---|---|
| `INVALID_REQUEST` | 400 | many | missing required field, malformed JSON, invalid value | Show generic error; let user fix input |
| `INVALID_CREDENTIALS` | 401 | login | wrong username/password OR ApplicationName mismatch (tenant) | Show generic "wrong credentials" |
| `INVALID_REFRESH` | 401 | refresh | refresh token not found / expired | Force re-login |
| `REUSE_DETECTED` | 401 | refresh | revoked refresh token replayed (all user tokens revoked) | Force re-login + alert security |
| `ACCOUNT_STOPPED` | 403 | login, refresh | user or their company is inactive/deleted | Show "account stopped" screen, don't auto-retry |
| `INVALID_OLD_PASSWORD` | 400 | change-password | old password didn't match | Show field error |
| (no token) | 401 | any protected | missing or malformed `Authorization` header | Attempt refresh, then re-login if refresh also fails |
| `EMPLOYEE_ONLY` | 403 | attendance/*, tasks/{id}/accept|reject|complete|time | Company called an employee-only endpoint | Hide the UI for company role |
| `COMPANY_ONLY` | 403 | employees, tasks (POST/DELETE/approve/disapprove) | Employee called a company-only endpoint | Hide the UI for employee role |
| `NO_ACTIVE_ATTENDANCE` | 404 | heartbeat, check-out | no open attendance row for today | Prompt user to check-in first |
| `CHECKIN_FAILED` | 500 | check-in | server failed to create the attendance row | Retry with backoff |
| `TASK_NOT_FOUND` | 404 | tasks/* | task id missing or already soft-deleted | Refresh list |
| `TASK_FORBIDDEN` | 403 | tasks/* | task is in a different company / not your role's scope | Refresh list |
| `TASK_NOT_ASSIGNED` | 403 | tasks/{id}/accept|reject|complete|time | employee tried to mutate a task not assigned to them | Refresh detail |
| `INVALID_STATE` | 409 | tasks/{id}/* | workflow refused — current status doesn't allow this transition | Re-fetch detail and re-render available actions |
| `ACTION_FAILED` | 500 | tasks/{id}/* | `TaskManger.*` returned false unexpectedly | Retry with backoff |
| `INVALID_EMPLOYEE` | 400 | POST /tasks | assignee not in your company / inactive | Refetch employees list |
| `INVALID_PROJECT` | 400 | POST /tasks | project not in your company | Refetch projects list |
| `NOTIFICATION_NOT_FOUND` | 404 | notifications/{id}/read | unknown notification id | Refresh notifications |
| `NOTIFICATION_FORBIDDEN` | 403 | notifications/{id}/read | notification belongs to another user | Refresh notifications |
| `FILE_TOO_LARGE` | 400 | tasks/{id}/attachments (POST) | uploaded file exceeds the server's max size limit | Show error, prompt user to choose a smaller file |
| `SERVER_ERROR` | 500 | any | unhandled exception. Body has detail if `ApiDetailedErrors=true` in dev | Retry with backoff; report to backend if persistent |

---

## 7. Push notifications (FCM)

### How it works

1. Mobile app integrates Firebase SDK and obtains an FCM device token.
2. After login, app calls `POST /api/v1/device-tokens` with the token.
3. The backend stores the token in `MobileDeviceToken` table (one user can have many tokens — phone + tablet).
4. Whenever any backend code calls `NotificationHub.Send(...)` — which already happens for every existing event (task assigned, accepted, completed, approved, time updated, etc.) — the backend automatically fans out the notification to FCM for every active token of every recipient. **No backend changes needed per event.**

### What the mobile app receives

FCM legacy payload:
```json
{
  "notification": {
    "title": "Telesak",
    "body": "تم اسناد المهمة 'Audit Q2' لك"
  },
  "data": {
    "notificationId": "5555",
    "type": "NewTask",
    "link": "/Employee/Tasks/TaskDetails/1234"
  }
}
```

- The body is the Arabic message (truncated at ~180 chars).
- The `data.link` is a relative path the web uses; the mobile app should parse it to decide which screen to open (e.g. `/Employee/Tasks/TaskDetails/{id}` → open task detail screen for `{id}`).
- `data.notificationId` matches the `notificationId` field in `GET /api/v1/notifications` responses — useful for dedupe.

### Token lifecycle

- **Register at:** every successful login.
- **Re-register at:** `onTokenRefresh` events from the Firebase SDK.
- **Unregister at:** logout (pass `deviceToken` in the logout body — `POST /auth/logout` deactivates it). Also unregister via DELETE when the user switches accounts.

### Auto-cleanup of stale tokens

If the backend gets `NotRegistered` / `InvalidRegistration` / `MismatchSenderId` from FCM for a token, that token row is set `IsActive=0` automatically. The app doesn't need to manage this.

### When push doesn't fire

- `FcmServerKey` Web.config value still has the `REPLACE_*` placeholder (dev). The dispatcher silently no-ops. Set a real key to enable push.
- The recipient has no active `MobileDeviceToken` row (they never registered).
- The recipient is the sender (the existing actor-skip guard at `NotificationHub.Send` lines 175/192).

---

## 8. Testing checklist (for QA)

### Prerequisites
- [ ] SQL migration `Telesak-Docs/sql/mobile-api-tables.sql` was run on the target DB.
- [ ] Web.config has a real `JwtSecret` (not the placeholder).
- [ ] Postman collection imported from `Telesak-Docs/postman/Telesak-MobileAPI.postman_collection.json`.
- [ ] Collection variables filled: `baseUrl`, `username`, `password`.

### Critical-path tests (run for every release)

**Auth (Slice 2)**
- [ ] Login with valid creds → 200 + accessToken + refreshToken.
- [ ] Login with wrong password → 401 INVALID_CREDENTIALS.
- [ ] Login with stopped/deleted account → 403 ACCOUNT_STOPPED.
- [ ] Refresh happy path → 200, NEW tokens, OLD refresh row marked Revoked + ReplacedByTokenId set.
- [ ] Refresh reuse detection → 401 REUSE_DETECTED + all user's refresh rows revoked.
- [ ] Logout → 200; subsequent refresh with that token → 401.
- [ ] `/me` returns user + attendance shape correctly for Employee.
- [ ] `/me` returns user only (attendance=null) for Company.
- [ ] `/me/change-password` with wrong old password → 400 INVALID_OLD_PASSWORD.

**Attendance (Slice 3)**
- [ ] Login as Employee → DB has one new Attendance row for today.
- [ ] `/check-in` is idempotent — second call returns `isNew=false` with same `attendanceId`.
- [ ] `/heartbeat` updates `LastHeartbeat` (verify with SQL).
- [ ] `/check-out` sets `CheckOut` (verify with SQL).
- [ ] After check-out, `/heartbeat` returns 404 NO_ACTIVE_ATTENDANCE.
- [ ] As Company: `/check-in` returns 403 EMPLOYEE_ONLY.
- [ ] As Company: `/today` returns `hasAttendance=false`.

**Tasks — Employee (Slice 4)**
- [ ] `/tasks?status=all` returns paginated list scoped to the employee's tasks.
- [ ] `/tasks?status=new|inprogress|done` filters correctly.
- [ ] `/tasks/{id}` returns full detail incl. `statusLog`.
- [ ] `/tasks/{id}/accept` on a New task → 200, status moves to Accepted (or Inprogress if start date <= today). Notification rows created in DB.
- [ ] `/tasks/{id}/complete` on Inprogress → 200, status=Done. Notification rows created.
- [ ] `/tasks/{id}/complete` on a New task → 409 INVALID_STATE.
- [ ] `/tasks/{id}/time` with `{ time: 2.5 }` → 200; DB `TaskTLog` has new row with `TimeCount=2.5`; `Task.ActualTime` recomputed.

**Tasks — Company (Slice 5)**
- [ ] `POST /tasks` with valid body → 201 + new task in DB.
- [ ] `POST /tasks` with empId from a different company → 400 INVALID_EMPLOYEE.
- [ ] `POST /tasks` as Employee → 403 COMPANY_ONLY.
- [ ] `/tasks/{id}/approve` on Done task → 200, status=Approved.
- [ ] `/tasks/{id}/disapprove` on Done task → 200, status=NotAproved.
- [ ] `/tasks/{id}/approve` on New task → 409 INVALID_STATE.
- [ ] `DELETE /tasks/{id}` → 200, DB `IsDeleted=1`.
- [ ] `DELETE /tasks/{id}` second call → 404 TASK_NOT_FOUND.

**Comments (Slice 7) — ⚠️ NOT YET IMPLEMENTED. Skip these tests; the endpoints return 404 today.**
- [ ] `GET /tasks/{id}/comments` as Employee (own task) → 200, array oldest-first, each item has `commentId`, `authorType`, `body`, `createdAt`.
- [ ] `GET /tasks/{id}/comments` as Employee on another employee's task → 403 TASK_FORBIDDEN.
- [ ] `GET /tasks/{id}/comments` as Company → 200, can fetch any task in company.
- [ ] `POST /tasks/{id}/comments` with valid body → 201, new comment in array with correct `authorType`.
- [ ] `POST /tasks/{id}/comments` with empty `body` (`""`) → 400 INVALID_REQUEST.
- [ ] `authorType` is `"company"` when posted by Company user and `"employee"` when posted by Employee.

**Attachments (Slice 7) — ⚠️ NOT YET IMPLEMENTED. Skip these tests; the endpoints return 404 today.**
- [ ] `GET /tasks/{id}/attachments` → 200, array with `fileName`, `fileUrl` (full absolute URL), `fileSizeBytes`, `uploadedByName`.
- [ ] `fileUrl` opens the file directly (verify with browser or `Linking.openURL()`).
- [ ] `POST /tasks/{id}/attachments` as `multipart/form-data` with valid `file` field → 201, response includes `attachmentId`, `fileName`, `fileSizeBytes`.
- [ ] `POST /tasks/{id}/attachments` with `description` field → 201, `description` present in response.
- [ ] `POST /tasks/{id}/attachments` without a `file` field → 400 INVALID_REQUEST.
- [ ] `POST /tasks/{id}/attachments` with file exceeding server max → 400 FILE_TOO_LARGE.
- [ ] Both endpoints return 403 TASK_FORBIDDEN when Employee accesses another employee's task.

**Pickers (Slice 4)**
- [ ] `/projects` returns projects in caller's company only.
- [ ] `/employees` as Company → 200, active employees.
- [ ] `/employees` as Employee → 403 COMPANY_ONLY.

**Notifications + FCM (Slice 6)**
- [ ] `POST /device-tokens` with a token → 200; DB row exists, `IsActive=1`.
- [ ] Re-register same token → 200; DB count stays 1 (LastSeenDate updates).
- [ ] `DELETE /device-tokens/{token}` → 200; DB `IsActive=0`.
- [ ] `GET /notifications?page=1` → 200, paginated.
- [ ] `POST /notifications/{id}/read` → 200; DB `IsSeen=1, SeenDate≈now`.
- [ ] `POST /notifications/read-all` → 200; all caller's unread rows now IsSeen=1.
- [ ] **End-to-end FCM:** as Company, approve a Done task. Verify a debug log line in the server (no errors in `[FcmDispatcher]`). If a real FcmServerKey is set + a real device is registered, the device receives a push.

### Regression (the most important set — run after every Mobile API deploy)

These confirm the **web app is unchanged** — the brief's hardest constraint.

- [ ] Web login as Employee — still works.
- [ ] Web Employee task list — loads and renders Arabic text.
- [ ] Web Employee accept/complete buttons — still trigger correctly. DB shows TaskTLog row.
- [ ] Web Company login — dashboard loads.
- [ ] Web Company task create — saves and assigns notification.
- [ ] Web Company approve/disapprove — works.
- [ ] Web SignalR notification bell — lights up in real time.
- [ ] Web `/Attendance/LogActivity` — heartbeat still fires every 60s from browser.
- [ ] Web `/Notifications` page — lists notifications.
- [ ] Web logout — clears session.

If any regression test fails, **stop and revert** before continuing the Mobile API tests.

---

## 9. Mobile dev FAQ

**Q: What happens to the access token when the user backgrounds the app for 2 hours?**
A: It expires (60 min default). On the next request, you'll get 401. Call `/auth/refresh` with the stored refresh token, replace both, retry. Refresh tokens are valid for 30 days.

**Q: Should I send `Authorization: Bearer ...` on `/auth/login`?**
A: No. Login is open. Refresh is also open. Logout requires the access token (it's how we identify the user to scope the device-token deactivation).

**Q: My token is "Test" but the API returns 401. Why?**
A: Two common reasons: (1) the `JwtSecret` in Web.config is still the `REPLACE_*` placeholder — the validator rejects all tokens in that state. (2) The token's `iss` claim doesn't match the server's `JwtIssuer` setting (different env, different token).

**Q: Why are dates UTC with `Z`? My users are in Riyadh (UTC+3).**
A: The server is UTC. Convert to local time on the mobile side for display. Hijri conversion is also a mobile-side concern.

**Q: The same notification keeps arriving multiple times.**
A: Probably the user has multiple active device tokens (phone + tablet — that's correct). Or stale FCM tokens that haven't been deactivated yet. Mobile dedupe by `data.notificationId` if needed.

**Q: I send a refresh token and immediately get 401 REUSE_DETECTED.**
A: You sent an already-rotated token. Most likely a network retry replayed an old request after the server already issued a new pair. Don't retry `/refresh` — treat any 401 as "force re-login".

**Q: I'm getting 500 with a vague Arabic message. How do I debug?**
A: Ask the backend team to set `<add key="ApiDetailedErrors" value="true" />` in Web.config (dev only). The 500 response will then include the exception type + message + inner exception. **Never** enable this in prod.

**Q: Can I batch multiple actions (e.g. complete 10 tasks at once)?**
A: No — out of scope per the brief. One id per request.

**Q: Do I need to handle SignalR / WebSockets?**
A: No. Mobile only gets push via FCM. The web's SignalR live notifications stay web-only.

**Q: What's the rate limit?**
A: None today. Be reasonable — don't poll `/me` more than once a minute, don't fire `/heartbeat` more than once per 60s. Future versions may add throttling.

---

## 10. Out of scope (do NOT request)

These are explicitly NOT in the Mobile API and won't be added without a new brief:

- Reports / PDF / RDLC export endpoints
- Bulk actions (delete multiple tasks, etc.)
- Forgot password / password reset
- Admin endpoints
- Activity log / audit feed
- Offline / sync support
- Hijri date conversion in API responses (mobile handles it)
- Bilingual error messages (Arabic only)
- TenantId multi-tenancy (single tenant per environment)
- Swagger UI in production (it'd require a WebApi 2 upgrade)

---

**Last updated:** 2026-06-30 — flagged §5.10 Comments and §5.11 Attachments as NOT YET IMPLEMENTED (specified but no controller/routes in code). Previous: 2026-05-12 added those two sections as design specs.
**Maintainer:** backend team. Ping us if anything contradicts what the API actually returns.

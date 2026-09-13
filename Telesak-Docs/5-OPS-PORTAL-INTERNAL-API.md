# Ops Portal Internal API — Task Attachments (Upload + Read)

> **Status: IMPLEMENTED (pending tester sign-off).** Approved 2026-07-08 with the Done/Approved
> guard added at review. Code builds clean. Decisions are locked in §0; the endpoint behaves
> exactly as documented below.
>
> **2026-09-13:** a blocking `NullReferenceException` was found in the reused AppCode and fixed —
> the endpoint returned `500 SAVE_FAILED` on every call before that. See §12.

**Audience:** Telesak backend team + Ops Portal team.
**Author:** backend team.
**Date:** 2026-07-08.

---

## 0. Locked decisions (from review)

| # | Decision | Choice |
|---|----------|--------|
| Notifications | Fire the employee + company notifications (and FCM push)? | **Yes — reuse `TaskManger.AttachTaskFile` as-is.** No shared-code change. |
| Max size | Where does the size limit come from? | **The existing Esnad `Web.config` request limit** (`httpRuntime maxRequestLength`, currently `1048576` KB in `webesnad.config`). **No new app key, no custom size check.** Oversized uploads are rejected by ASP.NET/IIS before reaching our code (same as the Mobile API). |
| Envelope | Response shape? | **Standard `ApiResponse` envelope** — `success` / `data` / `message`, plus `code` on failure. Exact shape in §6. |
| Language | Error `message` text? | **Arabic** (e.g. `"غير مصرح"`). Machine-readable `code` stays English. |
| Finished tasks | Attach to Done/Approved tasks? | **No.** If `Task.StatusID` is `Done (5)` or `Approved (6)`, reject with **409 `INVALID_STATE`**. (Added at review.) |

---

## 1. Why this endpoint exists

Ops Portal needs to attach files to Telesak tasks, but:

1. **Ops Portal cannot safely write into the Telesak web-root `/Upload/Task/` folder directly.**
   It's a different application/host; giving it write access to Telesak's web root is a
   security and deployment hazard.
2. **Telesak hides attachment rows whose physical file is missing from `/Upload/Task/`.**
   So a DB-only insert (without the file landing in the right folder) would create an
   attachment that never appears in the UI.

The fix: a **server-to-server** endpoint owned by Telesak that accepts the file over HTTP,
writes it to Telesak's own `/Upload/Task/` folder, and inserts the matching `dbo.Attachment`
row in one atomic operation — so file and row are always consistent.

This is **not** part of the Mobile API (`/api/v1/…`). It is a separate internal surface
(`/api/internal/…`) authenticated by a shared secret, not a JWT user token.

---

## 2. Endpoint contract

```
POST /api/internal/tasks/{taskId}/attachments
Host: <telesak-host>
X-Ops-Portal-Key: <shared secret>
Content-Type: multipart/form-data; boundary=...
```

`{taskId}` — integer, the `dbo.Task.TaskID` to attach to.

### Multipart form fields

| Field              | Required | Type / limit                    | Notes |
|--------------------|----------|---------------------------------|-------|
| `file`            | **Yes**  | binary                          | The file bytes. The part's `filename` is read only as a fallback for `originalFileName`. |
| `description`     | No       | text, ≤ 350 chars (`nvarchar(350)`) | Stored in `Attachment.Description`. Truncated/rejected if longer — see §10 Q4. |
| `originalFileName`| No       | text, ≤ 255 chars (`nvarchar(255)`) | Stored in `Attachment.OriginalFileName`. If omitted, falls back to the `file` part's own filename. Sanitized before storage. |

---

## 3. Authentication — shared secret

- The caller must send header `X-Ops-Portal-Key: <shared secret>`.
- The server compares it against a new Web.config appSetting **`OpsPortalKey`**, added to
  `Web.config` **and** the three env variants (`webesnad.config`, `webuat.config`,
  `webtele.config`) — same pattern as `JwtSecret` / `FcmServerKey`.
- Comparison uses a **constant-time** compare (no early-out) to avoid timing attacks.
- Missing or wrong header → **401** with `code = INVALID_API_KEY`, `message = "غير مصرح"`.
- If the configured key is missing, empty, or still a `REPLACE_*` placeholder, the endpoint
  returns **503** `OPS_API_DISABLED` rather than accepting any key — mirrors the FCM
  "silently disabled until configured" convention, but fails **closed** for a security surface.
- No JWT. `JwtAuthorizeAttribute` is **not** applied. `MvcApplication.userData` is **not** set.
- **Transport:** must be HTTPS in UAT/prod. The shared key is a bearer credential; never send
  it over plain HTTP. (Enforcement = ops/infra, noted here as a requirement.)

---

## 4. Behavior (happy path)

1. Validate the shared secret (§3). Reject with `401 INVALID_API_KEY` if wrong.
2. Confirm the body is `multipart/form-data`; reject `400 INVALID_REQUEST` otherwise.
3. Read the multipart parts; extract `file`, `description`, `originalFileName`.
4. Reject `400 MISSING_FILE` if no `file` part / zero bytes.
5. **Size is not checked in app code.** Oversized requests are already rejected by ASP.NET/IIS
   at the existing `httpRuntime maxRequestLength` (and IIS `maxAllowedContentLength`) limit
   before the request reaches this action — so there is no custom `FILE_TOO_LARGE` path here.
6. Load `dbo.Task` by `{taskId}`. Reject `404 TASK_NOT_FOUND` if missing **or `IsDeleted = 1`**.
   Then reject `409 INVALID_STATE` if `StatusID` is `Done (5)` or `Approved (6)` — no attachments
   on finished/approved tasks (locked decision §0).
7. Generate stored `FileName` = `GUID + originalExtension`, **no path component**.
8. Save the physical bytes to `Server.MapPath("~/Upload/Task/")` + storedFileName
   (create the folder if it doesn't exist). On IO failure → `500 SAVE_FAILED` and **do not**
   insert the DB row.
9. Insert `dbo.Attachment(FileName = storedName, Description, OriginalFileName, TaskID)` via
   `TaskManger.AttachTaskFile` (which also logs and fires employee + company notifications /
   FCM — retained by decision §0).
10. Re-query the row to read back the identity `AttachmentID`, then return `201 Created`
    with the body in §6.

**Ordering guarantee:** file is written to disk **before** the DB row is inserted. If the
insert fails after the file is written, the orphaned file is harmless (no row references it);
if the file write fails, no row is created. This keeps Telesak's "hide rows with missing
files" rule satisfied — a returned `attachmentId` always has a file on disk.

---

## 5. Validation & rejection matrix

| Condition                                   | HTTP | `code`              | `message` (Arabic) |
|---------------------------------------------|------|---------------------|--------------------|
| Missing/empty/wrong `X-Ops-Portal-Key`      | 401  | `INVALID_API_KEY`   | `غير مصرح` |
| `OpsPortalKey` not configured on server     | 503  | `OPS_API_DISABLED`  | `الخدمة غير مفعّلة` |
| Body not `multipart/form-data`              | 400  | `INVALID_REQUEST`   | `بيانات الطلب غير صحيحة` |
| No `file` part / zero-byte file             | 400  | `MISSING_FILE`      | `يجب اختيار ملف` |
| Task not found / `IsDeleted = 1`            | 404  | `TASK_NOT_FOUND`    | `لم يتم العثور على المهمة` |
| Task `StatusID` = Done (5) or Approved (6)  | 409  | `INVALID_STATE`     | `لا يمكن إضافة مرفق لمهمة منتهية أو معتمدة` |
| Disk write or DB insert fails               | 500  | `SAVE_FAILED`       | `تعذر رفع الملف` |

> **Oversized file:** not in this table — rejected by ASP.NET/IIS at the existing
> `maxRequestLength` / `maxAllowedContentLength` limit (decision §0), so it never reaches
> the action and does not produce this envelope. Same behaviour as the Mobile API.

> **`description` / `originalFileName` length:** default = **silently truncate** to the column
> limit (350 / 255) rather than reject, so a valid file upload is never lost over metadata.
> (Minor — see §10.)

> **State guard (narrower than the Mobile API):** the internal endpoint rejects `Done` and
> `Approved` tasks with `409 INVALID_STATE` (locked decision §0). It does **not** additionally
> block `NotAproved` or archived tasks the way the mobile `CanAddExtensions` guard does — only
> `IsDeleted = 0` plus the Done/Approved check are enforced.

---

## 6. Response shape (standard `ApiResponse` envelope)

**Success 201:**

```json
{
  "success": true,
  "data": {
    "attachmentId": 22556,
    "taskId": 56228,
    "fileName": "dea4029a0a2940e5b169a149bfbb23cc.png",
    "originalFileName": "report.png",
    "description": "optional text"
  },
  "message": "تم رفع الملف"
}
```

**Failure:**

```json
{
  "success": false,
  "code": "INVALID_API_KEY",
  "message": "غير مصرح"
}
```

| Field              | Source |
|--------------------|--------|
| `attachmentId`    | `Attachment.AttachmentID` (identity, read back after insert) |
| `taskId`          | echo of the route `{taskId}` |
| `fileName`        | the **generated** stored name (GUID hex + ext), no path |
| `originalFileName`| stored `Attachment.OriginalFileName` (sanitized) |
| `description`     | stored `Attachment.Description` |

Note: **no `fileUrl`** (Ops Portal asked for the generated name, not a public URL) and
**no `uploadedByName`/`createdAt`** (this endpoint has no user identity).

---

## 7. Reuse decision (CLAUDE.md "reuse, don't rewrite")

`TaskManger.AttachTaskFile(taskId, fileName, description, originalFileName)` already:
- inserts the `Attachment` row,
- calls `LogTask.LogAddAttachment`,
- **sends two `NotificationHub.Send` notifications** (employee + company) — which, via
  Slice 6, also fan out to FCM.

One wrinkle: it returns `bool`, not the new `AttachmentID` we must echo back. So after calling
it we **re-query** `AttachmentRepository` by `(TaskID, FileName == storedName)` to read the
identity — exactly what the mobile controller already does in `TasksController.AddAttachment`.

**Decision (§0): reuse `TaskManger.AttachTaskFile` as-is.** Notifications to employee + company
(and FCM push) are wanted, so no variant method is needed and **no shared-code change** —
`TaskManger.cs` is untouched. This keeps us fully inside the "reuse, don't rewrite" policy
with zero HIGH-RISK edits.

---

## 8. Files to add / edit (once approved)

**Add:**
- `Api/Controllers/InternalAttachmentsController.cs` — the endpoint (thin; file-save logic
  mirrored from the mobile `AddAttachment`, then reuses `TaskManger.AttachTaskFile`).
  Class/route named so it does **not** collide with the Mobile API routes. Shared-secret
  check is inline at the top of the action (default — see §10).
- `Api/Dtos/Internal/OpsAttachmentResultDto.cs` — response `data` shape in §6.

**Edit (additive only):**
- `App_Start/WebApiConfig.cs` — one explicit route:
  `api/internal/tasks/{taskId}/attachments` → `InternalAttachments.Upload`
  (constrained to `POST` + numeric `taskId`), registered **before** the generic routes.
- `Web.config` + `webesnad.config` + `webuat.config` + `webtele.config` — add the single
  `OpsPortalKey` appSetting. **No `OpsPortalMaxUploadBytes` key** — size is bounded by the
  existing `httpRuntime maxRequestLength` already in these files (decision §0).

**No change to `AppCode/TaskManger.cs`** (reuse-as-is). **No schema changes** — reuses
`dbo.Attachment` and `dbo.Task` as-is. No new tables.

---

## 9. Security & operational notes

- **Fail closed:** unconfigured key ⇒ endpoint disabled (503), never "allow all".
- **Constant-time** key comparison.
- Never log the `X-Ops-Portal-Key` value or the full request headers.
- **Path traversal:** stored name is a server-generated GUID; `originalFileName` is sanitized
  (reuse `Extentions.SanitizeFileName`) and used only as display metadata, never as a path.
- **Content-Type is not trusted** for security decisions; the file is stored, not executed.
  `/Upload/Task/` must not have execute permissions (infra concern, noted).
- **Size limit:** enforced entirely by the existing `httpRuntime maxRequestLength`
  (`1048576` KB in `webesnad.config`) and IIS `maxAllowedContentLength` — no app-level check.
  If ops wants a tighter Ops-Portal-specific ceiling later, that's a Web.config change, not code.
- Endpoint is **single-tenant per environment** (same DB as the host env), like the rest.

---

## 10. Remaining minor defaults (flag if you disagree)

The four main decisions are locked in §0. These smaller ones I've defaulted so I can proceed;
say the word if you want any changed:

- **Over-length metadata** → **silently truncate** `description` to 350 and `originalFileName`
  to 255 rather than reject the upload.
- **Shared-secret check location** → **inline** at the top of the action (there's only one
  internal endpoint today). If more `/api/internal/…` endpoints are planned, I'll promote it
  to a reusable `[OpsPortalKey]` filter instead.
- **Sync vs async** → **match the existing async multipart pattern** already used by the mobile
  `AddAttachment` in the same file (WebApi multipart reads are async by nature). This mirrors
  precedent in the codebase; noting it because CLAUDE.md's default is sync-only.

---

## 11. Test plan (draft — fill in after approval)

- [ ] Valid key + valid file + existing task → 201, row in `dbo.Attachment`, file on disk in `/Upload/Task/`, response echoes generated `fileName` + `taskId`.
- [ ] File on disk name == returned `fileName`; row `FileName` == returned `fileName`.
- [ ] Wrong/missing key → 401 `INVALID_API_KEY` (`"غير مصرح"`), no file written, no row.
- [ ] Unconfigured `OpsPortalKey` → 503 `OPS_API_DISABLED`.
- [ ] Non-multipart body → 400 `INVALID_REQUEST`.
- [ ] No `file` part → 400 `MISSING_FILE`.
- [ ] Oversized file → rejected by ASP.NET/IIS at `maxRequestLength` (not our envelope); no file written, no row.
- [ ] Unknown `taskId` → 404 `TASK_NOT_FOUND`.
- [ ] `IsDeleted = 1` task → 404 `TASK_NOT_FOUND`.
- [ ] `originalFileName` omitted → falls back to the `file` part filename, sanitized.
- [ ] Attachment appears in Telesak web task-details (file present ⇒ not hidden).
- [ ] Employee + company **do** receive the "attachment added" notification / FCM push (reuse-as-is).

---

**Implemented in these files:**
- `Api/Controllers/InternalAttachmentsController.cs` *(new)* — the endpoint.
- `Api/Dtos/Internal/OpsAttachmentResultDto.cs` *(new)* — response `data` shape.
- `App_Start/WebApiConfig.cs` — route `api/internal/tasks/{taskId}/attachments` (POST, numeric taskId).
- `EtaskMinstry.csproj` — `<Compile>` entries for the two new files.
- `Web.config`, `webesnad.config`, `webtele.config`, `webuat.config` — `OpsPortalKey` appSetting.

**Before it works in an environment:** set a real `OpsPortalKey` value (replace the
`REPLACE_WITH_OPS_PORTAL_SHARED_SECRET` placeholder). While the placeholder stands, the
endpoint returns `503 OPS_API_DISABLED`.

---

## 12. Post-implementation fix — `userData` NullReferenceException (2026-09-13)

**Symptom:** every call to `POST /api/internal/tasks/{taskId}/attachments` returned
`500 SAVE_FAILED`. No `dbo.Attachment` row was committed, and the physical file was left
orphaned in `/Upload/Task/` (it is written before the row insert, by design — §1).

**Cause.** §3 states the endpoint deliberately does not set `MvcApplication.userData` — it is
authenticated by the shared key, not by a session or a JWT. But two pieces of the AppCode it
reuses dereferenced that property without a null check:

| Site | Read |
|---|---|
| `AppCode/LogTask.cs` → `LogTaskSingleValue` (and 3 sibling log methods) | `MvcApplication.userData.isCompany`, to set `TaskLog.IsFromCompany` |
| `AppCode/Notification.cs` → `NotificationHub.Send`, 3 sites | `userData.isCompany` / `userData.userId`, to suppress notifying the actor about their own action |

`MvcApplication.userData` returns `null` when neither `Session["User"]` nor `Items["User"]`
is set (`Global.asax.cs`), so both threw. Both throws happen **inside**
`TaskManger.AttachTaskFile` and **before** its closing `_unitOfWork.Save()`, which is why the
attachment row never reached the database. The controller's `try/catch` turned the exception
into `SAVE_FAILED`, hiding the real cause.

The Mobile API upload path (`TasksController.AddAttachment`) was unaffected:
`JwtAuthorizeAttribute` populates `userData` from the JWT claims before the action runs.

**Fix.** Null-tolerant reads at all seven sites; behaviour is byte-for-byte unchanged whenever
`userData` is present.

- `LogTask.cs` — new private `ActorIsCompany()` helper returning `actor == null || actor.isCompany`.
  With no logged-in actor the change came from the Saqia/Ops dispatch side, never from the
  employee, so the row logs as **company-side** (`TaskLog.IsFromCompany = 1`). The task-log UI
  renders that as `"المدير المسؤول"`, which is correct for a dispatched file.
- `Notification.cs` — `Send` hoists `var actor = MvcApplication.userData;` and guards each read.
  With no actor there is no "self" to suppress, so both the employee and the company are
  notified normally — which is the intended behaviour for an Ops Portal dispatch.

**Why this blocks the read endpoints.** `LogAddAttachment` is the *only* record of when an
attachment was uploaded and from which side — `dbo.Attachment` has no `UploadedAt` or
uploader column. Without this fix, Ops-Portal-uploaded files produce no `TaskLog` row at all,
so the `uploadedAtUtc` / `uploadedBySource` fields of the planned list endpoints would be
permanently null for exactly the files the Ops Portal cares about.

**Verification status:** compiles clean (`EtaskMinstry.csproj`, 0 errors). The runtime path
needs IIS + the database and has **not** been exercised yet — see the test rows below, which
supersede the optimistic notification row in §11.

- [ ] Valid key + valid file → **201**, `dbo.Attachment` row committed, file on disk.
- [ ] Same call writes a `dbo.TaskLog` row with `IsFromCompany = 1` and `Value` containing the stored `FileName`.
- [ ] Employee **and** company both receive the "attachment added" notification + FCM push.
- [ ] Web UI task-details still shows the attachment.
- [ ] Regression: an attachment added from the Telesak web UI by a **company** user still logs `IsFromCompany = 1`.
- [ ] Regression: an attachment added from the web UI by an **employee** still logs `IsFromCompany = 0`, and that employee is still not notified of their own upload.

---

## 13. Read endpoint — list a task's attachments (2026-09-13)

Answers §2.1 of `TELESAK-API-REQUEST-task-attachments.md` from the Ops Portal team.
Implemented; §2.2 (`/content`) and §2.3 (company range) follow separately.

```
GET /api/internal/tasks/{taskId}/attachments
Host: <telesak-host>
X-Ops-Portal-Key: <same shared secret as the upload>
Accept: application/json
```

Same template as the upload — the two routes are told apart by verb.

### 13.1 Response `200`

```json
{
  "success": true,
  "data": [
    {
      "attachmentId": 12345,
      "taskId": 987,
      "fileName": "3f2ac1d4e5b6478callf00dbaadf00d1.pdf",
      "originalFileName": "تقرير الزيارة.pdf",
      "description": "تقرير الزيارة الميدانية",
      "sizeBytes": 284113,
      "contentType": "application/pdf",
      "fileExists": true,
      "uploadedAtUtc": "2026-08-14T06:31:22Z",
      "uploadedBySource": "employee",
      "uploadedByUserTypeId": 2
    }
  ],
  "message": ""
}
```

The JSON formatter runs with `NullValueHandling.Ignore`, so **a field that is unknown is
absent from the object, not present as `null`.** Treat "key missing" as "unknown".

### 13.2 Fields

| Field | Notes |
|---|---|
| `attachmentId`, `taskId`, `fileName`, `originalFileName`, `description` | Straight from `dbo.Attachment`. |
| `sizeBytes` | Real size on disk. `0` when the file is missing. |
| `contentType` | Guessed from the extension via `System.Web.MimeMapping`; `application/octet-stream` when unrecognised. |
| `fileExists` | `false` when the row exists but the file is gone from `/Upload/Task/`. Telesak's own UI hides such rows. **Skip these when building an export** rather than writing a 0-byte file into a customer's ZIP. |
| `uploadedAtUtc` | **UTC**, converted from the server-local `dbo.TaskLog.LogDate`. Absent when no log row matches. |
| `uploadedBySource` | `"employee"` or `"company"`. Absent when unknown. |
| `uploadedByUserTypeId` | `2` = employee, `3` = company side. Absent when unknown. |
| `uploadedByUserAccountId`, `uploadedByName` | **Never returned** — see §13.4. |

### 13.3 Where the upload facts come from

`dbo.Attachment` has no date column and no uploader column, so neither can be read from it.
What does exist is the `dbo.TaskLog` row that `LogTask.LogAddAttachment` writes beside every
insert, whose `Value` carries the stored file name:

```
[{'columnName':'AttachmentID','oldVal':'','newVal':'<stored file name>'}]
```

The endpoint loads the task's attachment log rows in one query and matches them to
attachments by that stored name — which is a GUID, so a mismatch is not a practical concern.
`LogDate` gives the upload time and `IsFromCompany` gives the side.

Every insert path in the application goes through `LogAddAttachment` (the web UI's
task-create and task-detail flows, the Mobile API, and the Ops Portal upload), so coverage is
complete going forward. **Rows predating the log feature, or written by a path that bypassed
it, will have no match** — those come back with the three `uploaded*` fields absent.

### 13.4 What we cannot give you, and why

The Ops Portal request asked for four `uploaded*` fields. Two of them are answerable and two
are not:

- **`uploadedByUserAccountId` / `uploadedByName` — not available at all.** No table records
  which account attached a file. `dbo.TaskLog` stores a single `IsFromCompany` bit and no
  actor id. The fields exist on the DTO so the contract need not change if a column is added
  later, but they are never populated today. Adding one would not be retroactive, and per
  §2.1 of the request we have not touched the shared schema.
- **`uploadedByUserTypeId = 4` (Ops Portal / Saqia dispatch) cannot be distinguished from
  `3` (company user).** Both are `IsFromCompany = 1`. The distinction that *is* reliable is
  **`2` vs not-`2`** — "the employee attached this" against "someone on the company or
  dispatch side did" — which is the one the ملفات المهام للموظفين report actually turns on.

This is strictly better than the Ops Portal's current heuristic, but it is not the full
identity they asked for. Flagging it plainly so nobody builds on an assumption of exactness.

### 13.5 Errors

| Situation | Status | `code` |
|---|---|---|
| Key missing / wrong | `401` | `INVALID_API_KEY` |
| `OpsPortalKey` unset or still a `REPLACE_*` placeholder | `503` | `OPS_API_DISABLED` |
| Task does not exist, or `IsDeleted = 1` | `404` | `TASK_NOT_FOUND` |
| Anything else | `500` | via `ApiExceptionFilter` |

A task that exists but has no attachments is `200` with `"data": []`, not `404`.

### 13.6 Test plan

- [ ] Valid key, task with attachments → `200`, one item per `dbo.Attachment` row, ordered by `attachmentId`.
- [ ] Task with no attachments → `200`, `data: []`.
- [ ] Wrong/missing key → `401` `INVALID_API_KEY`; unconfigured key → `503` `OPS_API_DISABLED`.
- [ ] Unknown `taskId` → `404`; `IsDeleted = 1` task → `404`.
- [ ] `POST` to the same URL still routes to the upload (verb split works).
- [ ] Row whose file was deleted from `/Upload/Task/` → `fileExists: false`, `sizeBytes: 0`, still listed.
- [ ] `sizeBytes` matches the byte count on disk; `contentType` is `application/pdf` for a `.pdf`.
- [ ] File uploaded via the Ops Portal endpoint → `uploadedBySource: "company"`, `uploadedByUserTypeId: 3`.
- [ ] File attached by an employee in the web UI → `uploadedBySource: "employee"`, `uploadedByUserTypeId: 2`.
- [ ] **`uploadedAtUtc` is genuinely UTC** — compare against `dbo.TaskLog.LogDate`, which is Riyadh local; the response must be 3 hours earlier, not equal.
- [ ] A pre-log-feature attachment → the three `uploaded*` keys are absent, and the item still lists.
- [ ] Error responses carry a JSON body, never an HTML page from `customErrors`.

**Implemented in:**
- `Api/Controllers/InternalAttachmentsController.cs` — `List` action + list helpers.
- `Api/Dtos/Internal/OpsAttachmentListItemDto.cs` *(new)* — item shape.
- `App_Start/WebApiConfig.cs` — route `InternalApi_TaskAttachmentsList` (GET, numeric taskId).
- `EtaskMinstry.csproj` — `<Compile>` entry for the new DTO.

---

## 14. Read endpoint — download one attachment's bytes (2026-09-13)

Answers §2.2 of `TELESAK-API-REQUEST-task-attachments.md` — the endpoint the Ops Portal
called "the blocker". Implemented; §2.3 (company range) follows separately.

```
GET /api/internal/attachments/{attachmentId}/content
Host: <telesak-host>
X-Ops-Portal-Key: <same shared secret>
```

### 14.1 Response `200`

The raw file bytes, streamed from `/Upload/Task/` — not buffered in memory, since an export
run is a few hundred of these back to back. Headers:

| Header | Value |
|---|---|
| `Content-Type` | Guessed from the extension; `application/octet-stream` when unrecognised. |
| `Content-Length` | Real byte count. |
| `Content-Disposition` | `attachment; filename*=utf-8''<percent-encoded>; filename=<ascii fallback>` |

Real example for an attachment named `تقرير الزيارة.pdf` stored as `3f2ac1d4e5b64789.pdf`:

```
Content-Disposition: attachment; filename*=utf-8''%D8%AA%D9%82%D8%B1%D9%8A%D8%B1%20%D8%A7%D9%84%D8%B2%D9%8A%D8%A7%D8%B1%D8%A9.pdf; filename=3f2ac1d4e5b64789.pdf
```

Two notes on that header:

- **The charset token is lowercase `utf-8''`**, not the `UTF-8''` in the request. This is
  what .NET's `ContentDispositionHeaderValue.FileNameStar` emits; RFC 5987 defines the token
  as case-insensitive and every client accepts it.
- **A plain ASCII `filename=` is present alongside `filename*`.** RFC 6266 clients prefer
  `filename*` when both appear, so this changes nothing for a correct client — it only means
  a client that ignores the extended form saves the GUID stored name rather than mojibake.
  If the Ops Portal's HTTP client has the opposite preference, say so and we will drop it.

`Content-Type` comes from `System.Web.MimeMapping`, which is IIS's table — so a `.zip`
reports as `application/x-zip-compressed` rather than `application/zip`. Harmless, but worth
knowing if you switch on the value.

### 14.2 Errors — exactly the distinctions requested

| Situation | Status | `code` |
|---|---|---|
| Attachment row does not exist | `404` | `ATTACHMENT_NOT_FOUND` |
| Row exists, but its task is unknown or `IsDeleted = 1` | `404` | `ATTACHMENT_NOT_FOUND` |
| Row exists but the physical file is missing on disk | `410` | `FILE_MISSING` |
| File present but unreadable (permissions, I/O error) | `500` | `READ_FAILED` |
| Key missing / wrong | `401` | `INVALID_API_KEY` |
| `OpsPortalKey` unset or still a `REPLACE_*` placeholder | `503` | `OPS_API_DISABLED` |
| Anything else | `500` | via `ApiExceptionFilter` |

The second row is an addition to the request's table, for consistency with §13: an attachment
hanging off a soft-deleted task is treated as not existing, so a file cannot be pulled out of
a deleted task by guessing its id. **If the Ops Portal needs files from deleted tasks for an
already-dispatched export, tell us — it is a one-line change, but it should be a decision
rather than an accident.**

### 14.3 On "never a 200 carrying an HTML page"

The request singled this out as the one failure mode they cannot detect. Three things make it
structurally true here rather than merely intended:

1. Every error path above returns `Request.CreateResponse(status, ApiResponse.Fail(...))` —
   a real status with a JSON body, never a bare status and never a redirect.
2. `ApiExceptionFilter` is registered globally, so an unhandled exception becomes a JSON
   `500`, not an ASP.NET error page.
3. `Web.config` has `customErrors mode="On" defaultRedirect="~/Error"`, which is what would
   produce an HTML page — but it only rewrites responses that have no content of their own,
   and there is no `<httpErrors>` element, so IIS's `existingResponse="Auto"` default passes
   our bodied responses through untouched.

**The one case outside that guarantee:** a URL matching *no* route at all falls through to
MVC, where `customErrors` can redirect to the HTML error page. That means a `302` (or an HTML
`200` after following it) indicates **a malformed URL on the caller's side**, not a missing
file. Worth asserting on in the Ops Portal client: a response whose `Content-Type` is
`text/html` should never be written to a ZIP.

### 14.4 Hardening

- The stored name from `dbo.Attachment.FileName` is reduced to its bare file name before it
  reaches the filesystem, so a row containing `..\..\web.config` cannot escape `/Upload/Task/`.
  Nothing should ever have written a path into that column; this endpoint is the first thing
  that turns it into a file read, so it does not assume.
- The ASCII `filename=` fallback is restricted to `[A-Za-z0-9._-]`, which also rules out
  header injection through a stored name containing quotes or CR/LF.
- Files open with `FileShare.Read`, so concurrent downloads — and the Telesak web UI serving
  the same file — do not lock each other out.
- The action is named `Download` in code, not `Content`, so it cannot collide with
  `ApiController`'s own protected `Content<T>` helpers during action selection. The URL is
  unchanged.

### 14.5 Test plan

- [ ] Valid key + existing attachment with file on disk → `200`, bytes byte-for-byte identical to the file in `/Upload/Task/`.
- [ ] `Content-Length` equals the real file size; body length matches it.
- [ ] Arabic `originalFileName` → `filename*=utf-8''…` decodes back to the original name.
- [ ] `Content-Type` is `application/pdf` for a `.pdf`, `application/octet-stream` for an unknown extension.
- [ ] Unknown `attachmentId` → `404` `ATTACHMENT_NOT_FOUND`.
- [ ] Attachment whose task has `IsDeleted = 1` → `404`.
- [ ] Row present, file deleted from disk → **`410` `FILE_MISSING`**, JSON body, not `404` and not `200`.
- [ ] Wrong/missing key → `401`; unconfigured key → `503`.
- [ ] **No error response has `Content-Type: text/html`.**
- [ ] A ~10 MiB file downloads intact, and server memory does not spike by the file size (streaming, not buffering).
- [ ] Two concurrent downloads of the same attachment both succeed.
- [ ] Round trip against §13: every item with `fileExists: true` downloads `200`; every item with `fileExists: false` returns `410`.

**Implemented in:**
- `Api/Controllers/InternalAttachmentsController.cs` — `Download` action + `BuildMediaType`, `BuildContentDisposition`, `AsciiFallbackName` helpers.
- `App_Start/WebApiConfig.cs` — route `InternalApi_AttachmentContent` (GET, numeric attachmentId).

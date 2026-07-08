# Ops Portal Internal API — Task Attachment Upload

> **Status: IMPLEMENTED (pending tester sign-off).** Approved 2026-07-08 with the Done/Approved
> guard added at review. Code builds clean. Decisions are locked in §0; the endpoint behaves
> exactly as documented below.

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

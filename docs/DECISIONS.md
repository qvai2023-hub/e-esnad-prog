# Technical Decisions Log

## Decision Record Format
Each decision includes: Context, Decision, Rationale, and Consequences.

---

## DEC-001: Separate Date from Time in Attendance

**Date:** 2026-03-08
**Session:** a02
**Status:** Implemented

### Context
The attendance report showed full datetime (e.g., `1/8/2026 7:07:16 PM`) in both check-in and check-out columns, making the report cluttered and hard to read.

### Decision
Separate the date into its own column and show only time (HH:mm) in check-in/check-out columns.

### Rationale
- Reduces redundancy (date shown once per row)
- Cleaner report layout
- Easier to scan attendance times

### Implementation
- SP: Added `AttendanceDate`, `CheckInTime`, `CheckOutTime` columns
- RDLC: New date column with `dd/MM/yyyy` format

---

## DEC-002: Dual Duration Format (Minutes + Display)

**Date:** 2026-03-08
**Session:** a02
**Status:** Implemented

### Context
Duration was shown as `HH:MM:SS` string which cannot be summed for totals.

### Decision
Store duration in two formats:
- `DurationMinutes` (INT) - for calculations and SUM
- `DurationDisplay` (VARCHAR) - for display (H:MM format)

### Rationale
- Enables aggregation (SUM of minutes)
- Maintains readable display format
- Supports future features like "total hours per employee"

### Implementation
- SP: Calculate minutes internally, format as `H:MM` for display
- Model: `DurationMinutes` (int?) + `DurationDisplay` (string)

---

## DEC-003: Handle Missing Check-out

**Date:** 2026-03-08
**Session:** a02
**Status:** Implemented

### Context
When employee checks in but doesn't check out, the check-out field was empty (NULL).

### Decision
Display "لم يُسجل" (Not Recorded) instead of empty/blank.

### Rationale
- Clear indication that check-out wasn't recorded
- Better user experience
- Distinguishes from data loading issues

### Implementation
- SP: `ISNULL(CheckOutTime, 'لم يُسجل')`

---

## DEC-004: Employee Name as Header

**Date:** 2026-03-08
**Session:** a02, a03
**Status:** Fully Implemented

### Context
Employee name was repeated on every row in the attendance table.

### Decision
Show employee name once as a group header, with their attendance records listed below.

### Rationale
- Reduces visual clutter
- Groups related data together
- More professional report layout

### Implementation
- RDLC: TablixGroup on EmpID field
- Group Header row with employee name (bold, highlighted)
- Group Footer row with subtotals
- Row number resets per employee using `RowNumber("EmployeeGroup")`

---

## DEC-005: Report Parameters for Date Range

**Date:** 2026-03-08
**Session:** a03
**Status:** Implemented

### Context
Report needed to display the selected date range and period in Arabic.

### Decision
Add three report parameters: StartDate, EndDate, ReportPeriod.

### Rationale
- Shows user the exact date range selected
- Arabic month names for better readability
- Consistent formatting across report

### Implementation
- Controller: Format dates and Arabic month names
- RDLC: Display parameters in report header

---

## DEC-006: Professional Report Structure

**Date:** 2026-03-08
**Session:** a03
**Status:** Implemented

### Context
Report needed a professional, client-ready format.

### Decision
Implement structured report with:
- Header: Logo, company name, period, date range
- Body: Grouped data by employee with 6 columns
- Footer: Totals and disclaimer

### Rationale
- Professional appearance
- Clear data organization
- Easy to print and export

### Implementation
- Complete RDLC rebuild with new structure
- Blue color scheme (#3d85c6)
- RTL support for Arabic text

---

---

## Sprint 1 Decisions

### DEC-007: Client-Side Attendance Tracking with Heartbeat

**Date:** 2026-03-09
**Session:** earlier
**Status:** Implemented

### Context
Employees could check in and forget to check out, leaving orphaned attendance sessions.

### Decision
Implement a heartbeat-based attendance tracking system:
- Client sends periodic heartbeats via `attendance-tracker.js`
- `LastHeartbeat` column added to Attendance table
- Server-side `AutoCheckoutJob` checks for stale heartbeats and auto-checks out

### Rationale
- Ensures attendance records are always closed
- Non-intrusive to user experience
- Server-side fallback for closed browsers

### Implementation
- JS: `attendance-tracker.js` with periodic heartbeat API calls
- C#: `AutoCheckoutJob.cs` runs on app startup
- SQL: `AddLastHeartbeat.sql` migration

---

### DEC-008: Inactivity Checkout with Modal Notification

**Date:** 2026-03-09
**Session:** earlier
**Status:** Implemented

### Context
Users leaving browser open but inactive needed a checkout mechanism.

### Decision
Show a modal popup after 30 seconds of inactivity with countdown timer, then auto-checkout and redirect to logout.

### Rationale
- Gives user a chance to stay active
- Clear visual feedback before checkout
- Prevents false inactive checkouts

---

### DEC-009: Remove UserAgent from Session Validation

**Date:** 2026-03-09
**Session:** earlier
**Status:** Implemented

### Context
`UserAccountVM.cs` was checking UserAgent string for session validation, causing false session invalidation when browser updates changed the agent string.

### Decision
Remove UserAgent check from session validation.

### Rationale
- UserAgent strings change frequently with browser updates
- Was causing legitimate users to be logged out
- Session token alone is sufficient for validation

---

### DEC-010: Hijri/Gregorian Calendar Toggle for Reports

**Date:** 2026-03-16
**Session:** haj1c
**Status:** Implemented

### Context
Task report needed to support both Hijri and Gregorian calendar date display.

### Decision
Add a calendar type toggle in `TaskReportPreperation.cshtml` and use `ConvertDate()` helper in the controller.

### Rationale
- Saudi business requirement for Hijri dates
- User flexibility to switch between calendars
- `CultureInfo("ar-SA")` for Arabic formatting

---

### DEC-011: Add Interaction Column to Task Report

**Date:** 2026-03-16
**Session:** haj1c
**Status:** Implemented

### Context
Task report lacked visibility into employee task interaction status.

### Decision
Add التفاعل (Interaction) column to CompanyTasks report, sourced from updated `sp_CompanyTasks` stored procedure.

### Rationale
- Management needs to see task engagement at a glance
- Color-coded: متفاعل (active) vs غير متفاعل (inactive)
- Integrated into existing report structure

---

## Architecture Decisions

### ADR-001: RDLC for Reports

**Status:** Existing

### Decision
Use RDLC (Report Definition Language Client) for all reports.

### Rationale
- Native .NET integration
- Client-side rendering
- PDF/Excel export support
- No Report Server required

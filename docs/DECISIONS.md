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
**Session:** a02
**Status:** Partially Implemented

### Context
Employee name was repeated on every row in the attendance table.

### Decision
Show employee name once as a group header, with their attendance records listed below.

### Rationale
- Reduces visual clutter
- Groups related data together
- More professional report layout

### Implementation
- RDLC: Employee name added below company name
- Future: Full group header with repeat for each employee

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

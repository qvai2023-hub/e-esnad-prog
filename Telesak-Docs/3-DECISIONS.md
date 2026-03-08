# TELE SAK - Technical Decisions

## Sprint 0 Decisions

---

### DEC-001: Separate Date from Time in Attendance

**Date:** 2026-03-08 | **Task:** A-02 | **Status:** Implemented

#### Context
The attendance report showed full datetime in both check-in and check-out columns, making the report cluttered.

#### Decision
Separate the date into its own column (`AttendanceDate`) and show only time (HH:mm) in check-in/check-out columns.

#### Rationale
- Reduces redundancy
- Cleaner report layout
- Easier to scan attendance times

#### Implementation
```sql
-- SP returns:
AttendanceDate  -- DATE only
CheckInTime     -- HH:mm only
CheckOutTime    -- HH:mm or "لم يُسجل"
```

---

### DEC-002: Dual Duration Format

**Date:** 2026-03-08 | **Task:** A-02 | **Status:** Implemented

#### Context
Duration was shown as `HH:MM:SS` string which cannot be summed for totals.

#### Decision
Store duration in two formats:
- `DurationMinutes` (INT) - for SUM calculations
- `DurationDisplay` (VARCHAR) - for display (H:MM format)

#### Rationale
- Enables SQL aggregation (SUM of minutes)
- Maintains readable display format
- Supports subtotals per employee

#### Implementation
```sql
DurationMinutes = DATEDIFF(MINUTE, CheckIn, CheckOut)
DurationDisplay = CAST(DurationMinutes/60 AS VARCHAR) + ':' + FORMAT(DurationMinutes%60, '00')
```

---

### DEC-003: Handle Missing Check-out

**Date:** 2026-03-08 | **Task:** A-02 | **Status:** Implemented

#### Context
When employee checks in but doesn't check out, the check-out field was empty.

#### Decision
Display "لم يُسجل" (Not Recorded) instead of blank.

#### Rationale
- Clear indication that check-out wasn't recorded
- Better user experience
- Distinguishes from data loading issues

#### Implementation
```sql
ISNULL(CheckOutTime, 'لم يُسجل')
```

---

### DEC-004: Employee Grouping in Report

**Date:** 2026-03-08 | **Task:** A-03 | **Status:** Implemented

#### Context
Employee name was repeated on every row in the attendance table.

#### Decision
Use RDLC TablixGroup to group records by employee with:
- Group Header: Employee name (bold, highlighted)
- Group Footer: Subtotals (days count + total hours)
- Row numbers reset per employee

#### Rationale
- Reduces visual clutter
- Groups related data together
- Professional report layout
- Enables per-employee subtotals

#### Implementation
```xml
<Group Name="EmployeeGroup">
  <GroupExpressions>
    <GroupExpression>=Fields!EmpID.Value</GroupExpression>
  </GroupExpressions>
</Group>
```

---

### DEC-005: Report Parameters for Date Range

**Date:** 2026-03-08 | **Task:** A-03 | **Status:** Implemented

#### Context
Report needed to display the selected date range in header.

#### Decision
Add three report parameters passed from Controller:
- `StartDate` - yyyy/MM/dd format
- `EndDate` - yyyy/MM/dd format
- `ReportPeriod` - Arabic month name(s)

#### Rationale
- Shows user the exact date range selected
- Arabic month names using `CultureInfo("ar-SA")`
- Consistent formatting

#### Implementation
```csharp
CultureInfo arCulture = new CultureInfo("ar-SA");
arCulture.DateTimeFormat.Calendar = new GregorianCalendar();
reportPeriod = fromDt.ToString("MMMM yyyy", arCulture);
```

---

### DEC-006: Professional Report Structure

**Date:** 2026-03-08 | **Task:** A-03 | **Status:** Implemented

#### Context
Report needed a professional, client-ready format.

#### Decision
Implement structured report:
- **Header:** Logo, company name, period, date range
- **Body:** 6 columns with employee grouping
- **Footer:** Total employees + total work days
- **Page Footer:** Print date + page numbers
- **Disclaimer:** Data source attribution

#### Color Scheme
- Primary: `#3d85c6` (Blue)
- Group Header: `#e6f2ff` (Light Blue)
- Group Footer: `#f5f5f5` (Light Gray)

#### Rationale
- Professional appearance
- Clear data organization
- Easy to print and export as PDF

---

## Architecture Decisions

### ADR-001: RDLC for Reports

**Status:** Existing (inherited)

#### Decision
Use RDLC (Report Definition Language Client) for all reports.

#### Rationale
- Native .NET integration
- Client-side rendering
- PDF/Excel export support
- No SQL Server Reporting Services required

---

### ADR-002: ReportAgent Static Class

**Status:** Existing (inherited)

#### Decision
Use `ReportAgent` static class to pass data between Controller and Report.aspx.

#### Rationale
- Simple data transfer mechanism
- Supports DataSources and Parameters
- Existing pattern in codebase

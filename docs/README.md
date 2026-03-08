# E-Esnad Project Documentation

## Overview
E-Esnad is a task management and attendance tracking system for organizations.

## Documentation Files

| File | Description |
|------|-------------|
| [CHANGELOG.md](./CHANGELOG.md) | Version history and changes |
| [SPRINT-TRACKER.md](./SPRINT-TRACKER.md) | Sprint progress and tasks |
| [DECISIONS.md](./DECISIONS.md) | Technical decisions and rationale |

## Project Structure

```
EtaskMinstryWeb/
├── EtaskMinstry/           # Main web application
│   ├── ReportsRDLC/        # RDLC report files
│   └── Views/              # MVC Views
├── TaskManagementModel/    # Entity models and data access
├── TaskManagementRepo/     # Repository layer
└── TaskManagementCore/     # Business logic
```

## Key Features

- Employee attendance tracking (Check-in/Check-out)
- Attendance reports with duration calculation
- Task management
- Company/Organization management

## Reports

### Attendance Report (`Attendance.rdlc`)
- Displays employee attendance records
- Shows date, check-in time, check-out time, and duration
- Groups by employee with company header

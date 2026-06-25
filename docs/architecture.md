## Repository Pattern

The application separates business logic from storage.

Program/API
→ Service
→ Repository
→ Data Store

Current repository implementation uses an in-memory list.
Future implementations may use SQL Server or Azure SQL.

## RequestStatus Values

| Value | Name |
|---:|---|
| 0 | Submitted |
| 1 | InReview |
| 2 | Completed |
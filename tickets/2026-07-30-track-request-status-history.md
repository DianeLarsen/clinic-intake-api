# Track Request Status History

**Status:** Complete  
**Completed:** July 30, 2026  
**Type:** Feature  
**Priority:** Medium

## User story

As clinic staff, I need to view a request's status-change history so I can understand what changed, when it changed, and who made the change.

## Delivered

- Added a `RequestStatusHistory` entity with the request relationship, previous and new `RequestStatus` values, updater name, and UTC timestamp.
- Added the one-to-many navigation from `IntakeRequest` to its history records.
- Created the EF Core migration `20260721204924_AddRequestStatusHistory` to create the history table, request foreign key, and index.
- Updated the status-change path to capture the previous status and save a history record alongside the request update.
- Added `GET /api/v1/requests/{id}/history`.
  - Requires authentication.
  - Returns history only for requests in the caller's clinic.
  - Returns `404 Not Found` for a missing request or a request outside the caller's clinic.
  - Returns `200 OK` with `[]` when the request exists but has no status changes.
  - Returns entries newest first, using timestamp and history ID as the tie-breaker.
- Added `RequestStatusHistoryDto` so API clients receive readable status names rather than enum numbers.
- Updated API documentation for the new authenticated endpoint and its clinic-isolation behavior.

## Acceptance criteria verification

| Criterion | Result |
| --- | --- |
| Records previous status, new status, UTC time, and authenticated user | Complete |
| History is retrievable through an authenticated endpoint | Complete |
| History is isolated to the caller's clinic | Complete |
| Existing status-update behavior continues to work | Verified |
| Database change is created through an EF Core migration | Complete |
| Unit and integration tests cover the feature | Complete |
| API documentation is updated | Complete |

## Test coverage

- Successful status update records the correct history entry.
- Missing request does not create a history entry.
- History models map correctly to response DTOs.
- A user can update a request and retrieve its history.
- History is returned newest first.
- A request with no history returns `200 OK` and an empty array.
- Missing request returns `404 Not Found`.
- A request in another clinic returns `404 Not Found`.
- A request without a bearer token returns `401 Unauthorized`.

**Verification result:** `dotnet build` and `dotnet test` passed with 53 tests passing.

## Delivery

- Feature branch: `feature/request-status-history`
- Pull request: #18, **Track request status history**
- Completed and merged July 30, 2026.

## Deliberately out of scope

An admin-only rule for status downgrades was not added. That is a separate authorization feature, not an innocent little footnote to an audit-history ticket.

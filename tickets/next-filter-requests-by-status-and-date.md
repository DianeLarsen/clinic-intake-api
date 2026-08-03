# Ticket: Filter Intake Requests by Status and Date

**Type:** Feature  
**Priority:** Medium  
**Status:** Ready to start

## User story

As clinic staff, I need to filter intake requests by status and date range so I can quickly find the requests that need attention.

## Acceptance criteria

- `GET /api/v1/requests` accepts optional query parameters for:
  - `status`
  - `fromDate`
  - `toDate`
- When no filters are provided, the endpoint keeps its current behavior.
- When `status` is provided, only requests with that status are returned.
- When a date range is provided, only requests created within that inclusive range are returned.
- Filters can be combined.
- Requests remain limited to the authenticated user’s clinic.
- Invalid status and invalid date-range input return a clear `400 Bad Request` response.
- Unit and integration tests cover filtering, combined filters, invalid input, and clinic isolation.
- Swagger documents the query parameters and responses.

## Work outline

1. Trace the current `GET /api/v1/requests` path through controller, service, and repository.
2. Decide the query-parameter DTO/shape and validation rules.
3. Add filter support through each layer without changing unfiltered behavior.
4. Add focused tests.
5. Update Swagger and handbook documentation.
6. Verify locally, then commit, push, open a PR, and verify deployment after merge.

## Start here

Do not code yet. Find the controller action for `GET /api/v1/requests` and write down:

- its current route and parameters
- the service method it calls
- the repository method that executes the query
- where paging and clinic filtering occur today


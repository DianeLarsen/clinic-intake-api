## Ticket: Track Request Status History

**Type:** Feature
**Priority:** Medium
**User story:**
As clinic staff, I need to view a request’s status-change history so I can understand what changed, when it changed, and who made the change.

### Acceptance criteria

- When a request status changes, the system records:
  - Previous status
  - New status
  - Date/time of the change
  - The authenticated user who made the change

- A request’s history can be retrieved through an authenticated API endpoint.
- A user can view history only for requests in their clinic.
- Existing status-update behavior still works.
- The database change is created through an EF Core migration.
- Unit and integration tests cover the new behavior.
- API documentation is updated.

## Your work outline — in the right order

Don’t write code yet. Work through these stages and tell me what you think at each one; I’ll review your plan without handing you the implementation.

1. Understand the existing behavior

   Find where request status changes today. Trace the route from controller to service to repository/database.

   Questions to answer:
   - Which endpoint updates a status?
     [HttpPut("{id}/status")] Endpoint: PUT /api/v1/requests/{id}/status
   - Controller action:
     RequestsController.UpdateStatus(int id, UpdateRequestStatusDto dto)
   - What DTO does it accept?
     UpdateRequestStatusDto
   - Current service call:
     UpdateStatusAsync(id, dto.Status, clinicId)
   - Where does the authenticated user information currently come from?
     From the JWT bearer token sent by the client. ASP.NET validates the token and places its claims in HttpContext.User, exposed in the controller as User.
   - Where is clinic authorization enforced?
     First by the ClinicMember authorization policy before the controller action runs; then GetRequiredClinicId() reads the validated ClinicId claim. The service also receives clinicId, so it can limit the database operation to that clinic.
   - Authenticated-user path
     JWT bearer token
     → ASP.NET validates token
     → creates HttpContext.User
     → ControllerBase exposes it as User
     → User.GetRequiredClinicId() reads ClinicId
   - Clinic authorization:
     the authorization policy runs before the controller action, then
      User.GetRequiredClinicId() gets the caller’s clinic ID.
     User name: available through: User.Identity?.Name
     in the feature:
     string changedBy = User.Identity?.Name
     ?? throw new InvalidOperationException(
     "The authenticated user does not have a name claim."
     );

2. Decide the data model

   Sketch the new entity/class on paper or in notes.

   Decide:
   - Entity name: RequestStatusHistory
   - Fields and their C# types
     | Field | Type | Purpose |
     | ----------------- | ---------------- | --------------------------------------- |
     | `Id` | `int` | Unique ID for this history record |
     | `IntakeRequestId` | `int` | Foreign key to the request |
     | `IntakeRequest` | `IntakeRequest?` | Navigation property back to its request |
     | `PreviousStatus` | `RequestStatus` | Status before the change |
     | `NewStatus` | `RequestStatus` | Status after the change |
     | `UpdatedBy` | `string?` | Authenticated user who made the change |
     | `ChangedAtUtc` | `DateTime` | UTC time the change happened |
     Current timestamp choice:

public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;

- Relationship to `IntakeRequest`
- Relationship decision:  One IntakeRequest → many RequestStatusHistory records
- Added to IntakeRequest:
   public List<RequestStatusHistory> RequestStatusHistory { get; set; } = [];
- Whether the old/new values should use your existing status enum
- Whether “who changed it” is a user ID, user name, or both

3. Plan the database change

   Before generating anything, identify:
   - Which `DbContext` needs a new `DbSet`
   - Any relationship configuration needed
   - What the migration should create
   - Whether the history records should be deleted if an intake request is deleted—and why

4. Plan the write behavior

   Decide exactly when history gets created.

   Think through:
   - What happens if the request does not exist?
   - What happens if the new status equals the current status?
   - At what point do you capture the old status?
   - Should updating the request and inserting its history be one database save operation?

5. Design the read endpoint

   Define the endpoint before implementing it.

   Decide:
   - HTTP method and route
   - Expected response shape
   - Sort order—usually newest first
   - What happens when the request does not exist
   - What happens when the request belongs to another clinic

6. Identify every code layer that must change

   Make a checklist under these headings:
   - Models
   - Data / `DbContext`
   - Migrations
   - DTOs
   - Repository interface and implementation
   - Service interface and implementation
   - Controller
   - Tests
   - Swagger/docs

7. Implement in small vertical slices

   Recommended order:
   1. Model + DbContext + migration
   2. Repository support
   3. Service logic that records history during status update
   4. Read endpoint
   5. Tests
   6. Documentation

8. Verify like it is trying to embarrass you in production
   - Run unit and integration tests.
   - Test a status change locally.
   - Retrieve the history and verify old/new values, user, and time.
   - Test unauthorized and wrong-clinic access.
   - Confirm existing request endpoints still behave correctly.

Start with step 1: trace the current status-update path and tell me the files/methods you find.

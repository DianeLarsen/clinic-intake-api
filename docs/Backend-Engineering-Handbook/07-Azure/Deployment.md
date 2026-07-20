# Azure Deployment

## Purpose

Deploy the Clinic Intake API to Azure App Service with Azure SQL Database while keeping local development and automated tests on SQLite.

## Production Resources

- Resource group: `rg-clinic-intake-api-prod-westus`
- Region: `West US`
- App Service plan: `asp-clinic-intake-api-prod-westus`
- Web App: `clinic-intake-api-dlarsen-2026`
- Azure SQL server: `clinicintake-dlarsen-2026`
- Database: `clinicintake`

## Database Strategy

- Local development and tests use SQLite.
- Azure Production uses Azure SQL Database.
- The API selects the provider through `Database:Provider`.
- SQLite uses `EnsureCreatedAsync()` because local/test data is disposable.
- Azure SQL uses `MigrateAsync()` and the `InitialSqlServer` EF Core migration.

## Production Configuration

Azure App Service stores production configuration as environment variables.

Required settings include:

- `ASPNETCORE_ENVIRONMENT=Production`
- `Database__Provider=SqlServer`
- `ConnectionStrings__DefaultConnection`
- Production JWT issuer, audience, and signing-key settings

Connection strings and JWT signing keys are stored in Azure App Settings and are never committed to Git.

## Network Security

- Azure SQL uses a public endpoint protected by firewall rules.
- The current local public IP is allowed for administration.
- The Web App's outbound IPv4 addresses are explicitly allowed.
- The broad "Allow Azure services and resources" firewall option remains disabled.
- TLS 1.2 is required for Azure SQL connections.

## Deployment

The first deployment was performed manually:

1. Run the test suite.
2. Publish the API project in Release mode.
3. Create a ZIP from the published files.
4. Upload the ZIP to App Service through the Kudu ZIP deployment endpoint.
5. Disable SCM publishing credentials again after deployment.

A GitHub Actions deployment workflow is the next planned improvement.

## Validation

Production deployment was verified on July 20, 2026:

- `GET /health/live` returned `200 Healthy`
- `GET /health/ready` returned `200 Healthy` with a healthy Azure SQL database check
- Swagger returns `404` in Production because it is Development-only
- `GET /api/v1/requests` without a token returns `401 Unauthorized`

## SQL Server Cascade-Delete Fix

SQL Server rejected the original schema because deleting a clinic created multiple cascade paths to `IntakeRequests`.

The database model now prevents deleting a clinic while it has related patients or intake requests. The `IntakeRequest → Patient` relationship remains cascade delete.

All 47 unit and integration tests pass after the change.

## Cost Notes

- Azure SQL Database uses the Azure SQL free serverless offer with overage billing disabled.
- The Linux App Service Basic B1 plan is approximately $13.14 per month.
- Delete the resource group when the deployment is no longer needed to stop billable resources.
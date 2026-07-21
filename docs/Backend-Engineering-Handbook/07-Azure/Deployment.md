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

## Continuous Deployment with GitHub Actions

The API deploys automatically whenever a change is merged into the `main` branch.

The deployment workflow:

1. Builds the .NET solution in Release mode.
2. Publishes only `ClinicIntakeApi.csproj`.
3. Uploads the published API as a GitHub Actions artifact.
4. Authenticates to Azure using OpenID Connect (OIDC).
5. Cleans stale files from App Service before deploying the new package.
6. Deploys the artifact to the Production App Service slot.

OIDC uses short-lived tokens instead of storing an Azure publish profile or password in GitHub. The Azure identity has the least-privilege `Website Contributor` role scoped to this App Service.

### Deployment Reliability Notes

- The deployment workflow uses `clean: true` so files from an older deployment do not remain in App Service.
- The API uses `EnableRetryOnFailure()` with SQL Server. This handles temporary Azure SQL availability events, including a serverless database resuming after auto-pause.
- Azure SQL serverless may return a temporary `40613` error on the first connection after it resumes. EF Core retries the connection automatically.

### Production Verification

After deployment, verify:

```bash
curl -i https://clinic-intake-api-dlarsen-2026-dhdmdmesgkgygpbz.westus-01.azurewebsites.net/health/live

curl -i https://clinic-intake-api-dlarsen-2026-dhdmdmesgkgygpbz.westus-01.azurewebsites.net/health/ready
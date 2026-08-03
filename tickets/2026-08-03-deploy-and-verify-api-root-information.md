# Ticket: Deploy and Verify API Root Information

**Type:** Deployment / Verification  
**Priority:** Low  
**Status:** Complete  
**Completed:** August 3, 2026

## User story

As a visitor, I can open the deployed API URL and see clear information about the API, public endpoints, protected endpoints, and Swagger documentation.

## Acceptance criteria

- [x] The root endpoint is deployed to Azure.
- [x] Visiting the Azure site URL returns API-information JSON.
- [x] `/swagger` loads.
- [x] `/health/live` and `/health/ready` return healthy responses.
- [x] A protected `/api/v1/requests` call without a token returns `401 Unauthorized`.
- [x] The root response identifies public routes and explains that request routes require a JWT Bearer token.

## Delivered behavior

The public root route, `GET /`, returns a short JSON guide with:

- API name and version
- Swagger documentation route
- Liveness and readiness health routes
- Public endpoint list
- Notice that all `/api/v1/requests` routes require a valid JWT Bearer token

## Verification performed

The deployed Azure App Service was verified after the GitHub pull request merged:

- Root API information endpoint responded successfully.
- Liveness and readiness checks responded successfully.
- Swagger was available.
- Request routes rejected an unauthenticated request with `401 Unauthorized`.

## Notes

The initial `502 Bad Gateway` cleared once the Azure deployment finished starting. No code change was required.


# Movies Test Resolve Feedback RequestId By Token Design

Date: 2026-04-24

## Summary

Add a test-only Movies admin API that resolves a feedback `requestId` from a public feedback link token.

This endpoint is intended purely to support manual testing of the feedback lifecycle APIs that currently require `requestId`, while the public feedback link itself only contains a token.

## Goals

- Let testers translate a feedback token into the matching `MovieFeedbackRequest.Id`.
- Reuse the same token-hash lookup model already used by the public feedback flow.
- Keep the endpoint restricted to development/test environments and admin users.
- Return only the minimal field needed: `requestId`.

## Non-Goals

- Returning the full feedback request payload.
- Exposing the raw token hash.
- Making this endpoint public or production-available.
- Changing the public feedback route format.

## Current State

- Public feedback links use `/movies/feedback/{token}`.
- `MovieFeedbackService` hashes the token and resolves the request through `TokenHash`.
- Test-only admin endpoints already exist under:
  - `/api/movies/admin/test/...`
- Existing feedback test utilities such as `expire-opened` currently require `requestId`.

## Recommended Approach

Add a test-only POST endpoint that accepts the token in the request body, hashes it server-side, and returns the corresponding `requestId`.

This is the best fit because:

- it avoids putting tokens in the URL
- it follows the existing token-hash lookup pattern
- it exposes only the minimal testing data needed
- it fits naturally into the current test-admin controller

## Detailed Design

### 1. Endpoint Shape

Add a new endpoint to `MoviesAdminTestController`:

- `POST /api/movies/admin/test/feedback-links/resolve-request`

Request body:

```json
{
  "token": "..."
}
```

Response body:

```json
{
  "requestId": "..."
}
```

### 2. Environment Restriction

The endpoint must remain unavailable in production.

Enforcement:

1. Keep standard admin authorization:
   - `[Authorize(Roles = "MoviesAdmin,Admin")]`
2. Keep the existing runtime environment guard used by test endpoints:
   - allow only `Development` and `Test`
   - otherwise return `NotFound()`

### 3. Service and Repository Flow

Add an admin test lookup method:

- `ResolveFeedbackRequestIdByTokenAsync(string token, CancellationToken cancellationToken = default)`

Behavior:

1. Validate the token is not blank.
2. Trim the token.
3. Hash the token with the same SHA256 logic used by public feedback token resolution.
4. Query `MovieFeedbackRequest` by `TokenHash`.
5. If not found, return `null`.
6. If found, return a DTO containing only `requestId`.

### 4. DTO Contract

Recommended request DTO:

```csharp
public sealed class MoviesAdminResolveFeedbackRequestByTokenRequestDto
{
    public string Token { get; set; } = string.Empty;
}
```

Recommended response DTO:

```csharp
public sealed class MoviesAdminResolveFeedbackRequestByTokenResponseDto
{
    public Guid RequestId { get; set; }
}
```

### 5. Error Handling

- blank token:
  - `400 Bad Request`
- unknown token:
  - `404 Not Found`
- disallowed environment:
  - `404 Not Found`

### 6. Testing Strategy

Backend tests should cover:

- blank token returns bad request at controller level
- production environment blocks the endpoint
- valid token resolves the expected `requestId`
- unknown token returns not found
- the lookup uses the same token-hash logic as the public feedback flow

## Risks

- Even in test-only scope, this endpoint makes it easier to bridge public tokens to internal IDs, so it must stay environment-guarded.
- If token hashing diverges from the public feedback flow, testers could get inconsistent results.

## Open Decisions Resolved

- Return only `requestId`
- Use POST with token in request body
- Keep the endpoint test-only and admin-only

## Implementation Outline

1. Add request/response DTOs for token resolution.
2. Add a service/repository lookup that hashes the token and finds the request.
3. Add the new endpoint to `MoviesAdminTestController`.
4. Add tests for blank token, not found, environment blocking, and successful resolution.

# Movies Test Expire Feedback Opened Design

Date: 2026-04-24

## Summary

Add a test-only Movies admin API that makes a feedback request behave like it was opened more than 7 days ago without any submission.

This endpoint exists only for development and test environments. It must not mark the feedback request expired directly. Instead, it mutates the request lifecycle fields so the existing public feedback service applies the normal expiration rule when the feedback link is accessed.

## Goals

- Provide a fast way to simulate the "opened but not submitted for 7 days" expiration case.
- Reuse the real feedback expiration logic already implemented in `MovieFeedbackService`.
- Keep the endpoint unavailable in production.
- Preserve existing admin authorization requirements.

## Non-Goals

- Directly setting the feedback request status to `Expired`.
- Bypassing the feedback token validation flow.
- Exposing the endpoint outside development or test environments.
- Creating a generic time-travel API for feedback lifecycle testing.

## Current State

- `MovieFeedbackService` expires a feedback request when `FirstOpenedAtUtc + 7 days <= utcNow` and there has been no submission.
- Feedback requests already track:
  - `FirstOpenedAtUtc`
  - `LastOpenedAtUtc`
  - `SubmittedCount`
  - `ExpiredReason`
  - `Status`
- A test-only Movies admin controller already exists for forcing showtimes to finish early.

## Recommended Approach

Add a test-only admin endpoint that rewrites the feedback request to look like it was first opened 8 days ago and never submitted.

This is the best fit because:

- it exercises the same expiration logic used in production
- it avoids duplicating or bypassing feedback state transitions
- it keeps the testing API narrow and predictable
- it matches the exact business rule the user wants to validate

## Detailed Design

### 1. Endpoint Shape

Add a new endpoint to `MoviesAdminTestController`:

- `POST /api/movies/admin/test/feedback-requests/{requestId}/expire-opened`

Request body:

- none

Response body:

- `requestId`
- `previousFirstOpenedAtUtc`
- `previousLastOpenedAtUtc`
- `newFirstOpenedAtUtc`
- `newLastOpenedAtUtc`
- `message`

### 2. Environment Restriction

The endpoint must not be available in production.

Enforcement:

1. Keep standard admin authorization:
   - `[Authorize(Roles = "MoviesAdmin,Admin")]`
2. Keep runtime environment guard in the test controller:
   - allow only `Development` and `Test`
   - otherwise return `NotFound()`

### 3. Service and Repository Flow

Add a focused admin test method:

- `ForceExpireOpenedFeedbackRequestAsync(Guid requestId, CancellationToken cancellationToken = default)`

Behavior:

1. Load the target `MovieFeedbackRequest`.
2. If not found, return `null`.
3. Capture the previous opened timestamps.
4. Set:
   - `FirstOpenedAtUtc = DateTime.UtcNow.AddDays(-8)`
   - `LastOpenedAtUtc = DateTime.UtcNow.AddDays(-8)`
   - `SubmittedCount = 0`
   - `UpdatedAtUtc = DateTime.UtcNow`
5. Ensure the request remains in a state the public feedback flow will evaluate:
   - keep `Status = Sent`
   - clear `ExpiredReason`
6. Save changes.
7. Return a DTO describing the mutation.

This method should not:

- set `Status = Expired`
- set `InvalidatedAtUtc`
- create or send email
- submit feedback

### 4. Data Mutation Rule

Exact mutation rule:

- move both opened timestamps to 8 days in the past
- keep the request as a sent, usable link until the normal service evaluates it

Reasoning:

- 8 days avoids edge timing around the 7-day boundary
- keeping status as `Sent` ensures the public feedback endpoint performs the real expiration transition

### 5. Triggering the Expiration

After the endpoint runs:

- the operator opens the feedback link or calls the public feedback endpoint
- `MovieFeedbackService` sees `FirstOpenedAtUtc + 7 days <= utcNow`
- the service marks the request expired with the standard expiration rule

No separate expire action is needed in the test endpoint.

### 6. DTO Contract

Recommended response DTO:

```csharp
public sealed class MoviesAdminForceExpireOpenedFeedbackRequestResponseDto
{
    public Guid RequestId { get; set; }
    public DateTime? PreviousFirstOpenedAtUtc { get; set; }
    public DateTime? PreviousLastOpenedAtUtc { get; set; }
    public DateTime NewFirstOpenedAtUtc { get; set; }
    public DateTime NewLastOpenedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

Recommended message:

- `"Feedback request opened timestamps moved to the past for expiry testing."`

### 7. Testing Strategy

Backend tests should cover:

- endpoint/service returns not found for unknown feedback request
- valid feedback request gets both opened timestamps moved beyond 7 days
- `UpdatedAtUtc` is refreshed
- status stays usable for normal evaluation
- production environment blocks the endpoint
- after forcing opened expiry, the public feedback service marks the request expired when the token is read

## Risks

- Because the endpoint edits real feedback lifecycle data, it must remain unavailable in production.
- If testers run it on shared non-production data, they may invalidate feedback links they still wanted to use.
- If the request has never been sent or has already been fully invalidated, the test result may not reflect the normal public-link flow.

## Open Decisions Resolved

- Mechanism: mutate lifecycle timestamps, do not force status to expired
- Scope: development/test only
- Security: admin-only plus environment guard
- Trigger method: let the public feedback service perform the actual expiration

## Implementation Outline

1. Add a response DTO for forcing a feedback request into the opened-expired state.
2. Add service/repository support to move `FirstOpenedAtUtc` and `LastOpenedAtUtc` into the past.
3. Add a test-only endpoint to `MoviesAdminTestController`.
4. Add tests for not-found, successful mutation, and environment blocking.
5. Add a regression test that confirms the public feedback flow expires the request after the forced timestamp change.

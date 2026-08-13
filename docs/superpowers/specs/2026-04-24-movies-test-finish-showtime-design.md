# Movies Test Finish Showtime Design

Date: 2026-04-24

## Summary

Add a test-only Movies admin API that forces a showtime to end early by updating `Showtime.EndAtUtc` to a timestamp in the past.

This API exists only to exercise the real feedback-email flow already implemented in the Movies module. It must not send feedback emails directly. Instead, it mutates showtime data so the existing background service sees the showtime as finished and sends feedback invitations through the normal production path.

## Goals

- Provide a fast way to make one showtime look finished in test or development environments.
- Trigger feedback email delivery through the real post-showtime background workflow.
- Keep the endpoint unavailable in production.
- Preserve existing authorization requirements for Movies admin actions.

## Non-Goals

- Creating a generic time-travel framework for Movies.
- Manually sending feedback emails from the endpoint.
- Updating `MovieFeedbackRequest` records directly in the endpoint.
- Exposing the endpoint to non-admin users or to production.

## Current State

- `MovieFeedbackInvitationBackgroundService` sends feedback emails only after the related showtime has ended.
- `PaymentRepository` creates `MovieFeedbackRequest` records with `Status = Pending` and `AvailableAtUtc` derived from showtime end time.
- Admin showtime APIs already live under `MoviesAdminController`.
- The Movies catalog data for `Showtime` is stored in the catalog context and accessed through the Movies admin service/repository path.

## Recommended Approach

Add a test-only admin endpoint that updates `Showtime.EndAtUtc` to `UtcNow.AddMinutes(-1)` and returns the old/new values.

This is the best fit because:

- it exercises the same background send logic used in production
- it avoids duplicating email-sending behavior in a testing endpoint
- it keeps the test API narrow and easy to reason about
- it only mutates the one data point the background service already trusts

## Detailed Design

### 1. Endpoint Shape

Add a new controller dedicated to test-only admin actions:

- `MoviesAdminTestController`

Recommended route:

- `POST /api/movies/admin/test/showtimes/{showtimeId}/finish-now`

Request body:

- none

Response body:

- `showtimeId`
- `previousEndAtUtc`
- `newEndAtUtc`
- `message`

### 2. Environment Restriction

The endpoint must not be available in production.

Recommended enforcement:

1. Keep standard admin authorization:
   - `[Authorize(Roles = "MoviesAdmin,Admin")]`
2. Add runtime environment guard:
   - allow only `Development` and `Test`
   - otherwise return `NotFound()` or `Forbid()`

Returning `NotFound()` is preferable because it reduces visibility of test-only functionality outside approved environments.

### 3. Service and Repository Flow

Add a focused admin test service method:

- `ForceFinishShowtimeAsync(Guid showtimeId, CancellationToken cancellationToken = default)`

Behavior:

1. Load the target showtime from the catalog store.
2. If not found, return `null`.
3. Capture `previousEndAtUtc`.
4. Set `EndAtUtc = DateTime.UtcNow.AddMinutes(-1)`.
5. Update `UpdatedAtUtc = DateTime.UtcNow`.
6. Save changes.
7. Return a DTO containing old/new end times.

This method should not:

- update `MovieFeedbackRequest`
- call the feedback background service directly
- enqueue or send email itself

### 4. Data Mutation Rule

The endpoint intentionally edits the real showtime row.

Exact mutation rule:

- overwrite `EndAtUtc` to one minute before `utc now`

Reasoning:

- one minute in the past avoids edge timing around exact `utc now`
- overwriting an existing future `EndAtUtc` is acceptable because this endpoint is only for non-production environments

### 5. Triggering Feedback Mail

After the endpoint runs:

- `MovieFeedbackInvitationBackgroundService` will find pending feedback requests whose associated showtime has now ended
- the service will send the email using the existing invitation workflow

No additional trigger is needed in the endpoint.

The operator can either:

- wait for the next poll interval
- or manually run the background logic in a test harness if desired

### 6. DTO Contract

Recommended response DTO:

```csharp
public sealed class ForceFinishShowtimeResponseDto
{
    public Guid ShowtimeId { get; set; }
    public DateTime? PreviousEndAtUtc { get; set; }
    public DateTime NewEndAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

Recommended message:

- `"Showtime end time moved to the past for feedback-email testing."`

### 7. Testing Strategy

Backend tests should cover:

- endpoint/service returns not found for unknown showtime
- valid showtime gets `EndAtUtc` moved into the past
- `UpdatedAtUtc` is refreshed
- production environment blocks the endpoint
- after forced finish, the feedback invitation background service treats the showtime as ended and can send feedback mail

## Risks

- Because the endpoint edits real showtime data, it must stay unavailable in production.
- If testers use it on actively-used non-production data, downstream reports may see altered end times.
- If the environment guard is weak, the endpoint could accidentally leak into a deployed environment.

## Open Decisions Resolved

- Mechanism: mutate showtime data, do not bypass the email workflow
- Scope: test/development only
- Security: admin-only plus environment guard
- Trigger method: move `EndAtUtc` into the past

## Implementation Outline

1. Add a test-only admin controller for Movies testing utilities.
2. Add a DTO and admin test service method for forcing a showtime to finish.
3. Implement catalog persistence to update `EndAtUtc` and `UpdatedAtUtc`.
4. Gate the controller by environment and admin role.
5. Add tests for not-found, successful mutation, and environment blocking.

# Movies Test Finish Showtime Unlock Feedback Design

Date: 2026-04-24

## Summary

Adjust the existing test-only `finish-now` Movies admin API so it not only moves a showtime's `EndAtUtc` into the past, but also updates pending feedback requests for that showtime to become immediately eligible for the feedback invitation background service.

This change is needed because the current background service selects pending feedback requests by `AvailableAtUtc <= utcNow`, while the existing `finish-now` endpoint only mutates `Showtime.EndAtUtc`. As a result, testers can finish a showtime early but still wait until the original `AvailableAtUtc` before any email is sent.

## Goals

- Make the existing `finish-now` test endpoint trigger feedback emails within the next background-service poll cycle.
- Keep the email send path unchanged and production-like.
- Update only pending feedback requests for the targeted showtime.
- Preserve the test-only scope and admin-only access of the endpoint.

## Non-Goals

- Sending feedback emails directly from the endpoint.
- Changing the production background-service selection rules.
- Rewriting already sent, expired, or submitted feedback requests.
- Introducing a second endpoint for the same testing goal.

## Current State

- `MoviesAdminTestController` exposes:
  - `POST /api/movies/admin/test/showtimes/{showtimeId}/finish-now`
- `ForceFinishShowtimeAsync` currently updates only:
  - `Showtime.EndAtUtc`
  - `Showtime.UpdatedAtUtc`
- `PaymentRepository.CreateFeedbackRequestIfMissingAsync` creates feedback requests with:
  - `Status = Pending`
  - `AvailableAtUtc = showtime.EndAtUtc ?? showtime.StartAtUtc`
- `MovieFeedbackInvitationBackgroundService` only loads requests where:
  - `Status == Pending`
  - `AvailableAtUtc <= utcNow`

This means a forced-finished showtime can still have feedback requests blocked by the original `AvailableAtUtc`.

## Recommended Approach

Extend `ForceFinishShowtimeAsync` so it also updates `AvailableAtUtc` for pending feedback requests of the same showtime to the same forced end time.

This is the best fit because:

- it preserves the existing test endpoint users already call
- it keeps the background email workflow unchanged
- it aligns the request-availability data with the mutated showtime data
- it avoids extra test-only endpoints or production logic changes

## Detailed Design

### 1. Endpoint Shape

Keep the existing endpoint:

- `POST /api/movies/admin/test/showtimes/{showtimeId}/finish-now`

No route change is needed.

Response body can stay unchanged if desired, because the visible behavior change is internal. If helpful, a future enhancement could add a count of updated feedback requests, but that is not required for this scope.

### 2. Service and Repository Flow

Keep using:

- `ForceFinishShowtimeAsync(Guid showtimeId, CancellationToken cancellationToken = default)`

Updated behavior:

1. Load the target showtime from the catalog store.
2. If not found, return `null`.
3. Compute `forcedEndAtUtc = DateTime.UtcNow.AddMinutes(-1)`.
4. Update:
   - `Showtime.EndAtUtc = forcedEndAtUtc`
   - `Showtime.UpdatedAtUtc = utcNow`
5. Find `MovieFeedbackRequest` rows where:
   - `ShowtimeId == showtimeId`
   - `Status == Pending`
6. For each matching request:
   - set `AvailableAtUtc = forcedEndAtUtc`
   - set `UpdatedAtUtc = utcNow`
7. Save changes.
8. Return the same finish-showtime response DTO.

### 3. Data Mutation Rule

Only pending feedback requests should be updated.

Do not modify feedback requests that are:

- `Sent`
- `Expired`
- `Submitted`
- `Cancelled`

Reasoning:

- `Pending` requests are still waiting for invitation timing and should follow the forced-finished showtime
- non-pending requests have already progressed through the lifecycle and should not be rewritten

### 4. Email Trigger Result

After the endpoint runs:

- the showtime looks finished
- pending feedback requests for that showtime become immediately available
- the next `MovieFeedbackInvitationBackgroundService` poll will load those requests and send invitation emails through the normal path

Given the current code, that poll interval is 1 minute.

### 5. Testing Strategy

Backend tests should cover:

- `ForceFinishShowtimeAsync` still returns `null` when showtime does not exist
- `ForceFinishShowtimeAsync` still moves `EndAtUtc` into the past
- pending feedback requests for the same showtime get `AvailableAtUtc` moved into the past
- pending feedback requests for other showtimes are not changed
- non-pending feedback requests for the same showtime are not changed
- the invitation background service sends email after the forced-finish operation because the request becomes selectable

## Risks

- This slightly broadens the side effects of `finish-now`, so the endpoint documentation should make that explicit.
- If testers use it on shared non-production data, it may accelerate invitation sending for multiple pending requests on that showtime.
- If there are future lifecycle rules tied to `AvailableAtUtc`, this endpoint will intentionally bypass the original schedule for testing purposes.

## Open Decisions Resolved

- Keep one endpoint, do not add a second unlock endpoint
- Mutate `AvailableAtUtc` for pending feedback requests
- Keep background service logic unchanged
- Preserve test-only scope and admin-only authorization

## Implementation Outline

1. Update the repository/service logic behind `ForceFinishShowtimeAsync`.
2. Add tests proving pending feedback requests are unlocked by the endpoint.
3. Add a regression test showing the invitation background service can send after the forced finish.

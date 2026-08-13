# Movie Feedback Link Lifecycle Design

Date: 2026-04-24

## Summary

Constrain movie feedback submission to a dedicated public page reached only through an email link, and move feedback invitation delivery to after the booked showtime has ended.

Each feedback link must:

- render the feedback form only on the dedicated route `/movies/feedback/:token`
- be emailed only after the related movie showtime has finished
- allow at most 3 successful feedback submissions
- expire immediately after the third submission
- expire automatically 7 days after the first link open if the user has opened the link but has not submitted feedback

The current codebase already has a dedicated public feedback page, token-based request validation, and a 3-submission cap. The main work is tightening the request lifecycle so email sending is deferred until showtime end and link expiry depends on first interaction.

## Goals

- Keep the feedback form off normal movie pages and available only via the special token route.
- Send feedback emails only after the linked showtime has ended.
- Preserve the existing maximum of 3 submissions per feedback link.
- Invalidate the link immediately after the third successful submission.
- Start a 7 day inactivity countdown from the first link open when no feedback has been submitted.
- Expose clear status and expiry messaging to the dedicated feedback page.

## Non-Goals

- Building an authenticated customer feedback dashboard.
- Allowing feedback submission directly from booking history or movie detail pages.
- Introducing per-submission email reminders or resend flows.
- Changing public aggregation and display of approved feedback entries beyond what is needed for lifecycle messaging.
- Generalizing this flow into a mall-wide survey framework.

## Current State

### Frontend

- `MoviePublicFeedbackPage.tsx` already hosts a standalone public form on `/movies/feedback/:token`.
- The page loads request context through `fetchPublicMovieFeedbackRequest(token)` and submits through `submitMovieFeedbackByToken(token, payload)`.
- The page already avoids rendering the form when `canSubmit` is false.

### Backend

- `MovieFeedbackService` validates token links and blocks submission once the request reaches 3 feedbacks.
- `MovieFeedbackRepository` sets the request to `Submitted` and invalidates it on the third successful submission.
- `MovieFeedbackRequest` already stores lifecycle fields such as `AvailableAtUtc`, `SentAtUtc`, `ExpiresAtUtc`, `SubmittedAtUtc`, and `InvalidatedAtUtc`.
- The Movies module already runs background services for hold cleanup and ticket email outbox processing, so there is an established place for scheduled workflow logic.

### Gap Against Requested Behavior

- The lifecycle does not currently record first link interaction.
- The 7 day expiry rule after first open is not implemented.
- The email send trigger is not yet explicitly tied to showtime completion in the feedback request lifecycle.
- User-facing messaging still treats all terminal states almost the same.

## Recommended Approach

Keep the existing tokenized public page and extend `MovieFeedbackRequest` into an explicit lifecycle record.

This is the best fit for the current codebase because:

- the dedicated page already exists and should remain the only render surface for the form
- the domain already models feedback invitations separately from submitted feedback entries
- the infrastructure already supports background services and SMTP-based email delivery
- the existing 3-submission rule can be preserved with a small domain extension instead of a rewrite

The core design choice is to add first-open tracking and treat expiry as a combination of:

- immediate invalidation after the third submission
- inactivity expiry 7 days after first open when no submission has occurred
- standard invalidation when the request is cancelled or manually expired by backend processing

## Detailed Design

### 1. Feedback Form Visibility

The feedback form remains available only on the existing dedicated route:

- `FRONTEND/src/features/movies/routes/MovieRoutes.tsx`
- `path="feedback/:token"`

Normal user-facing pages such as:

- `MovieHomePage`
- `MovieDetailPage`
- checkout and payment pages

must not render an embedded feedback form or expose a direct feedback submission action.

They may contain informational copy such as "feedback will be sent by email after the show ends", but the form itself stays exclusive to the token route.

### 2. Feedback Request Lifecycle

`MovieFeedbackRequest` should become the single source of truth for invitation state.

Add lifecycle fields:

- `FirstOpenedAtUtc` as nullable `DateTime`
- `LastOpenedAtUtc` as nullable `DateTime`
- `ExpiredReason` as nullable string or enum-backed code

Lifecycle states stay simple:

- `Pending`: request exists but email has not been sent yet
- `Sent`: email was sent and the token may still be used
- `Submitted`: the link reached the third successful submission and is immediately invalidated
- `Expired`: the link can no longer be used because of inactivity or other expiry logic
- `Cancelled`: the request was intentionally disabled

Rules:

1. Create the request in `Pending`.
2. When the related showtime has ended and the email is successfully sent, transition to `Sent`.
3. On the first successful token open, set `FirstOpenedAtUtc` and `LastOpenedAtUtc`.
4. On later opens, update `LastOpenedAtUtc`.
5. If 3 submissions are recorded, transition to `Submitted`, set `SubmittedAtUtc`, and set `InvalidatedAtUtc`.
6. If `FirstOpenedAtUtc + 7 days <= now` and there are still zero submissions, transition to `Expired`, set `InvalidatedAtUtc`, and record `ExpiredReason = "OpenedNoSubmission7Days"`.

### 3. Email Delivery Timing

Feedback email sending moves to a post-showtime workflow.

Recommended behavior:

- create or retain one `MovieFeedbackRequest` per `BookingId + ShowtimeId`
- do not send the feedback email when booking is created
- a background service periodically scans `Pending` feedback requests
- for each request, load the related showtime and determine whether the show has ended
- only after `showtime end time <= utc now` should the service generate a token, hash it, set send metadata, and dispatch the email

This background service should reuse the existing Movies infrastructure pattern:

- poll on a small interval
- batch a limited number of pending requests
- update retry counts and last error on failures

The email body should contain:

- movie title
- showtime summary
- a single feedback link to `/movies/feedback/:token`
- short text stating the link allows up to 3 submissions
- short text stating that if the link is opened but no feedback is sent, it expires 7 days after the first open

### 4. First-Open Expiry Rule

The 7 day countdown begins only after the first successful open of the feedback link.

Interpretation of "no interaction":

- the user has opened the link at least once
- the user has not submitted any feedback entry

This means:

- unopened links do not expire under the 7 day inactivity rule
- opened links with at least one submission also do not use this specific expiry rule
- only opened-and-never-submitted links expire after 7 days from `FirstOpenedAtUtc`

Implementation behavior for `GetPublicRequestAsync(token)`:

1. Validate the token and load the request.
2. If the request is already terminal, return terminal status without mutating.
3. If `FirstOpenedAtUtc` is null, set both `FirstOpenedAtUtc` and `LastOpenedAtUtc` to `utc now`.
4. Otherwise set `LastOpenedAtUtc` to `utc now`.
5. Re-check whether the request should immediately transition to `Expired` because the 7 day window has passed.

The expiry check should also run in submission flow to avoid race conditions.

### 5. Submission Limit Rule

The existing submission limit of 3 remains correct and should be made explicit in service and UI.

Behavior:

- submission 1 and 2 succeed if the request is otherwise valid
- submission 3 succeeds and then immediately invalidates the link
- submission 4 must fail because the request is already terminal

This should continue to be enforced both:

- in the application service before repository write
- inside the repository transaction to prevent concurrent over-submission

### 6. Public Page Behavior

`MoviePublicFeedbackPage.tsx` remains the only form UI but should surface lifecycle details more clearly.

Required behaviors:

- if the token is invalid, render only an error state
- if the link is valid but not yet available, render a waiting state without the form
- if the link has expired due to 7 day inactivity, render an expired state without the form
- if the link has reached 3 submissions, render a submitted/closed state without the form
- if the link is valid and submittable, render the form and remaining context

The page should display:

- movie title
- link availability window or status
- remaining submission count if available from API
- terminal-state message that distinguishes:
  - invalid link
  - showtime not finished yet
  - link expired after 7 days without feedback
  - submission limit reached

### 7. API Contract Changes

`PublicMovieFeedbackRequestResponseDto` should expose enough information for the dedicated page to render correct status without guessing from generic strings.

Recommended additions:

- `RemainingSubmissions`
- `FirstOpenedAtUtc`
- `ExpiredReason`
- `TerminalMessage` or a more structured status reason code

The frontend should stop collapsing most terminal errors into the same "already submitted" message because that hides real state transitions.

### 8. Background Processing and Consistency

Two consistency points matter:

1. Send timing
2. Expiry timing

Recommended split:

- one background service handles pending feedback email dispatch after showtime completion
- the public request service performs on-read expiry checks for interaction-based expiry

This avoids relying only on scheduled cleanup for correctness. Even if the cleanup job has not run yet, the next page load or submit attempt still sees the request as expired.

An optional cleanup pass may still batch-mark aged requests as `Expired` for reporting and operational clarity.

## Error Handling

### Invalid or tampered token

- Return a dedicated invalid-link response or error.
- Do not expose whether a booking exists behind the token.

### Showtime not ended yet

- Do not allow email send.
- If a token somehow exists before send, the public request should return a not-ready state and no form.

### Email send failure

- Keep the request in `Pending`.
- Increment retry metadata and keep the background service retryable.
- Do not generate multiple active tokens for the same request.

### Concurrent submissions near the third attempt

- The repository transaction remains responsible for the final count check.
- Only one request may create the third successful submission and invalidate the link.

### Expiry while user is on the page

- Submission must re-check validity at submit time.
- If the 7 day threshold passes while the page is open, the form submit returns an expired response and the page switches to a closed state.

## Testing Strategy

### Backend tests

- `GetPublicRequestAsync()` sets `FirstOpenedAtUtc` and `LastOpenedAtUtc` on first open.
- Reopening the link updates `LastOpenedAtUtc` without resetting `FirstOpenedAtUtc`.
- A request opened more than 7 days ago with zero submissions transitions to `Expired`.
- A request with at least one submission does not expire under the opened-no-submission rule.
- The third successful submission invalidates the link immediately.
- A fourth submission attempt fails.
- Pending requests are emailed only after showtime end.
- Email failures update retry metadata without duplicating tokens.

### Frontend tests

- Only `/movies/feedback/:token` renders the feedback form.
- Terminal states do not render the form.
- Expired-after-open messaging is distinct from submission-limit messaging.
- The page consumes structured status from the API instead of inferring from generic error text.

### Integration checks

- Booking completion creates a pending feedback request.
- After showtime end, the background service sends exactly one email.
- The emailed token opens the dedicated page and starts the 7 day first-open window.
- After 3 submissions, the same token no longer allows submission.

## Risks

- If showtime end time is derived inconsistently, feedback emails may send too early or too late.
- If first-open updates are not atomic enough, multiple simultaneous opens could create noisy audit timestamps.
- If the frontend keeps normalizing all backend messages to one sentence, users will not understand why the link closed.

## Open Decisions Resolved

- Feedback form location: dedicated token route only.
- Email timing: after showtime has ended.
- Submission limit: 3 successful submissions per link.
- Post-limit behavior: invalidate immediately after the third submission.
- Inactivity expiry: 7 days from the first link open, only when there has been no submission.

## Implementation Outline

1. Extend `MovieFeedbackRequest` persistence with first-open and expiry-reason fields.
2. Update feedback service and repository logic to track first open and enforce 7 day opened-no-submission expiry.
3. Add or extend a background service that sends feedback emails only after showtime completion.
4. Expose structured lifecycle metadata in the public feedback request API.
5. Update the dedicated frontend page to render distinct availability and expiry states.
6. Add backend and frontend tests for send timing, first-open tracking, 3-submission invalidation, and 7 day expiry.

# Movie Feedback First-Open Hard Expiry Design

## Goal

Change the feedback link lifecycle so that the link always expires 7 days after the first open, regardless of whether the recipient has submitted 0, 1, or 2 feedback entries.

## Current Context

- The current lifecycle already closes the link immediately after the 3rd submission.
- The current 7-day expiry rule only applies when the link was opened but no feedback was submitted.
- This behavior is implemented in [MovieFeedbackService.cs](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/MovieFeedbackService.cs), where the expiry check still depends on `Feedbacks.Count == 0`.
- The desired business rule is stricter:
  - once the link is opened the first time, the link has a 7-day lifetime
  - that lifetime does not reset after submissions
  - if the user reaches 3 submissions earlier, the submission limit still closes the link immediately

## Decision

Use a hard first-open expiry:

- The expiry window starts at `FirstOpenedAtUtc`
- The link becomes expired when `FirstOpenedAtUtc + 7 days <= now`
- This applies even if the request already has 1 or 2 submitted feedback entries
- Submission limit remains unchanged:
  - 3rd submission closes the link immediately

## Approach Options

### Option 1: Recommended

Update the existing service rule to expire any opened `Sent` request after 7 days from `FirstOpenedAtUtc`, without checking feedback count.

Why this is recommended:

- smallest change
- aligns directly with the new business rule
- preserves the rest of the lifecycle implementation
- keeps submission-limit behavior intact

### Option 2

Write `ExpiresAtUtc = FirstOpenedAtUtc + 7 days` when the link is first opened and drive expiry from `ExpiresAtUtc`.

Why this is weaker for now:

- touches more lifecycle semantics
- risks colliding with the existing meaning of `ExpiresAtUtc`
- not necessary to satisfy the new rule

### Option 3

Reset the 7-day window after every submission or every later open.

Why this is rejected:

- directly conflicts with the requested rule
- makes link lifetime dependent on repeated interactions instead of first open

## Domain Rules

### First-Open Expiry

The link expires when all of the following are true:

- `Status == Sent`
- `FirstOpenedAtUtc` has a value
- `InvalidatedAtUtc` is null
- `FirstOpenedAtUtc + 7 days <= now`

Feedback count is no longer part of this expiry rule.

### Submission Limit

The existing submission limit remains unchanged:

- 1st and 2nd submissions are allowed if the 7-day first-open window has not passed
- the 3rd submission closes the link immediately

### Priority Between Rules

The link is unavailable when either of these happens first:

- the 7-day first-open expiry is reached
- the 3rd submission is completed

## User-Facing Behavior

### Public Feedback Page

If the link is beyond 7 days from the first open:

- the form is hidden
- the request is shown as expired
- the message must no longer imply that no submissions were made

Recommended wording:

- English backend message:
  - `Feedback link expired 7 days after the first open.`
- Vietnamese frontend notice:
  - `Link feedback đã hết hạn sau 7 ngày kể từ lần mở đầu tiên.`

### Admin Test Flow

The existing `expire-opened` test endpoint remains conceptually valid:

- it moves `FirstOpenedAtUtc` into the past
- after that, opening the public feedback link should expire it even if 1 or 2 feedback entries already exist

## Technical Changes

### Backend

Update [MovieFeedbackService.cs](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/MovieFeedbackService.cs):

- replace the current open-without-submission expiry rule with a first-open hard expiry rule
- remove the `Feedbacks.Count == 0` condition from the expiry decision
- update request message text so it no longer says “without a submission”

No schema change is required.

### Frontend

Update [MoviePublicFeedbackPage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/pages/MoviePublicFeedbackPage.tsx):

- keep the same status handling
- change the expired notice copy to match the new business rule wording

## Testing

Tests should cover:

- opened 8 days ago, 0 submissions => expired
- opened 8 days ago, 2 submissions => expired
- opened 6 days ago, 2 submissions => still allowed
- 3rd submission before the 7-day limit => closes immediately with submission-limit reason
- public feedback page shows the new expiry notice wording

## Scope

In scope:

- change the 7-day expiry rule to depend only on first open
- keep 3-submission closure
- update user-facing messaging and tests

Out of scope:

- changing the email send schedule
- changing token generation
- changing how many submissions are allowed

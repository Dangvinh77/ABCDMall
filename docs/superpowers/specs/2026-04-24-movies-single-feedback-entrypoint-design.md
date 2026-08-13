# Movies Single Feedback Entrypoint Design

## Goal

Remove every direct feedback submission path from the regular movie pages and keep a single feedback submission flow: the tokenized public feedback page reached from the feedback email after a showtime has ended.

## Current Context

- [MovieDetailPage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/pages/MovieDetailPage.tsx) still contains a local submit form, submit state, `createMovieFeedback` calls, and browser `localStorage` fallback logic.
- [MoviePublicFeedbackPage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/pages/MoviePublicFeedbackPage.tsx) already represents the intended feedback flow:
  - token-based access
  - submit limit and expiry handling
  - only available after showtime end
- The product rule is now explicit: users should only be able to submit feedback through the emailed feedback link.

## Decision

Use a single feedback entrypoint:

- Keep feedback submission only on the public token route:
  - `/movies/feedback/:token`
- Make the movie detail page feedback section read-only:
  - keep rating summary
  - keep rating breakdown
  - keep feedback list
  - remove all submit controls and submit-side state

## Approach Options

### Option 1: Recommended

Remove the submit form from `MovieDetailPage` entirely and let the feedback list area become read-only content.

Why this is recommended:

- matches the domain rule exactly
- removes the duplicate submit path
- removes misleading browser-local fallback behavior
- reduces frontend state and API coupling on the movie detail page

### Option 2

Replace the existing form with a disabled form shell and explanatory copy.

Why this is weaker:

- still visually implies the movie page is part of the submit flow
- keeps a layout that suggests interaction without allowing it

### Option 3

Hide the form conditionally behind flags or environment checks.

Why this is rejected:

- preserves a second submit implementation in code
- conflicts with the requirement to have one feedback method only

## UX Changes

### Movie Detail Page

The feedback section remains visible but read-only:

- show audience score
- show per-rating breakdown
- show the list of feedback entries
- keep rating filters if they are already useful for browsing

The submit form area is removed.

Add a small English note in the feedback section:

`To join feedbacks, book now and watch for the feedback link in your email after the showtime ends.`

This note should be informational only, not a secondary submit path.

### Public Feedback Page

No behavioral change. It remains the only place where feedback can be submitted.

## Technical Changes

### Frontend

Update [MovieDetailPage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/pages/MovieDetailPage.tsx):

- remove `createMovieFeedback` import
- remove local submit form state:
  - reviewer name
  - rating input state for submission
  - comment input state
  - submitted success state
- remove `handleFeedbackSubmit`
- remove `localStorage` fallback helpers used only for local submission
- keep feedback fetching and display logic
- keep read-only feedback fallback content if needed for API failure resilience
- add a small read-only note that explains the email-based feedback flow

No backend change is required.

## Error Handling

- If loading movie feedback fails, the page may still fall back to bundled read-only feedback content as it does today.
- No local or offline submit fallback should remain anywhere on the movie detail page.
- The only writable feedback path is still controlled by the token page and backend lifecycle checks.

## Testing

Frontend verification should cover:

- the movie detail page no longer renders a feedback submit form
- the movie detail page still renders rating summary and feedback list
- the informational note about booking now and email feedback is visible
- the public feedback page still renders the form when `canSubmit = true`
- the public feedback page remains hidden/closed when the request cannot submit

## Scope

In scope:

- remove duplicate submit UI from regular movie pages
- preserve read-only feedback browsing on movie detail
- keep the token feedback page as the only submit path

Out of scope:

- changing backend feedback lifecycle rules
- changing email scheduling rules
- changing review aggregation logic

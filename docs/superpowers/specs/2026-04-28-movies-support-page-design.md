# Movies Support Page Design

Date: 2026-04-28

## Summary

Replace the `Snacks` action in the movies home header with a `Support` entry that uses a `?`-style help icon and routes users to a dedicated movies support page.

The support page will combine:

- static support contact information
- a structured FAQ for common booking questions
- one highlighted ticket recovery section for users who did not receive their ticket email

The highlighted recovery section must not show a static answer. Instead, it must render a resend request form that accepts:

- `email`
- `bookingCode`

and submits a resend request for the matching booking only.

## Goals

- Replace the current movies home `Snacks` header action with `Support`.
- Route users to a dedicated support page inside the movies module.
- Show clear support contact information, including support email.
- Provide multiple support questions with concise answers:
  - how to pay
  - how to submit feedbacks
  - how to book tickets
  - when the confirmation email arrives
- Highlight `I did not receive my ticket email` as the primary recovery path.
- Use a resend form that requires both `email` and `bookingCode`.

## Non-Goals

- Building a general support system for all mall modules.
- Adding chat, live agent, or ticket-tracking features.
- Reworking the movies booking flow outside support entry points.
- Defining final SMTP or infrastructure changes beyond what is required for resend.

## Current Context

### Frontend

- [MovieHomePage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/pages/MovieHomePage.tsx) currently renders a header action labeled `Snacks` with a `Popcorn` icon.
- The movies routes currently include home, promotions, showtimes, detail, booking, checkout, payment result, and public feedback routes, but no support route.
- The movies frontend already has its own route helper in [moviePaths.ts](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/routes/moviePaths.ts).

### Backend

- The movies backend already contains ticket email infrastructure and ticket dispatch services.
- The requested support form needs a dedicated resend endpoint that validates `email + bookingCode` before resending.
- The support page itself can ship with static FAQ content from frontend without needing a content management dependency.

## Recommended Approach

Use a dedicated movies support route and page:

- add `/movies/support`
- navigate there from the home header `Support` action
- keep FAQ content static in frontend for now
- submit resend requests through a movies-specific backend endpoint

This is the recommended approach because it keeps the experience focused, shareable, and easy to extend without overloading the home page UI.

## Alternative Approaches Considered

### Option 1: Recommended

Dedicated support page in the movies module.

Why this is recommended:

- clean separation from the home hero page
- easy to revisit and share by URL
- enough space for FAQ and the highlighted resend block
- aligns with the user request for a standalone support interface

### Option 2

Modal or overlay opened from the home page.

Why this is weaker:

- harder to fit multiple FAQ topics comfortably
- not shareable by URL
- less suitable for a recovery flow with a form and stateful submission

### Option 3

Reuse a generic mall FAQ page and add a movies subsection.

Why this is rejected:

- weaker visual consistency with the movies module
- support recovery flow becomes harder to prioritize
- introduces unnecessary cross-module coupling

## Detailed Design

### 1. Navigation Change

Update the movies home header in [MovieHomePage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/pages/MovieHomePage.tsx):

- replace the `Snacks` label with `Support`
- replace the `Popcorn` icon with a `?`-oriented help icon such as `CircleHelp`
- wire the button to `navigate(moviePaths.support())`

Update [moviePaths.ts](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/routes/moviePaths.ts):

- add `support: () => '/movies/support'`

Update [MovieRoutes.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/routes/MovieRoutes.tsx):

- add the new support route and page component

### 2. Support Page Information Architecture

Create a dedicated page component:

- [MovieSupportPage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/pages/MovieSupportPage.tsx)

The page should contain four sections:

#### Hero / Intro

- page title such as `Movie Support`
- short description covering booking, payment, feedback, and email support
- visible support email address
- large visual help marker using the `?` icon language introduced by the header button

#### FAQ Section

Render the common support topics as stacked cards or accordion items:

- `How to pay`
- `How to send feedbacks`
- `How to book tickets`
- `When will I receive my email ticket`

Each answer should stay short, direct, and operational.

#### Highlighted Ticket Email Recovery Section

Render a larger, visually stronger card titled:

- `I did not receive my ticket email`

This section must not include a static answer paragraph.

Instead, it must render:

- `Email address` field
- `Booking code` field
- `Request resend` button
- helper text explaining that resend is only available when the email matches the booking code

#### Support Policy / Notes

Render a compact note block below or beside the resend form:

- resend works only for a valid booking
- the entered email must match the booking email
- transient system issues may delay delivery

### 3. Visual Direction

The support page should preserve the movies module identity:

- cinematic dark background
- soft purple/cyan glow accents already used on the movies home page
- bold heading treatment
- clearer reading rhythm than the home hero page

Layout:

- desktop: two columns
  - FAQ column
  - highlighted resend card column
- mobile: one column
  - move the highlighted resend card above FAQ because it is the most urgent action

The resend card should be visually emphasized using:

- brighter border or glow
- stronger gradient background
- larger spacing and clearer form affordances

## Data and API Design

### Frontend Data Model

Static FAQ and support copy can live in frontend-local constants for the first version.

Suggested split:

- page-level static support metadata
- FAQ items array
- resend form state and request state

No CMS or backend content API is required for the initial release.

### Resend Ticket API

Add a movies support resend endpoint:

- `POST /api/movies/support/resend-ticket`

Request body:

- `email: string`
- `bookingCode: string`

Response behavior:

- success message when the resend request is accepted and processed
- validation error when email format or booking code is invalid
- not-found or business error when booking code does not exist
- mismatch error when email does not belong to the booking
- recoverable delivery error when resend cannot be completed temporarily

### Backend Validation Rules

Before resending:

- booking code must resolve to a real booking or ticket record
- provided email must match the booking email on record
- booking must be in a state that allows ticket email resend
- ticket payload must still be renderable or reproducible

No resend should occur if the email does not match the booking.

## UX States

### Idle

- email and booking code inputs enabled
- button labeled `Request resend`

### Submitting

- button enters loading state
- duplicate submissions are blocked temporarily
- inputs may be disabled during the request

### Success

- show a clear confirmation message near the form
- preserve the submitted values after success so the user can verify what was sent

### Error

Show explicit, user-readable error copy for cases such as:

- booking code not found
- email does not match booking
- resend unavailable right now

## Error Handling

### Frontend

- validate email format before submit
- require non-empty booking code
- prevent empty form submissions
- show inline status messages instead of relying on silent failure

### Backend

- reject invalid or mismatched resend requests deterministically
- avoid leaking excessive internal booking details
- return actionable messages that frontend can map to user-facing feedback

## Testing

### Frontend

Verification should cover:

- the movies home header renders `Support` instead of `Snacks`
- clicking `Support` routes to `/movies/support`
- the support page renders support email and all FAQ topics
- the highlighted ticket email section renders a form, not a static answer
- form validation blocks invalid email or empty booking code
- success and error submission states render correctly
- mobile layout still surfaces the resend section prominently

### Backend

Verification should cover:

- resend succeeds for a valid `email + bookingCode` pair
- resend fails when booking code does not exist
- resend fails when email does not match booking
- resend fails cleanly when ticket resend is temporarily unavailable

## Scope

In scope:

- movies home header action rename and icon swap
- new movies support route
- new movies support page UI
- static FAQ content for the first release
- highlighted resend form using `email + bookingCode`
- backend resend endpoint integration plan

Out of scope:

- general multi-module support center
- support content management system
- live support workflows
- unrelated booking UI redesign

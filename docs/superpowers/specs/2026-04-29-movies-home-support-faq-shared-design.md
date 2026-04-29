# Movies Home Support FAQ Shared Design

Date: 2026-04-29

## Summary

Replace the `Snacks` action on the movies home page with `Support`, route that action to the existing `/faq` page, and add a support preview block on the movies home page that uses the same support data source as the FAQ page.

The implementation must stop treating support content as movie-only demo UI. The movies home page and the FAQ page should consume the same support-facing data contract so the preview and the full support page stay aligned.

## Goals

- Replace the movies home header `Snacks` action with `Support`.
- Route the new `Support` action to the existing `/faq` page.
- Add a support preview section to the movies home page.
- Reuse one support data source for both the support preview and the FAQ page.
- Remove the current dependency on hardcoded FAQ content inside `FaqFeature`.
- Keep the support experience scoped to the existing mall support page instead of creating a second movies-only support route.

## Non-Goals

- Creating a new `/movies/support` route.
- Reworking the full FAQ visual design unless required by the new data flow.
- Building live chat, ticketing, or resend-ticket tooling in this change.
- Refactoring unrelated movies home sections.

## Current Context

### Frontend

- [MovieHomePage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/pages/MovieHomePage.tsx) renders a hardcoded `Snacks` button in the header and a promotions block, but no support block.
- [AppRoutes.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/routes/AppRoutes.tsx) already exposes the support page at `/faq`.
- [FaqPage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/pages/support/FaqPage.tsx) simply renders `FaqFeature`.
- [FaqFeature.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/faq/FaqFeature.tsx) currently owns category and FAQ data as frontend-local hardcoded arrays.

### Backend

- During repository inspection on 2026-04-29, no obvious FAQ or support content endpoint/controller was found under `BACKEND/**/*.cs`.
- This means the implementation plan must treat backend support integration as a verification item, not an assumption. If a support endpoint exists under a non-obvious name, the frontend should connect to it. If it does not exist, the implementation should stop and surface the gap instead of silently inventing one.

## Recommended Approach

Use the existing `/faq` page as the canonical support destination and introduce a shared support data layer in frontend:

- movies header button links to `/faq`
- movies home renders a support preview section
- FAQ page and support preview both read from a shared adapter or hook
- the adapter is responsible for mapping backend support data into a frontend-friendly shape

This is the recommended approach because it avoids duplicate support pages, keeps navigation simple, and creates one place to change support content behavior later.

## Alternative Approaches Considered

### Option 1: Recommended

Shared support data source with `/faq` as the canonical page and a preview block on movies home.

Why this is recommended:

- meets both requested behaviors
- avoids duplicate support destinations
- keeps support content aligned across pages
- supports a backend-backed content source when the endpoint is confirmed

### Option 2

Replace `Snacks` with a `/faq` link only and do not add a support preview block.

Why this is weaker:

- only satisfies half of the request
- users do not see support content on the movies landing page

### Option 3

Render full FAQ content directly on movies home and keep `/faq` separate.

Why this is rejected:

- duplicates support presentation
- makes movies home unnecessarily long
- increases the risk that `/faq` and movies home drift out of sync

## Detailed Design

### 1. Navigation Change

Update the movies home header in [MovieHomePage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/pages/MovieHomePage.tsx):

- replace the `Snacks` label with `Support`
- replace the `Popcorn` icon with a help-oriented icon
- route the button to `/faq`

This change should not introduce a new movies route helper unless one is actually needed for readability.

### 2. Shared Support Data Layer

Create a small support-facing frontend data layer that sits between UI components and raw support data.

Responsibilities:

- fetch support FAQ data from backend if an endpoint exists
- map raw category/question data into a stable UI model
- expose one loader that both the FAQ page and movies home can use
- centralize fallback/error handling instead of duplicating it in multiple pages

Expected output shape:

- support categories
- question/answer items grouped by category
- optional summary metadata for preview use, such as top questions or featured categories

The shared layer should live near the support/FAQ feature or a common API folder, not inside the movies feature.

### 3. FAQ Page Refactor

Refactor [FaqFeature.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/faq/FaqFeature.tsx) so it consumes the shared support data layer instead of hardcoded arrays.

Behavior:

- keep the current category-tab and accordion interaction model unless the data contract forces a small adjustment
- show loading, empty, and error states explicitly
- preserve `/faq` as the full support destination

### 4. Movies Home Support Preview

Add a new support preview section to [MovieHomePage.tsx](E:/DEV/Coding_Resource/Project/e_PROJECT/Semester3/eProjectSem3_Group2_ABCDMall_T2.2410.E0/CODE/FRONTEND/src/features/movies/pages/MovieHomePage.tsx).

Content:

- section title such as `Need help?`
- short support-oriented copy for booking and mall questions
- 2-4 featured FAQ items sourced from the same support data layer as `/faq`
- a clear CTA button linking to `/faq`

Placement:

- below promotions or near the lower content sections, where it reads as support/discovery rather than a primary booking action

Presentation:

- preserve the cinematic movies styling
- keep the block more compact and calmer than the hero/promotions sections
- do not render the full accordion page inside movies home

### 5. Error Handling and Fallback Rules

Support content must not quietly fall back to hidden demo data.

Rules:

- if backend support data loads successfully, both `/faq` and the movies support preview use it
- if the support endpoint is missing or integration is blocked, implementation must surface that explicitly during development rather than silently shipping contradictory data paths
- UI fallback may show a limited empty-state message and a CTA to `/feedback`, but only after the integration path is intentionally defined

## Testing Strategy

- Add or update frontend tests for the movies header navigation change.
- Add or update FAQ feature tests for backend-driven data rendering.
- Add or update movies home tests for support preview rendering, loading states, and `/faq` CTA behavior.
- Verify that removing local hardcoded FAQ arrays does not break `/faq` interactions.

## Open Constraint

The key implementation constraint is backend discovery:

- if a support FAQ endpoint already exists under a non-obvious module or route, use it
- if no such endpoint exists, stop and raise the mismatch because the requested backend-backed support source is not actually present in the inspected codebase

This constraint should be resolved before implementation code is written.

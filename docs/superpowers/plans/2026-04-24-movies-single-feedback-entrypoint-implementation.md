# Movies Single Feedback Entrypoint Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove direct feedback submission from the regular movie detail page while keeping rating summary and feedback browsing intact, so the token-based public feedback page is the only writable feedback flow.

**Architecture:** Keep the existing backend and public token feedback flow unchanged. Refactor `MovieDetailPage` into a read-only feedback consumer, add a small English guidance note for the email-based flow, and cover the change with focused Vitest tests plus a regression check for the existing public feedback page.

**Tech Stack:** React 19, React Router, Vitest, Testing Library, TypeScript

---

## File Structure

- Modify: `FRONTEND/src/features/movies/pages/MovieDetailPage.tsx`
  - Remove submit-only imports, local submit state, handler, and browser `localStorage` fallback.
  - Keep read-only feedback fetch, summary, breakdown, and list rendering.
  - Add the small English booking/email note.
- Create: `FRONTEND/src/features/movies/pages/__tests__/MovieDetailPage.test.tsx`
  - Add focused tests proving the page is read-only and still renders feedback browsing content.
- Verify: `FRONTEND/src/features/movies/pages/__tests__/MoviePublicFeedbackPage.test.tsx`
  - Keep the existing regression coverage for the public token page as the single submit flow.

### Task 1: Add read-only movie detail tests first

**Files:**
- Create: `FRONTEND/src/features/movies/pages/__tests__/MovieDetailPage.test.tsx`

- [ ] **Step 1: Write the failing read-only test file**

```tsx
import { render, screen } from "@testing-library/react";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MovieDetailPage } from "../MovieDetailPage";

const loadMovieDetailUiDataMock = vi.hoisted(() => vi.fn());
const fetchMovieFeedbacksMock = vi.hoisted(() => vi.fn());

vi.mock("../../api/movieUiAdapter", () => ({
  loadMovieDetailUiData: loadMovieDetailUiDataMock,
}));

vi.mock("../../api/moviesApi", () => ({
  fetchMovieFeedbacks: fetchMovieFeedbacksMock,
}));

describe("MovieDetailPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    loadMovieDetailUiDataMock.mockResolvedValue({
      movie: {
        id: "movie-now-1",
        apiId: "movie-api-1",
        title: "Cosmic Odyssey",
        imageUrl: "/poster.jpg",
        genre: "Sci-Fi, Drama",
        rating: 8.4,
        duration: "128 min",
        language: "English",
        ageRating: "T16",
        description: "A long journey through deep space.",
        director: "Jane Doe",
        cast: ["Actor One", "Actor Two"],
        isComingSoon: false,
        releaseDate: "24/04/2026",
      },
      movieSchedule: undefined,
    });
    fetchMovieFeedbacksMock.mockResolvedValue({
      movieId: "movie-api-1",
      totalCount: 1,
      averageRating: 4.5,
      ratingBreakdown: { 5: 1, 4: 0, 3: 0, 2: 0, 1: 0 },
      items: [
        {
          id: "feedback-1",
          movieId: "movie-api-1",
          displayName: "Alex",
          rating: 5,
          comment: "Great movie.",
          createdAtUtc: "2026-04-24T10:00:00Z",
        },
      ],
    });
  });

  it("renders feedback browsing content without a submit form", async () => {
    renderPage();

    expect(await screen.findByText(/ratings & feedback/i)).toBeInTheDocument();
    expect(screen.getByText(/Audience score/i)).toBeInTheDocument();
    expect(screen.getByText(/Alex/i)).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /submit feedback/i })).not.toBeInTheDocument();
    expect(screen.queryByLabelText(/name/i)).not.toBeInTheDocument();
    expect(
      screen.getByText(/To join feedbacks, book now and watch for the feedback link in your email after the showtime ends./i),
    ).toBeInTheDocument();
  });

  function renderPage() {
    render(
      <MemoryRouter initialEntries={["/movies/movie-api-1"]}>
        <Routes>
          <Route path="/movies/:movieId" element={<MovieDetailPage />} />
        </Routes>
      </MemoryRouter>,
    );
  }
});
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `npm --prefix FRONTEND run test -- MovieDetailPage`

Expected: FAIL because `MovieDetailPage` still renders the submit form and submit controls.

- [ ] **Step 3: Commit the red test**

```bash
git add FRONTEND/src/features/movies/pages/__tests__/MovieDetailPage.test.tsx
git commit -m "test: cover movie detail feedback read-only mode"
```

### Task 2: Remove the duplicate submit path from MovieDetailPage

**Files:**
- Modify: `FRONTEND/src/features/movies/pages/MovieDetailPage.tsx`

- [ ] **Step 1: Remove submit-only imports and state**

Replace the import block header so `createMovieFeedback`, `Send`, and submit-only state are removed:

```tsx
import { useEffect, useMemo, useState } from 'react';
import { useLocation, useNavigate, useParams, useSearchParams } from 'react-router-dom';
import {
  ArrowLeft,
  Star,
  Clock,
  Calendar,
  Film,
  Users,
  Globe,
  Shield,
  ChevronRight,
  Ticket,
  Play,
  MessageSquare,
} from 'lucide-react';
import { getDefaultBookingDate } from '../data/promotions';
import { formatScheduleDateParam, getScheduleDates, type MovieSchedule } from '../data/schedules';
import { Button } from '../component/ui/button';
import { Badge } from '../component/ui/badge';
import { moviePaths } from '../routes/moviePaths';
import { loadMovieDetailUiData } from '../api/movieUiAdapter';
import { fetchMovieFeedbacks } from '../api/moviesApi';
import type { Movie } from '../data/movie';
```

Also remove these state declarations:

```tsx
const [reviewerName, setReviewerName] = useState('');
const [reviewRating, setReviewRating] = useState(5);
const [reviewComment, setReviewComment] = useState('');
const [feedbackSubmitted, setFeedbackSubmitted] = useState(false);
```

And remove these helpers entirely because they only supported local submit fallback:

```tsx
function getFeedbackStorageKey(movieId: string) {
  return `abcd-cinema-feedback:${movieId}`;
}
```

- [ ] **Step 2: Remove submit fallback logic from the feedback-loading effect**

Replace the feedback-related setup near the top:

```tsx
const feedbackMovieId = movie?.apiId ?? movieId;
const defaultFeedback = useMemo(() => (movie ? buildDefaultMovieFeedback(movie) : []), [movie]);
const feedbacks = useMemo(() => {
  const source = submittedFeedback.length > 0 ? submittedFeedback : defaultFeedback;
  return feedbackRatingFilter ? source.filter((feedback) => feedback.rating === feedbackRatingFilter) : source;
}, [defaultFeedback, feedbackRatingFilter, submittedFeedback]);
```

Update the effect so it no longer depends on `feedbackStorageKey`, does not touch `localStorage`, and does not reset submit form state:

```tsx
useEffect(() => {
  if (!feedbackMovieId) {
    setSubmittedFeedback([]);
    return;
  }

  let active = true;

  async function loadFeedbacks() {
    try {
      const response = await fetchMovieFeedbacks(feedbackMovieId as string, feedbackRatingFilter);
      if (!active) return;

      setSubmittedFeedback(response.items.map((feedback) => ({
        id: feedback.id,
        author: feedback.displayName,
        rating: feedback.rating,
        comment: feedback.comment,
        createdAt: formatFeedbackDate(feedback.createdAtUtc),
      })));
      setApiAverageRating(response.averageRating);
      setApiRatingBreakdown(response.ratingBreakdown);
    } catch (error) {
      if (!active) return;

      setSubmittedFeedback(defaultFeedback);
      setApiAverageRating(null);
      setApiRatingBreakdown(null);
      console.warn('Movie feedback API failed; using bundled read-only fallback feedback.', error);
    }
  }

  void loadFeedbacks();

  return () => {
    active = false;
  };
}, [defaultFeedback, feedbackMovieId, feedbackRatingFilter]);
```

- [ ] **Step 3: Remove the submit handler**

Delete the entire `handleFeedbackSubmit` function:

```tsx
const handleFeedbackSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
  // remove the entire function
};
```

- [ ] **Step 4: Replace the form column with a small informational note**

Replace the right-hand `<form ...>` block in the feedback section with this read-only panel:

```tsx
<aside className="rounded-2xl bg-gray-800/50 p-5 ring-1 ring-gray-700/50">
  <h3 className="text-lg font-bold text-white">Feedback access</h3>
  <p className="mt-2 text-sm leading-relaxed text-gray-300">
    To join feedbacks, book now and watch for the feedback link in your email after the showtime ends.
  </p>
</aside>
```

Keep the rest of the feedback list and filter UI unchanged.

- [ ] **Step 5: Run the focused test to verify it passes**

Run: `npm --prefix FRONTEND run test -- MovieDetailPage`

Expected: PASS with `1 passed`.

- [ ] **Step 6: Commit the implementation**

```bash
git add FRONTEND/src/features/movies/pages/MovieDetailPage.tsx FRONTEND/src/features/movies/pages/__tests__/MovieDetailPage.test.tsx
git commit -m "feat: make movie detail feedback read only"
```

### Task 3: Run frontend regression verification

**Files:**
- Test: `FRONTEND/src/features/movies/pages/__tests__/MovieDetailPage.test.tsx`
- Test: `FRONTEND/src/features/movies/pages/__tests__/MoviePublicFeedbackPage.test.tsx`

- [ ] **Step 1: Run the focused feedback page regression set**

Run: `npm --prefix FRONTEND run test -- "MovieDetailPage|MoviePublicFeedbackPage"`

Expected: PASS for the new read-only movie detail test and the existing public feedback page tests.

- [ ] **Step 2: Run the frontend build**

Run: `npm --prefix FRONTEND run build`

Expected: PASS with a successful Vite build.

- [ ] **Step 3: Inspect git status**

Run: `git status --short`

Expected:
- only the intended frontend files and optional plan file are changed
- unrelated local backend changes, if still present, must remain untouched

- [ ] **Step 4: Commit the plan if it is intentionally tracked**

```bash
git add docs/superpowers/plans/2026-04-24-movies-single-feedback-entrypoint-implementation.md
git commit -m "docs: add movies single feedback entrypoint plan"
```

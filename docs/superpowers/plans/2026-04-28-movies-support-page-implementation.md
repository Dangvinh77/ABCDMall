# Movies Support Page Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the movies home `Snacks` header action with `Support`, add a dedicated `/movies/support` page with FAQ and a highlighted resend form, and wire the form to a public backend resend endpoint that validates `email + bookingCode`.

**Architecture:** Keep the support page inside the existing `movies` frontend module, with static FAQ content rendered locally and a single API call for resend requests. On the backend, add a small public support service and controller that reuse the existing booking lookup and ticket email dispatcher while enforcing booking-code lookup, email match, and confirmed-booking rules before resending.

**Tech Stack:** React 19, React Router, TypeScript, Vitest, Testing Library, ASP.NET Core Web API, FluentValidation, xUnit

---

## File Structure

- Modify: `FRONTEND/src/features/movies/pages/MovieHomePage.tsx`
  - Replace the `Snacks` button with `Support`, swap the icon, and route to the new support page.
- Modify: `FRONTEND/src/features/movies/routes/moviePaths.ts`
  - Add the support route helper.
- Modify: `FRONTEND/src/features/movies/routes/MovieRoutes.tsx`
  - Register the new support page route.
- Modify: `FRONTEND/src/features/movies/api/moviesApi.ts`
  - Add the resend request payload/model and the `requestTicketEmailResend()` API helper.
- Create: `FRONTEND/src/features/movies/data/support.ts`
  - Store support email and static FAQ items.
- Create: `FRONTEND/src/features/movies/pages/MovieSupportPage.tsx`
  - Render the support hero, FAQ content, and highlighted resend form.
- Create: `FRONTEND/src/features/movies/pages/__tests__/MovieHomePage.test.tsx`
  - Cover the renamed header action and navigation to the support route.
- Create: `FRONTEND/src/features/movies/pages/__tests__/MovieSupportPage.test.tsx`
  - Cover FAQ rendering, client validation, and successful resend requests.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Support/RequestTicketEmailResendRequestDto.cs`
  - Define the public resend request contract.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Support/RequestTicketEmailResendResponseDto.cs`
  - Define the simple success response payload.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Support/IMovieSupportService.cs`
  - Define the public support service contract.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Support/ITicketEmailResendGateway.cs`
  - Abstract the resend side effect so the service is testable.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Support/MovieSupportService.cs`
  - Validate booking existence, email match, and confirmed status before delegating resend.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Support/Validators/RequestTicketEmailResendRequestDtoValidator.cs`
  - Validate required email and booking code fields.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DependencyInjection.cs`
  - Register the new validator and support service.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Tickets/TicketEmailResendGateway.cs`
  - Adapt the existing ticket dispatcher to the new application abstraction.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/DependencyInjection.cs`
  - Register the resend gateway.
- Create: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesSupportController.cs`
  - Expose `POST /api/movies/support/resend-ticket`.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/RequestTicketEmailResendRequestDtoValidatorTests.cs`
  - Cover validator failures.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieSupportServiceTests.cs`
  - Cover not-found, email mismatch, non-confirmed booking, and success flows.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesSupportControllerTests.cs`
  - Cover controller validation, success, and business-error translation.

### Task 1: Write the frontend route and support-page tests first

**Files:**
- Create: `FRONTEND/src/features/movies/pages/__tests__/MovieHomePage.test.tsx`
- Create: `FRONTEND/src/features/movies/pages/__tests__/MovieSupportPage.test.tsx`

- [ ] **Step 1: Write the failing home header navigation test**

```tsx
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter, Route, Routes } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MovieHomePage } from "../MovieHomePage";

const loadHomeUiDataMock = vi.hoisted(() => vi.fn());

vi.mock("../../api/movieUiAdapter", () => ({
  loadHomeUiData: loadHomeUiDataMock,
}));

describe("MovieHomePage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    loadHomeUiDataMock.mockResolvedValue({
      nowShowingMovies: [],
      comingSoonMovies: [],
      promos: [],
    });
  });

  it("routes to the support page from the header support action", async () => {
    const user = userEvent.setup();

    render(
      <MemoryRouter initialEntries={["/movies"]}>
        <Routes>
          <Route path="/movies" element={<MovieHomePage />} />
          <Route path="/movies/support" element={<div>Support route reached</div>} />
        </Routes>
      </MemoryRouter>,
    );

    await user.click(screen.getByRole("button", { name: /support/i }));

    expect(await screen.findByText(/support route reached/i)).toBeInTheDocument();
    expect(screen.queryByRole("button", { name: /snacks/i })).not.toBeInTheDocument();
  });
});
```

- [ ] **Step 2: Write the failing support-page rendering and submit tests**

```tsx
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { MovieSupportPage } from "../MovieSupportPage";

const requestTicketEmailResendMock = vi.hoisted(() => vi.fn());

vi.mock("../../api/moviesApi", () => ({
  requestTicketEmailResend: requestTicketEmailResendMock,
}));

describe("MovieSupportPage", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders support email, faq topics, and the resend form", () => {
    render(
      <MemoryRouter>
        <MovieSupportPage />
      </MemoryRouter>,
    );

    expect(screen.getByRole("heading", { name: /movie support/i })).toBeInTheDocument();
    expect(screen.getByText(/support-cinema@abcdmall.com/i)).toBeInTheDocument();
    expect(screen.getByText(/how to pay/i)).toBeInTheDocument();
    expect(screen.getByText(/how to send feedbacks/i)).toBeInTheDocument();
    expect(screen.getByText(/how to book tickets/i)).toBeInTheDocument();
    expect(screen.getByText(/when will i receive my email ticket/i)).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: /i did not receive my ticket email/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/email address/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/booking code/i)).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /request resend/i })).toBeInTheDocument();
  });

  it("blocks invalid email and submits a valid resend request", async () => {
    const user = userEvent.setup();
    requestTicketEmailResendMock.mockResolvedValue({
      message: "Ticket email resent successfully.",
    });

    render(
      <MemoryRouter>
        <MovieSupportPage />
      </MemoryRouter>,
    );

    await user.type(screen.getByLabelText(/email address/i), "invalid-email");
    await user.type(screen.getByLabelText(/booking code/i), "BOOK-2026-001");
    await user.click(screen.getByRole("button", { name: /request resend/i }));

    expect(screen.getByText(/enter a valid email address/i)).toBeInTheDocument();
    expect(requestTicketEmailResendMock).not.toHaveBeenCalled();

    await user.clear(screen.getByLabelText(/email address/i));
    await user.type(screen.getByLabelText(/email address/i), "guest@example.com");
    await user.click(screen.getByRole("button", { name: /request resend/i }));

    await waitFor(() =>
      expect(requestTicketEmailResendMock).toHaveBeenCalledWith({
        email: "guest@example.com",
        bookingCode: "BOOK-2026-001",
      }),
    );

    expect(await screen.findByText(/ticket email resent successfully/i)).toBeInTheDocument();
  });
});
```

- [ ] **Step 3: Run the frontend tests to verify they fail**

Run: `npm --prefix FRONTEND run test -- "MovieHomePage|MovieSupportPage"`

Expected: FAIL because the home page still exposes `Snacks`, there is no support route/page, and the resend API helper does not exist yet.

- [ ] **Step 4: Commit the red frontend tests**

```bash
git add FRONTEND/src/features/movies/pages/__tests__/MovieHomePage.test.tsx FRONTEND/src/features/movies/pages/__tests__/MovieSupportPage.test.tsx
git commit -m "test: cover movies support navigation and page"
```

### Task 2: Implement the frontend support route, page, and resend client

**Files:**
- Modify: `FRONTEND/src/features/movies/pages/MovieHomePage.tsx`
- Modify: `FRONTEND/src/features/movies/routes/moviePaths.ts`
- Modify: `FRONTEND/src/features/movies/routes/MovieRoutes.tsx`
- Modify: `FRONTEND/src/features/movies/api/moviesApi.ts`
- Create: `FRONTEND/src/features/movies/data/support.ts`
- Create: `FRONTEND/src/features/movies/pages/MovieSupportPage.tsx`

- [ ] **Step 1: Add the support route helper and route registration**

Update `FRONTEND/src/features/movies/routes/moviePaths.ts`:

```ts
export const moviePaths = {
  home: () => "/movies",
  support: () => "/movies/support",
  promotions: () => "/movies/promotions",
  admin: () => "/movies/admin",
  showtimes: () => "/movies/showtimes",
  detail: (movieId: string) => `/movies/${movieId}`,
  booking: (movieId: string) => `/movies/${movieId}/booking`,
  checkout: (movieId: string) => `/movies/${movieId}/checkout`,
};
```

Update `FRONTEND/src/features/movies/routes/MovieRoutes.tsx`:

```tsx
import { Navigate, Route, Routes } from "react-router-dom";
import { MovieHomePage } from "../pages/MovieHomePage";
import { MovieDetailPage } from "../pages/MovieDetailPage";
import { PromotionsPage } from "../pages/PromotionsPage";
import { SchedulePage } from "../pages/SchedulesPage";
import { SeatSelectionPage } from "../pages/SeatSelectionPage";
import { CheckoutPage } from "../pages/CheckOutPage";
import { MoviePaymentSuccessPage } from "../pages/MoviePaymentSuccessPage";
import { MoviePaymentCancelPage } from "../pages/MoviePaymentCancelPage";
import { MoviePublicFeedbackPage } from "../pages/MoviePublicFeedbackPage";
import { MovieSupportPage } from "../pages/MovieSupportPage";

export function MoviesRoutes() {
  return (
    <Routes>
      <Route index element={<MovieHomePage />} />
      <Route path="support" element={<MovieSupportPage />} />
      <Route path="promotions" element={<PromotionsPage />} />
      <Route path="showtimes" element={<SchedulePage />} />
      <Route path="feedback/:token" element={<MoviePublicFeedbackPage />} />
      <Route path=":movieId" element={<MovieDetailPage />} />
      <Route path=":movieId/booking" element={<SeatSelectionPage />} />
      <Route path=":movieId/checkout" element={<CheckoutPage />} />
      <Route path="payment/success" element={<MoviePaymentSuccessPage />} />
      <Route path="payment/cancel" element={<MoviePaymentCancelPage />} />
      <Route path="*" element={<Navigate to="/movies" replace />} />
    </Routes>
  );
}
```

- [ ] **Step 2: Replace the home header action with Support**

Update the icon import and button block in `FRONTEND/src/features/movies/pages/MovieHomePage.tsx`:

```tsx
import {
  Film,
  CircleHelp,
  Clock,
  CalendarDays,
  TrendingUp,
  Sparkles,
  ChevronRight,
  ChevronLeft,
  ShieldUser,
} from "lucide-react";
```

```tsx
<Button
  variant="ghost"
  onClick={() => navigate(moviePaths.support())}
  className="hidden h-10 px-3 text-base font-semibold text-gray-200 hover:bg-white/20 hover:text-white md:inline-flex"
>
  <CircleHelp className="mr-2 size-4" />
  Support
</Button>
```

- [ ] **Step 3: Add the support content constants and resend API helper**

Create `FRONTEND/src/features/movies/data/support.ts`:

```ts
export const movieSupportEmail = "support-cinema@abcdmall.com";

export const movieSupportFaqs = [
  {
    question: "How to pay",
    answer:
      "Choose your seats, continue to checkout, complete payment, and wait for the confirmation screen before leaving the page.",
  },
  {
    question: "How to send feedbacks",
    answer:
      "After your showtime ends, watch for the feedback email and open the link inside that message to submit your movie feedback.",
  },
  {
    question: "How to book tickets",
    answer:
      "Pick a movie, choose a showtime, select seats, review your order, and finish payment to confirm the booking.",
  },
  {
    question: "When will I receive my email ticket",
    answer:
      "Your ticket email should arrive shortly after payment succeeds. Delivery can take a few minutes during busy periods.",
  },
] as const;
```

Add the API model and helper in `FRONTEND/src/features/movies/api/moviesApi.ts`:

```ts
export interface RequestTicketEmailResendPayload {
  email: string;
  bookingCode: string;
}

export interface RequestTicketEmailResendModel {
  message: string;
}
```

```ts
export async function requestTicketEmailResend(
  payload: RequestTicketEmailResendPayload,
): Promise<RequestTicketEmailResendModel> {
  return api.post<RequestTicketEmailResendModel, RequestTicketEmailResendPayload>(
    "/movies/support/resend-ticket",
    payload,
  );
}
```

- [ ] **Step 4: Create the support page with FAQ and resend states**

Create `FRONTEND/src/features/movies/pages/MovieSupportPage.tsx`:

```tsx
import { useState } from "react";
import { CircleHelp, Mail, RefreshCcw, Ticket } from "lucide-react";
import { Button } from "../component/ui/button";
import { movieSupportEmail, movieSupportFaqs } from "../data/support";
import { requestTicketEmailResend } from "../api/moviesApi";

type SubmitState = "idle" | "submitting" | "success" | "error";

export function MovieSupportPage() {
  const [email, setEmail] = useState("");
  const [bookingCode, setBookingCode] = useState("");
  const [emailError, setEmailError] = useState("");
  const [bookingCodeError, setBookingCodeError] = useState("");
  const [submitState, setSubmitState] = useState<SubmitState>("idle");
  const [submitMessage, setSubmitMessage] = useState("");

  const validate = () => {
    let isValid = true;
    setEmailError("");
    setBookingCodeError("");

    const trimmedEmail = email.trim();
    const trimmedBookingCode = bookingCode.trim();
    const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

    if (!emailPattern.test(trimmedEmail)) {
      setEmailError("Enter a valid email address.");
      isValid = false;
    }

    if (!trimmedBookingCode) {
      setBookingCodeError("Booking code is required.");
      isValid = false;
    }

    return isValid;
  };

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    event.preventDefault();

    if (!validate()) {
      setSubmitState("error");
      setSubmitMessage("");
      return;
    }

    setSubmitState("submitting");
    setSubmitMessage("");

    try {
      const response = await requestTicketEmailResend({
        email: email.trim(),
        bookingCode: bookingCode.trim(),
      });

      setSubmitState("success");
      setSubmitMessage(response.message);
    } catch (error) {
      const message = error instanceof Error ? error.message : "Unable to resend the ticket email right now.";
      setSubmitState("error");
      setSubmitMessage(message);
    }
  };

  return (
    <div className="min-h-screen bg-[radial-gradient(circle_at_top,_rgba(139,92,246,0.2),_transparent_30%),radial-gradient(circle_at_80%_20%,_rgba(6,182,212,0.16),_transparent_24%),linear-gradient(to_bottom,_#030712,_#111827,_#030712)] px-4 py-10 text-white sm:px-6 lg:px-8">
      <div className="mx-auto max-w-7xl">
        <section className="rounded-3xl border border-white/10 bg-white/5 p-6 shadow-[0_24px_80px_rgba(3,7,18,0.35)] backdrop-blur-md sm:p-8">
          <div className="flex flex-col gap-6 lg:flex-row lg:items-center lg:justify-between">
            <div className="max-w-2xl space-y-4">
              <p className="text-sm uppercase tracking-[0.35em] text-cyan-200/75">ABCD Cinema Support</p>
              <h1 className="text-3xl font-black uppercase tracking-[0.08em] sm:text-4xl">Movie Support</h1>
              <p className="text-sm leading-7 text-gray-300 sm:text-base">
                Need help with booking, payment, feedbacks, or ticket delivery? Start with the answers below or request a ticket email resend for a confirmed booking.
              </p>
              <div className="inline-flex items-center gap-3 rounded-full border border-cyan-300/20 bg-cyan-400/10 px-4 py-2 text-sm text-cyan-100">
                <Mail className="size-4" />
                <span>{movieSupportEmail}</span>
              </div>
            </div>

            <div className="flex size-24 items-center justify-center rounded-full bg-gradient-to-br from-fuchsia-500/30 via-violet-500/30 to-cyan-400/30 ring-1 ring-white/10 sm:size-28">
              <CircleHelp className="size-12 text-white sm:size-14" />
            </div>
          </div>
        </section>

        <section className="mt-8 grid gap-6 lg:grid-cols-[1.1fr_0.9fr]">
          <div className="space-y-4">
            {movieSupportFaqs.map((item) => (
              <article key={item.question} className="rounded-2xl border border-white/10 bg-slate-900/50 p-5 shadow-[0_18px_40px_rgba(3,7,18,0.28)]">
                <h2 className="text-lg font-bold text-white">{item.question}</h2>
                <p className="mt-2 text-sm leading-7 text-gray-300">{item.answer}</p>
              </article>
            ))}
          </div>

          <aside className="rounded-3xl border border-fuchsia-400/30 bg-gradient-to-br from-fuchsia-500/12 via-slate-900/88 to-cyan-500/12 p-6 shadow-[0_20px_60px_rgba(168,85,247,0.22)]">
            <div className="flex items-center gap-3">
              <div className="flex size-12 items-center justify-center rounded-full bg-fuchsia-500/20 ring-1 ring-fuchsia-300/20">
                <Ticket className="size-5 text-fuchsia-100" />
              </div>
              <div>
                <h2 className="text-xl font-black uppercase tracking-[0.05em]">I did not receive my ticket email</h2>
                <p className="mt-1 text-sm text-gray-300">Use the same email address and booking code from the confirmed order.</p>
              </div>
            </div>

            <form className="mt-6 space-y-4" onSubmit={handleSubmit} noValidate>
              <div>
                <label className="mb-2 block text-sm font-semibold text-gray-200" htmlFor="support-email">
                  Email address
                </label>
                <input
                  id="support-email"
                  type="email"
                  value={email}
                  onChange={(event) => setEmail(event.target.value)}
                  disabled={submitState === "submitting"}
                  className="w-full rounded-2xl border border-white/10 bg-slate-950/60 px-4 py-3 text-sm text-white outline-none transition focus:border-cyan-300/60"
                />
                {emailError ? <p className="mt-2 text-sm text-rose-300">{emailError}</p> : null}
              </div>

              <div>
                <label className="mb-2 block text-sm font-semibold text-gray-200" htmlFor="support-booking-code">
                  Booking code
                </label>
                <input
                  id="support-booking-code"
                  value={bookingCode}
                  onChange={(event) => setBookingCode(event.target.value)}
                  disabled={submitState === "submitting"}
                  className="w-full rounded-2xl border border-white/10 bg-slate-950/60 px-4 py-3 text-sm text-white outline-none transition focus:border-cyan-300/60"
                />
                {bookingCodeError ? <p className="mt-2 text-sm text-rose-300">{bookingCodeError}</p> : null}
              </div>

              <Button
                type="submit"
                disabled={submitState === "submitting"}
                className="w-full rounded-2xl bg-gradient-to-r from-fuchsia-600 via-violet-500 to-cyan-500 text-white hover:from-fuchsia-500 hover:via-violet-400 hover:to-cyan-400"
              >
                <RefreshCcw className="mr-2 size-4" />
                {submitState === "submitting" ? "Sending request..." : "Request resend"}
              </Button>

              {submitMessage ? (
                <p className={`text-sm ${submitState === "success" ? "text-emerald-300" : "text-rose-300"}`}>{submitMessage}</p>
              ) : null}

              <p className="text-sm leading-6 text-gray-300">
                We only resend tickets for a valid confirmed booking when the booking code matches the email on record.
              </p>
            </form>
          </aside>
        </section>
      </div>
    </div>
  );
}
```

- [ ] **Step 5: Run the frontend tests to verify they pass**

Run: `npm --prefix FRONTEND run test -- "MovieHomePage|MovieSupportPage"`

Expected: PASS with the new home-page navigation test and the support-page rendering/submission tests.

- [ ] **Step 6: Commit the frontend implementation**

```bash
git add FRONTEND/src/features/movies/pages/MovieHomePage.tsx FRONTEND/src/features/movies/routes/moviePaths.ts FRONTEND/src/features/movies/routes/MovieRoutes.tsx FRONTEND/src/features/movies/api/moviesApi.ts FRONTEND/src/features/movies/data/support.ts FRONTEND/src/features/movies/pages/MovieSupportPage.tsx FRONTEND/src/features/movies/pages/__tests__/MovieHomePage.test.tsx FRONTEND/src/features/movies/pages/__tests__/MovieSupportPage.test.tsx
git commit -m "feat: add movies support page"
```

### Task 3: Write the backend validator, service, and controller tests first

**Files:**
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/RequestTicketEmailResendRequestDtoValidatorTests.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieSupportServiceTests.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesSupportControllerTests.cs`

- [ ] **Step 1: Write the failing validator test**

```csharp
using ABCDMall.Modules.Movies.Application.DTOs.Support;
using ABCDMall.Modules.Movies.Application.Services.Support.Validators;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class RequestTicketEmailResendRequestDtoValidatorTests
{
    [Fact]
    public async Task Should_require_valid_email_and_booking_code()
    {
        var validator = new RequestTicketEmailResendRequestDtoValidator();
        var model = new RequestTicketEmailResendRequestDto
        {
            Email = "invalid-email",
            BookingCode = ""
        };

        var result = await validator.ValidateAsync(model);

        Assert.Contains(result.Errors, error => error.PropertyName == "Email");
        Assert.Contains(result.Errors, error => error.PropertyName == "BookingCode");
    }
}
```

- [ ] **Step 3: Write the failing controller tests**

```csharp
using ABCDMall.Modules.Movies.Application.DTOs.Support;
using ABCDMall.Modules.Movies.Application.Services.Support;
using ABCDMall.Modules.Movies.Application.Services.Support.Validators;
using ABCDMall.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class MoviesSupportControllerTests
{
    [Fact]
    public async Task RequestTicketEmailResend_should_return_validation_problem_for_invalid_payload()
    {
        var controller = new MoviesSupportController(
            new FakeMovieSupportService(),
            new RequestTicketEmailResendRequestDtoValidator());

        var result = await controller.RequestTicketEmailResend(new RequestTicketEmailResendRequestDto
        {
            Email = "bad-email",
            BookingCode = ""
        });

        Assert.IsType<ObjectResult>(result.Result);
    }

    [Fact]
    public async Task RequestTicketEmailResend_should_return_ok_for_valid_payload()
    {
        var controller = new MoviesSupportController(
            new FakeMovieSupportService
            {
                Response = new RequestTicketEmailResendResponseDto
                {
                    Message = "Ticket email resent successfully."
                }
            },
            new RequestTicketEmailResendRequestDtoValidator());

        var result = await controller.RequestTicketEmailResend(new RequestTicketEmailResendRequestDto
        {
            Email = "guest@example.com",
            BookingCode = "BOOK-2026-001"
        });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<RequestTicketEmailResendResponseDto>(ok.Value);
        Assert.Equal("Ticket email resent successfully.", payload.Message);
    }

    [Fact]
    public async Task RequestTicketEmailResend_should_return_bad_request_when_service_throws()
    {
        var controller = new MoviesSupportController(
            new FakeMovieSupportService
            {
                Exception = new InvalidOperationException("Email does not match this booking.")
            },
            new RequestTicketEmailResendRequestDtoValidator());

        var result = await controller.RequestTicketEmailResend(new RequestTicketEmailResendRequestDto
        {
            Email = "guest@example.com",
            BookingCode = "BOOK-2026-001"
        });

        var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
        var problem = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal("Unable to resend ticket email.", problem.Title);
    }

    private sealed class FakeMovieSupportService : IMovieSupportService
    {
        public RequestTicketEmailResendResponseDto? Response { get; set; }
        public Exception? Exception { get; set; }

        public Task<RequestTicketEmailResendResponseDto> RequestTicketEmailResendAsync(RequestTicketEmailResendRequestDto request, CancellationToken cancellationToken = default)
        {
            if (Exception is not null)
            {
                throw Exception;
            }

            return Task.FromResult(Response ?? new RequestTicketEmailResendResponseDto
            {
                Message = "Ticket email resent successfully."
            });
        }
    }
}
```

- [ ] **Step 4: Run the backend tests to verify they fail**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter "RequestTicketEmailResendRequestDtoValidatorTests|MovieSupportServiceTests|MoviesSupportControllerTests"`

Expected: FAIL because the support DTOs, validator, service, gateway, and controller do not exist yet.

- [ ] **Step 5: Commit the red backend tests**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/RequestTicketEmailResendRequestDtoValidatorTests.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieSupportServiceTests.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesSupportControllerTests.cs
git commit -m "test: cover movies support resend flow"
```

### Task 4: Implement the backend support resend endpoint

**Files:**
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Support/RequestTicketEmailResendRequestDto.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Support/RequestTicketEmailResendResponseDto.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Support/IMovieSupportService.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Support/ITicketEmailResendGateway.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Support/MovieSupportService.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Support/Validators/RequestTicketEmailResendRequestDtoValidator.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DependencyInjection.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Tickets/TicketEmailResendGateway.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/DependencyInjection.cs`
- Create: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesSupportController.cs`

- [ ] **Step 1: Add the DTOs, gateway interface, validator, and service**

Create `RequestTicketEmailResendRequestDto.cs`:

```csharp
namespace ABCDMall.Modules.Movies.Application.DTOs.Support;

public sealed class RequestTicketEmailResendRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string BookingCode { get; set; } = string.Empty;
}
```

Create `RequestTicketEmailResendResponseDto.cs`:

```csharp
namespace ABCDMall.Modules.Movies.Application.DTOs.Support;

public sealed class RequestTicketEmailResendResponseDto
{
    public string Message { get; set; } = string.Empty;
}
```

Create `ITicketEmailResendGateway.cs`:

```csharp
namespace ABCDMall.Modules.Movies.Application.Services.Support;

public interface ITicketEmailResendGateway
{
    Task SendAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
```

Create `RequestTicketEmailResendRequestDtoValidator.cs`:

```csharp
using ABCDMall.Modules.Movies.Application.DTOs.Support;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Support.Validators;

public sealed class RequestTicketEmailResendRequestDtoValidator : AbstractValidator<RequestTicketEmailResendRequestDto>
{
    public RequestTicketEmailResendRequestDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.BookingCode)
            .NotEmpty()
            .MaximumLength(64);
    }
}
```

Create `IMovieSupportService.cs` and `MovieSupportService.cs`:

```csharp
using ABCDMall.Modules.Movies.Application.DTOs.Support;

namespace ABCDMall.Modules.Movies.Application.Services.Support;

public interface IMovieSupportService
{
    Task<RequestTicketEmailResendResponseDto> RequestTicketEmailResendAsync(
        RequestTicketEmailResendRequestDto request,
        CancellationToken cancellationToken = default);
}
```

```csharp
using ABCDMall.Modules.Movies.Application.DTOs.Support;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.Services.Support;

public sealed class MovieSupportService : IMovieSupportService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly ITicketEmailResendGateway _ticketEmailResendGateway;

    public MovieSupportService(
        IBookingRepository bookingRepository,
        ITicketEmailResendGateway ticketEmailResendGateway)
    {
        _bookingRepository = bookingRepository;
        _ticketEmailResendGateway = ticketEmailResendGateway;
    }

    public async Task<RequestTicketEmailResendResponseDto> RequestTicketEmailResendAsync(
        RequestTicketEmailResendRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var normalizedBookingCode = request.BookingCode.Trim();
        var normalizedEmail = request.Email.Trim();

        var booking = await _bookingRepository.GetByCodeAsync(normalizedBookingCode, cancellationToken);
        if (booking is null)
        {
            throw new InvalidOperationException("Booking code was not found.");
        }

        if (!string.Equals(booking.CustomerEmail.Trim(), normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Email does not match this booking.");
        }

        if (booking.Status != BookingStatus.Confirmed)
        {
            throw new InvalidOperationException("Ticket email can only be resent for confirmed bookings.");
        }

        await _ticketEmailResendGateway.SendAsync(booking.Id, cancellationToken);

        return new RequestTicketEmailResendResponseDto
        {
            Message = "Ticket email resent successfully."
        };
    }
}
```

- [ ] **Step 2: Register the application support service and validator**

Update `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DependencyInjection.cs`:

```csharp
using ABCDMall.Modules.Movies.Application.DTOs.Support;
using ABCDMall.Modules.Movies.Application.Services.Support;
using ABCDMall.Modules.Movies.Application.Services.Support.Validators;
```

```csharp
services.AddScoped<IValidator<RequestTicketEmailResendRequestDto>, RequestTicketEmailResendRequestDtoValidator>();
services.AddScoped<IMovieSupportService, MovieSupportService>();
```

- [ ] **Step 3: Add the infrastructure gateway and controller**

Create `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Tickets/TicketEmailResendGateway.cs`:

```csharp
using ABCDMall.Modules.Movies.Application.Services.Support;

namespace ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;

public sealed class TicketEmailResendGateway : ITicketEmailResendGateway
{
    private readonly ITicketEmailDispatcher _ticketEmailDispatcher;

    public TicketEmailResendGateway(ITicketEmailDispatcher ticketEmailDispatcher)
    {
        _ticketEmailDispatcher = ticketEmailDispatcher;
    }

    public Task SendAsync(Guid bookingId, CancellationToken cancellationToken = default)
        => _ticketEmailDispatcher.SendTicketEmailAsync(bookingId, cancellationToken);
}
```

Register it in `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/DependencyInjection.cs`:

```csharp
using ABCDMall.Modules.Movies.Application.Services.Support;
```

```csharp
services.AddScoped<ITicketEmailResendGateway, TicketEmailResendGateway>();
```

Create `BACKEND/ABCDMall.WebAPI/Controllers/MoviesSupportController.cs`:

```csharp
using ABCDMall.Modules.Movies.Application.DTOs.Support;
using ABCDMall.Modules.Movies.Application.Services.Support;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Route("api/movies/support")]
public sealed class MoviesSupportController : ControllerBase
{
    private readonly IMovieSupportService _movieSupportService;
    private readonly IValidator<RequestTicketEmailResendRequestDto> _requestValidator;

    public MoviesSupportController(
        IMovieSupportService movieSupportService,
        IValidator<RequestTicketEmailResendRequestDto> requestValidator)
    {
        _movieSupportService = movieSupportService;
        _requestValidator = requestValidator;
    }

    [HttpPost("resend-ticket")]
    public async Task<ActionResult<RequestTicketEmailResendResponseDto>> RequestTicketEmailResend(
        [FromBody] RequestTicketEmailResendRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _requestValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(new ValidationProblemDetails(
                validationResult.Errors
                    .GroupBy(x => x.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(x => x.ErrorMessage).ToArray())));
        }

        try
        {
            var response = await _movieSupportService.RequestTicketEmailResendAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to resend ticket email.",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
```

- [ ] **Step 4: Run the backend tests to verify they pass**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter "RequestTicketEmailResendRequestDtoValidatorTests|MovieSupportServiceTests|MoviesSupportControllerTests"`

Expected: PASS for the new validator, service, and controller tests.

- [ ] **Step 5: Commit the backend implementation**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Support/RequestTicketEmailResendRequestDto.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Support/RequestTicketEmailResendResponseDto.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Support/IMovieSupportService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Support/ITicketEmailResendGateway.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Support/MovieSupportService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Support/Validators/RequestTicketEmailResendRequestDtoValidator.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DependencyInjection.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Tickets/TicketEmailResendGateway.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/DependencyInjection.cs BACKEND/ABCDMall.WebAPI/Controllers/MoviesSupportController.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/RequestTicketEmailResendRequestDtoValidatorTests.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieSupportServiceTests.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesSupportControllerTests.cs
git commit -m "feat: add public movies ticket resend support"
```

### Task 5: Run the full verification set

**Files:**
- Test: `FRONTEND/src/features/movies/pages/__tests__/MovieHomePage.test.tsx`
- Test: `FRONTEND/src/features/movies/pages/__tests__/MovieSupportPage.test.tsx`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/RequestTicketEmailResendRequestDtoValidatorTests.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieSupportServiceTests.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesSupportControllerTests.cs`

- [ ] **Step 1: Run the focused frontend support tests**

Run: `npm --prefix FRONTEND run test -- "MovieHomePage|MovieSupportPage"`

Expected: PASS for the support navigation and support-page behavior tests.

- [ ] **Step 2: Run the frontend build**

Run: `npm --prefix FRONTEND run build`

Expected: PASS with a successful TypeScript compile and Vite build.

- [ ] **Step 3: Run the focused backend support tests**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter "RequestTicketEmailResendRequestDtoValidatorTests|MovieSupportServiceTests|MoviesSupportControllerTests"`

Expected: PASS for all public support resend tests.

- [ ] **Step 4: Run the full movies backend test project**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj`

Expected: PASS with the new support tests included in the existing movies test suite.

- [ ] **Step 5: Inspect git status**

Run: `git status --short`

Expected:
- only the intended frontend/backend support files and the plan file are changed
- unrelated work remains untouched

- [ ] **Step 6: Commit the implementation plan if it is intentionally tracked**

```bash
git add docs/superpowers/plans/2026-04-28-movies-support-page-implementation.md
git commit -m "docs: add movies support page implementation plan"
```

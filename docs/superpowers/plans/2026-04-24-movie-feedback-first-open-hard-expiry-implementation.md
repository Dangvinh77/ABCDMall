# Movie Feedback First-Open Hard Expiry Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Update movie feedback link expiry so any link expires 7 days after the first open, even when 1 or 2 feedback submissions already exist, while preserving the 3-submission closure rule.

**Architecture:** Keep the existing feedback lifecycle model and public token flow intact. Change the expiry rule in `MovieFeedbackService` to depend only on `FirstOpenedAtUtc`, then align public-page copy and tests with the new domain behavior.

**Tech Stack:** ASP.NET Core application services, xUnit, React, Vitest, Testing Library

---

## File Structure

- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/MovieFeedbackService.cs`
  - Replace the old “opened without submission” expiry rule with a first-open hard expiry rule.
  - Update backend messages so they no longer imply zero submissions.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs`
  - Add failing tests for 2-submission expiry and 2-submission still-valid scenarios.
  - Update wording assertions.
- Modify: `FRONTEND/src/features/movies/pages/MoviePublicFeedbackPage.tsx`
  - Update the expired notice text shown for `OpenedNoSubmission7Days`.
- Modify: `FRONTEND/src/features/movies/pages/__tests__/MoviePublicFeedbackPage.test.tsx`
  - Assert the new wording on the public page.

### Task 1: Lock the new backend expiry rule with tests first

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs`

- [ ] **Step 1: Add failing test cases for first-open hard expiry**

Append these tests to `MovieFeedbackServiceTests.cs`:

```csharp
[Fact]
public async Task GetPublicRequestAsync_should_expire_opened_request_after_seven_days_even_when_two_feedbacks_exist()
{
    var request = BuildSentRequest(DateTime.UtcNow.AddDays(-8), feedbackCount: 2);
    var repository = new FakeMovieFeedbackRepository(request);
    var service = CreateService(repository);

    var result = await service.GetPublicRequestAsync(PlainToken, CancellationToken.None);

    Assert.False(result.CanSubmit);
    Assert.Equal("Expired", result.Status);
    Assert.Equal("OpenedNoSubmission7Days", result.ExpiredReason);
    Assert.Equal("Feedback link expired 7 days after the first open.", result.Message);
}

[Fact]
public async Task GetPublicRequestAsync_should_keep_request_open_within_seven_days_even_when_two_feedbacks_exist()
{
    var request = BuildSentRequest(DateTime.UtcNow.AddDays(-6), feedbackCount: 2);
    var repository = new FakeMovieFeedbackRepository(request);
    var service = CreateService(repository);

    var result = await service.GetPublicRequestAsync(PlainToken, CancellationToken.None);

    Assert.True(result.CanSubmit);
    Assert.Equal("Sent", result.Status);
    Assert.Equal(1, result.RemainingSubmissions);
}
```

Also update the existing wording assertion in:

```csharp
GetPublicRequestAsync_should_expire_request_after_force_expire_opened_mutation
```

from:

```csharp
Assert.Equal("Feedback link expired after 7 days without a submission.", result.Message);
```

to:

```csharp
Assert.Equal("Feedback link expired 7 days after the first open.", result.Message);
```

- [ ] **Step 2: Run the focused backend test to verify it fails**

Run:

```bash
dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MovieFeedbackServiceTests
```

Expected:

- FAIL because `ShouldExpireBecauseOpenedWithoutSubmission(...)` still requires `Feedbacks.Count == 0`
- the new message assertion should also fail until the service wording is updated

- [ ] **Step 3: Commit the red test**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs
git commit -m "test: cover movie feedback first-open hard expiry"
```

### Task 2: Implement the backend rule and wording change

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/MovieFeedbackService.cs`

- [ ] **Step 1: Rename the expiry helper to reflect the new rule**

Replace:

```csharp
private static bool ShouldExpireBecauseOpenedWithoutSubmission(MovieFeedbackRequest request, DateTime utcNow)
{
    return request.Status == MovieFeedbackRequestStatus.Sent
        && request.FirstOpenedAtUtc.HasValue
        && request.Feedbacks.Count == 0
        && request.InvalidatedAtUtc is null
        && request.FirstOpenedAtUtc.Value.Add(OpenWithoutSubmissionExpiry) <= utcNow;
}
```

with:

```csharp
private static bool ShouldExpireBecauseFirstOpenWindowElapsed(MovieFeedbackRequest request, DateTime utcNow)
{
    return request.Status == MovieFeedbackRequestStatus.Sent
        && request.FirstOpenedAtUtc.HasValue
        && request.InvalidatedAtUtc is null
        && request.FirstOpenedAtUtc.Value.Add(OpenWithoutSubmissionExpiry) <= utcNow;
}
```

- [ ] **Step 2: Replace all call sites with the new helper**

Update `GetPublicRequestAsync`, `SubmitByTokenAsync`, `CanSubmit`, and `GetRequestMessage` so they use:

```csharp
ShouldExpireBecauseFirstOpenWindowElapsed(request, utcNow)
```

instead of:

```csharp
ShouldExpireBecauseOpenedWithoutSubmission(request, utcNow)
```

The key replacements are:

```csharp
if (ShouldExpireBecauseFirstOpenWindowElapsed(request, now))
```

```csharp
if (ShouldExpireBecauseFirstOpenWindowElapsed(feedbackRequest, now))
```

```csharp
&& !ShouldExpireBecauseFirstOpenWindowElapsed(request, utcNow)
```

```csharp
if (request.ExpiredReason == MovieFeedbackRequestExpiredReason.OpenedNoSubmission7Days
    || ShouldExpireBecauseFirstOpenWindowElapsed(request, utcNow))
```

- [ ] **Step 3: Update the backend expiry message**

Replace:

```csharp
return "Feedback link expired after 7 days without a submission.";
```

with:

```csharp
return "Feedback link expired 7 days after the first open.";
```

- [ ] **Step 4: Run the focused backend test to verify it passes**

Run:

```bash
dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MovieFeedbackServiceTests
```

Expected:

- PASS for the updated `MovieFeedbackServiceTests`

- [ ] **Step 5: Commit the backend implementation**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/MovieFeedbackService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs
git commit -m "feat: enforce hard expiry from movie feedback first open"
```

### Task 3: Update the frontend notice and regression tests

**Files:**
- Modify: `FRONTEND/src/features/movies/pages/MoviePublicFeedbackPage.tsx`
- Modify: `FRONTEND/src/features/movies/pages/__tests__/MoviePublicFeedbackPage.test.tsx`

- [ ] **Step 1: Write the failing frontend wording expectation**

In `MoviePublicFeedbackPage.test.tsx`, replace the expired notice assertion:

```tsx
expect(await screen.findByText(/7 ngày/i)).toBeInTheDocument();
```

with a stricter assertion:

```tsx
expect(
  await screen.findByText(/Link feedback đã hết hạn sau 7 ngày kể từ lần mở đầu tiên\./i),
).toBeInTheDocument();
```

- [ ] **Step 2: Run the focused frontend test to verify it fails**

Run:

```bash
npm --prefix FRONTEND run test -- MoviePublicFeedbackPage
```

Expected:

- FAIL because the page still says the link expired due to not submitting within 7 days

- [ ] **Step 3: Update the frontend notice copy**

In `MoviePublicFeedbackPage.tsx`, replace:

```tsx
if (request.expiredReason === 'OpenedNoSubmission7Days') {
  return 'Link feedback đã hết hạn vì không gửi phản hồi trong 7 ngày kể từ lần mở đầu tiên.';
}
```

with:

```tsx
if (request.expiredReason === 'OpenedNoSubmission7Days') {
  return 'Link feedback đã hết hạn sau 7 ngày kể từ lần mở đầu tiên.';
}
```

- [ ] **Step 4: Run the focused frontend test to verify it passes**

Run:

```bash
npm --prefix FRONTEND run test -- MoviePublicFeedbackPage
```

Expected:

- PASS for the public feedback page tests

- [ ] **Step 5: Commit the frontend wording change**

```bash
git add FRONTEND/src/features/movies/pages/MoviePublicFeedbackPage.tsx FRONTEND/src/features/movies/pages/__tests__/MoviePublicFeedbackPage.test.tsx
git commit -m "feat: update movie feedback expiry wording"
```

### Task 4: Run cross-layer verification and preserve unrelated changes

**Files:**
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs`
- Test: `FRONTEND/src/features/movies/pages/__tests__/MoviePublicFeedbackPage.test.tsx`

- [ ] **Step 1: Run the focused cross-layer regression set**

Run:

```bash
dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter "MovieFeedbackServiceTests|MoviesAdminServiceForceExpireOpenedFeedbackRequestTests"
```

Expected:

- PASS for backend lifecycle tests
- PASS for the admin force-expire test because the endpoint semantics still remain valid

Run:

```bash
npm --prefix FRONTEND run test -- src/features/movies/pages/__tests__/MoviePublicFeedbackPage.test.tsx
```

Expected:

- PASS for frontend notice rendering

- [ ] **Step 2: Run the frontend build**

Run:

```bash
npm --prefix FRONTEND run build
```

Expected:

- PASS with a successful Vite build

- [ ] **Step 3: Inspect git status**

Run:

```bash
git status --short
```

Expected:

- only the intended backend/frontend files and optional plan file are staged or modified
- unrelated pre-existing local changes must remain untouched

- [ ] **Step 4: Commit the plan if intentionally tracked**

```bash
git add docs/superpowers/plans/2026-04-24-movie-feedback-first-open-hard-expiry-implementation.md
git commit -m "docs: add movie feedback first-open hard expiry plan"
```

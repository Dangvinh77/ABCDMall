# Movie Feedback Link Lifecycle Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make movie feedback submit only through the dedicated token page, send feedback emails only after the showtime ends, allow at most 3 submissions per link, expire the link immediately after the third submission, and expire opened-but-never-submitted links 7 days after first open.

**Architecture:** Extend `MovieFeedbackRequest` into the authoritative lifecycle record for send timing, first-open tracking, and terminal expiry reasons. Keep token validation in the application service, preserve the repository-side transactional submission cap, and add a background service that sends feedback invitations after showtime completion. On the frontend, keep the form only on `/movies/feedback/:token` and render explicit terminal states from structured API metadata instead of generic error strings.

**Tech Stack:** ASP.NET Core Web API, C# application and infrastructure layers, Entity Framework Core migrations, React 19 + TypeScript + React Router, Vitest + React Testing Library, xUnit test project for Movies backend.

---

## File Structure

### Backend files to modify

- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Domain/Entities/MovieFeedbackRequest.cs`
  - Add first-open and expiry-reason lifecycle fields.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Feedbacks/PublicMovieFeedbackRequestResponseDto.cs`
  - Expose structured lifecycle data to the dedicated frontend page.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/IMovieFeedbackService.cs`
  - Keep the public request and token submit contract aligned with the richer response shape.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/MovieFeedbackService.cs`
  - Track first-open timestamps, compute terminal state, and enforce opened-no-submission expiry.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/IMovieFeedbackRepository.cs`
  - Add repository methods needed for lifecycle updates and pending-email scans.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Feedbacks/MovieFeedbackRepository.cs`
  - Persist first-open changes, send metadata, expiry transitions, and transactional third-submit invalidation.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Persistence/Booking/Configurations/MovieFeedbackRequestConfiguration.cs`
  - Configure new columns and indexes.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Persistence/Booking/MoviesBookingDbContext.cs`
  - Keep the entity model in sync with the migration.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Persistence/Booking/Migrations/MoviesBookingDbContextModelSnapshot.cs`
  - Accept the generated lifecycle schema changes.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/DependencyInjection.cs`
  - Register the new background service and any new email sender abstraction.
- `BACKEND/ABCDMall.WebAPI/Controllers/MovieFeedbackController.cs`
  - Return richer request state without collapsing all failures to the same user-facing meaning.

### Backend files to create

- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Domain/Enums/MovieFeedbackRequestExpiredReason.cs`
  - Define explicit expiry reasons.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/BackgroundServices/MovieFeedbackInvitationBackgroundService.cs`
  - Send pending feedback emails after showtime end.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Emails/IMovieFeedbackEmailSender.cs`
  - Send feedback invitation emails with token links.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Emails/MovieFeedbackEmailMessage.cs`
  - Carry email template data.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Emails/SmtpMovieFeedbackEmailSender.cs`
  - Reuse SMTP settings for feedback invitation delivery.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs`
  - Cover first-open tracking, 7-day expiry, and third-submit invalidation.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackInvitationBackgroundServiceTests.cs`
  - Cover post-showtime email dispatch and retry behavior.

### Frontend files to modify

- `FRONTEND/src/features/movies/api/moviesApi.ts`
  - Extend the public feedback request model with structured lifecycle fields.
- `FRONTEND/src/features/movies/pages/MoviePublicFeedbackPage.tsx`
  - Render state-specific UI from structured API values.
- `FRONTEND/src/features/movies/routes/MovieRoutes.tsx`
  - Preserve the token route as the only feedback form surface.

### Frontend files to create

- `FRONTEND/src/features/movies/pages/__tests__/MoviePublicFeedbackPage.test.tsx`
  - Cover invalid, waiting, expired, and third-submit terminal states.

### Verification commands

- Backend targeted tests:
  - `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MovieFeedback`
- Backend migration smoke check:
  - `dotnet ef migrations add AddMovieFeedbackLinkLifecycle --project BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/ABCDMall.Modules.Movies.Infrastructure.csproj --startup-project BACKEND/ABCDMall.WebAPI/ABCDMall.WebAPI.csproj --context MoviesBookingDbContext`
- Frontend targeted tests:
  - `npm --prefix FRONTEND run test -- MoviePublicFeedbackPage`
- Frontend build:
  - `npm --prefix FRONTEND run build`

## Task 1: Extend Feedback Request Domain and Persistence

**Files:**
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Domain/Enums/MovieFeedbackRequestExpiredReason.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Domain/Entities/MovieFeedbackRequest.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Persistence/Booking/Configurations/MovieFeedbackRequestConfiguration.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Persistence/Booking/Migrations/MoviesBookingDbContextModelSnapshot.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs`

- [ ] **Step 1: Write the failing entity-shape test**

```csharp
using ABCDMall.Modules.Movies.Domain.Entities;

public sealed class MovieFeedbackRequestLifecycleShapeTests
{
    [Fact]
    public void Should_start_without_open_tracking()
    {
        var request = new MovieFeedbackRequest();

        Assert.Null(request.FirstOpenedAtUtc);
        Assert.Null(request.LastOpenedAtUtc);
        Assert.Null(request.ExpiredReason);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MovieFeedbackRequestLifecycleShapeTests`

Expected: FAIL because `MovieFeedbackRequest` does not yet define the lifecycle fields.

- [ ] **Step 3: Add the new lifecycle enum and entity properties**

```csharp
namespace ABCDMall.Modules.Movies.Domain.Enums;

public enum MovieFeedbackRequestExpiredReason
{
    None = 0,
    OpenedNoSubmission7Days = 1,
    SubmissionLimitReached = 2,
    Cancelled = 3
}
```

```csharp
public class MovieFeedbackRequest
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid MovieId { get; set; }
    public Guid ShowtimeId { get; set; }
    public string PurchaserEmail { get; set; } = string.Empty;
    public string? TokenHash { get; set; }
    public MovieFeedbackRequestStatus Status { get; set; } = MovieFeedbackRequestStatus.Pending;
    public DateTime AvailableAtUtc { get; set; }
    public DateTime? SentAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? FirstOpenedAtUtc { get; set; }
    public DateTime? LastOpenedAtUtc { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public DateTime? InvalidatedAtUtc { get; set; }
    public MovieFeedbackRequestExpiredReason? ExpiredReason { get; set; }
    public int EmailRetryCount { get; set; }
    public string? LastEmailError { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
```

- [ ] **Step 4: Update EF configuration and create the migration**

```csharp
builder.Property(x => x.ExpiredReason)
    .HasConversion<int?>();

builder.HasIndex(x => new { x.Status, x.FirstOpenedAtUtc });
builder.HasIndex(x => x.LastOpenedAtUtc);
```

Run:

```bash
dotnet ef migrations add AddMovieFeedbackLinkLifecycle --project BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/ABCDMall.Modules.Movies.Infrastructure.csproj --startup-project BACKEND/ABCDMall.WebAPI/ABCDMall.WebAPI.csproj --context MoviesBookingDbContext
```

Expected: EF generates a new `AddMovieFeedbackLinkLifecycle` migration and updates `MoviesBookingDbContextModelSnapshot.cs` with `FirstOpenedAtUtc`, `LastOpenedAtUtc`, and `ExpiredReason` on `movies.MovieFeedbackRequests`.

- [ ] **Step 5: Run test and migration verification**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MovieFeedbackRequestLifecycleShapeTests`

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Domain/Enums/MovieFeedbackRequestExpiredReason.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Domain/Entities/MovieFeedbackRequest.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Persistence/Booking/Configurations/MovieFeedbackRequestConfiguration.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Persistence/Booking/Migrations
git commit -m "feat: add movie feedback request lifecycle fields"
```

## Task 2: Implement First-Open Tracking and 7-Day Expiry in Feedback Service

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Feedbacks/PublicMovieFeedbackRequestResponseDto.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/IMovieFeedbackRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/MovieFeedbackService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Feedbacks/MovieFeedbackRepository.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs`

- [ ] **Step 1: Write failing service tests for first open and 7-day expiry**

```csharp
[Fact]
public async Task Get_public_request_should_record_first_open_timestamp()
{
    var request = BuildSentRequest(firstOpenedAtUtc: null, feedbackCount: 0);
    var service = CreateService(request);

    var result = await service.GetPublicRequestAsync("plain-token");

    Assert.True(result.CanSubmit);
    Assert.NotNull(result.FirstOpenedAtUtc);
    Assert.Equal(3, result.RemainingSubmissions);
}

[Fact]
public async Task Get_public_request_should_expire_opened_request_after_seven_days_without_submission()
{
    var request = BuildSentRequest(firstOpenedAtUtc: DateTime.UtcNow.AddDays(-8), feedbackCount: 0);
    var service = CreateService(request);

    var result = await service.GetPublicRequestAsync("plain-token");

    Assert.False(result.CanSubmit);
    Assert.Equal("Expired", result.Status);
    Assert.Equal("OpenedNoSubmission7Days", result.ExpiredReason);
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter Get_public_request_should`

Expected: FAIL because the response DTO lacks `FirstOpenedAtUtc`, `RemainingSubmissions`, and `ExpiredReason`, and the service does not mutate first-open lifecycle state.

- [ ] **Step 3: Extend the public response DTO**

```csharp
public sealed class PublicMovieFeedbackRequestResponseDto
{
    public Guid FeedbackRequestId { get; set; }
    public Guid MovieId { get; set; }
    public Guid ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public DateTime AvailableAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public DateTime? FirstOpenedAtUtc { get; set; }
    public int RemainingSubmissions { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ExpiredReason { get; set; }
    public bool CanSubmit { get; set; }
    public string? Message { get; set; }
}
```

- [ ] **Step 4: Add repository lifecycle helpers and implement service logic**

```csharp
public interface IMovieFeedbackRepository
{
    Task<MovieFeedbackRequest?> GetRequestByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);
    Task<MovieFeedbackRequest> MarkOpenedAsync(Guid requestId, DateTime utcNow, CancellationToken cancellationToken = default);
    Task<MovieFeedbackRequest> MarkExpiredAsync(Guid requestId, MovieFeedbackRequestExpiredReason reason, DateTime utcNow, CancellationToken cancellationToken = default);
}
```

```csharp
private const int MaxSubmissionsPerRequest = 3;
private static readonly TimeSpan OpenWithoutSubmissionExpiry = TimeSpan.FromDays(7);

public async Task<PublicMovieFeedbackRequestResponseDto> GetPublicRequestAsync(string token, CancellationToken cancellationToken = default)
{
    var request = await GetRequestFromTokenAsync(token, cancellationToken);
    var now = DateTime.UtcNow;

    if (ShouldExpireBecauseOpenedWithoutSubmission(request, now))
    {
        request = await _feedbackRepository.MarkExpiredAsync(
            request.Id,
            MovieFeedbackRequestExpiredReason.OpenedNoSubmission7Days,
            now,
            cancellationToken);
    }
    else if (request.Status == MovieFeedbackRequestStatus.Sent)
    {
        request = await _feedbackRepository.MarkOpenedAsync(request.Id, now, cancellationToken);
    }

    return BuildPublicResponse(request, now);
}
```

- [ ] **Step 5: Keep submit flow aligned with the new terminal logic**

```csharp
public async Task<MovieFeedbackResponseDto> SubmitByTokenAsync(
    string token,
    SubmitMovieFeedbackByTokenRequestDto request,
    CancellationToken cancellationToken = default)
{
    var feedbackRequest = await GetRequestFromTokenAsync(token, cancellationToken);
    var now = DateTime.UtcNow;

    if (ShouldExpireBecauseOpenedWithoutSubmission(feedbackRequest, now))
    {
        feedbackRequest = await _feedbackRepository.MarkExpiredAsync(
            feedbackRequest.Id,
            MovieFeedbackRequestExpiredReason.OpenedNoSubmission7Days,
            now,
            cancellationToken);
    }

    if (!CanSubmit(feedbackRequest, now))
    {
        throw new InvalidOperationException(GetRequestMessage(feedbackRequest, now) ?? "Feedback link is not available.");
    }

    // existing submission creation continues here
}
```

- [ ] **Step 6: Run the targeted feedback service tests**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MovieFeedbackServiceTests`

Expected: PASS

- [ ] **Step 7: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Feedbacks/PublicMovieFeedbackRequestResponseDto.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/IMovieFeedbackRepository.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/MovieFeedbackService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Feedbacks/MovieFeedbackRepository.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs
git commit -m "feat: enforce movie feedback first-open expiry"
```

## Task 3: Add Post-Showtime Feedback Invitation Email Dispatch

**Files:**
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/BackgroundServices/MovieFeedbackInvitationBackgroundService.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Emails/IMovieFeedbackEmailSender.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Emails/MovieFeedbackEmailMessage.cs`
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Emails/SmtpMovieFeedbackEmailSender.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/IMovieFeedbackRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Feedbacks/MovieFeedbackRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/DependencyInjection.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackInvitationBackgroundServiceTests.cs`

- [ ] **Step 1: Write the failing background-service tests**

```csharp
[Fact]
public async Task Should_send_feedback_email_only_after_showtime_end()
{
    var request = BuildPendingRequest();
    var repository = new FakeFeedbackInvitationRepository(request, showtimeEnded: true);
    var sender = new FakeMovieFeedbackEmailSender();
    var service = CreateBackgroundService(repository, sender);

    await service.ProcessPendingRequestsOnceAsync(CancellationToken.None);

    Assert.Single(sender.Messages);
    Assert.Equal(MovieFeedbackRequestStatus.Sent, request.Status);
}

[Fact]
public async Task Should_not_send_feedback_email_before_showtime_end()
{
    var request = BuildPendingRequest();
    var repository = new FakeFeedbackInvitationRepository(request, showtimeEnded: false);
    var sender = new FakeMovieFeedbackEmailSender();
    var service = CreateBackgroundService(repository, sender);

    await service.ProcessPendingRequestsOnceAsync(CancellationToken.None);

    Assert.Empty(sender.Messages);
    Assert.Equal(MovieFeedbackRequestStatus.Pending, request.Status);
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MovieFeedbackInvitationBackgroundServiceTests`

Expected: FAIL because the background service and sender abstractions do not yet exist.

- [ ] **Step 3: Add the sender abstraction and message model**

```csharp
public sealed class MovieFeedbackEmailMessage
{
    public required string ToEmail { get; init; }
    public required string ToName { get; init; }
    public required string Subject { get; init; }
    public required string HtmlBody { get; init; }
}

public interface IMovieFeedbackEmailSender
{
    Task SendAsync(MovieFeedbackEmailMessage message, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 4: Implement the background service and repository send helpers**

```csharp
public sealed class MovieFeedbackInvitationBackgroundService : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await ProcessPendingRequestsOnceAsync(stoppingToken);

        using var timer = new PeriodicTimer(PollInterval);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessPendingRequestsOnceAsync(stoppingToken);
        }
    }
}
```

```csharp
public async Task<IReadOnlyList<MovieFeedbackRequest>> GetPendingInvitationRequestsAsync(
    DateTime utcNow,
    int take,
    CancellationToken cancellationToken = default)
{
    return await _dbContext.MovieFeedbackRequests
        .Where(x => x.Status == MovieFeedbackRequestStatus.Pending)
        .Where(x => x.AvailableAtUtc <= utcNow)
        .OrderBy(x => x.AvailableAtUtc)
        .Take(take)
        .ToListAsync(cancellationToken);
}
```

- [ ] **Step 5: Register new infrastructure services**

```csharp
services.AddScoped<IMovieFeedbackEmailSender, SmtpMovieFeedbackEmailSender>();
services.AddHostedService<MovieFeedbackInvitationBackgroundService>();
```

- [ ] **Step 6: Run targeted tests**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MovieFeedbackInvitationBackgroundServiceTests`

Expected: PASS

- [ ] **Step 7: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/BackgroundServices/MovieFeedbackInvitationBackgroundService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Emails/IMovieFeedbackEmailSender.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Emails/MovieFeedbackEmailMessage.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Services/Emails/SmtpMovieFeedbackEmailSender.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/DependencyInjection.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackInvitationBackgroundServiceTests.cs
git commit -m "feat: send movie feedback emails after showtime end"
```

## Task 4: Preserve Third-Submit Invalidation and Expose Structured Status Through the API

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/MovieFeedbackService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Feedbacks/MovieFeedbackRepository.cs`
- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/MovieFeedbackController.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs`

- [ ] **Step 1: Write failing tests for third-submit invalidation and response metadata**

```csharp
[Fact]
public async Task Third_submission_should_close_the_link_immediately()
{
    var request = BuildSentRequest(firstOpenedAtUtc: DateTime.UtcNow.AddDays(-1), feedbackCount: 2);
    var service = CreateService(request);

    await service.SubmitByTokenAsync("plain-token", new SubmitMovieFeedbackByTokenRequestDto
    {
        Rating = 5,
        Comment = "Great movie"
    });

    var result = await service.GetPublicRequestAsync("plain-token");

    Assert.False(result.CanSubmit);
    Assert.Equal("Submitted", result.Status);
    Assert.Equal(0, result.RemainingSubmissions);
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter Third_submission_should`

Expected: FAIL until the response builder exposes `RemainingSubmissions` and the repository stores the submission-limit expiry reason consistently.

- [ ] **Step 3: Tighten repository-side third-submit invalidation**

```csharp
if (trackedRequest.Feedbacks.Count == 2)
{
    trackedRequest.Status = MovieFeedbackRequestStatus.Submitted;
    trackedRequest.SubmittedAtUtc = utcNow;
    trackedRequest.InvalidatedAtUtc = utcNow;
    trackedRequest.ExpiredReason = MovieFeedbackRequestExpiredReason.SubmissionLimitReached;
}
```

- [ ] **Step 4: Build a structured public response and keep controller output aligned**

```csharp
private PublicMovieFeedbackRequestResponseDto BuildPublicResponse(MovieFeedbackRequest request, DateTime utcNow)
{
    return new PublicMovieFeedbackRequestResponseDto
    {
        FeedbackRequestId = request.Id,
        MovieId = request.MovieId,
        ShowtimeId = request.ShowtimeId,
        MovieTitle = movie?.Title ?? string.Empty,
        AvailableAtUtc = request.AvailableAtUtc,
        ExpiresAtUtc = request.ExpiresAtUtc,
        FirstOpenedAtUtc = request.FirstOpenedAtUtc,
        RemainingSubmissions = Math.Max(0, MaxSubmissionsPerRequest - request.Feedbacks.Count),
        Status = request.Status.ToString(),
        ExpiredReason = request.ExpiredReason?.ToString(),
        CanSubmit = CanSubmit(request, utcNow),
        Message = GetRequestMessage(request, utcNow)
    };
}
```

```csharp
return BadRequest(new ProblemDetails
{
    Title = "Unable to submit feedback.",
    Detail = ex.Message,
    Status = StatusCodes.Status400BadRequest
});
```

The controller change is intentionally small: keep HTTP shape stable while letting the frontend consume the richer success payload instead of parsing generic strings.

- [ ] **Step 5: Run targeted feedback tests**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MovieFeedbackServiceTests`

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Feedbacks/MovieFeedbackService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Feedbacks/MovieFeedbackRepository.cs BACKEND/ABCDMall.WebAPI/Controllers/MovieFeedbackController.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs
git commit -m "feat: return structured movie feedback request status"
```

## Task 5: Update the Dedicated Frontend Page to Use Structured Lifecycle Status

**Files:**
- Modify: `FRONTEND/src/features/movies/api/moviesApi.ts`
- Modify: `FRONTEND/src/features/movies/pages/MoviePublicFeedbackPage.tsx`
- Modify: `FRONTEND/src/features/movies/routes/MovieRoutes.tsx`
- Create: `FRONTEND/src/features/movies/pages/__tests__/MoviePublicFeedbackPage.test.tsx`

- [ ] **Step 1: Write failing frontend tests for terminal states**

```tsx
it('renders an expired-after-open message without the form', async () => {
  mockFetchPublicMovieFeedbackRequest({
    canSubmit: false,
    status: 'Expired',
    expiredReason: 'OpenedNoSubmission7Days',
    remainingSubmissions: 3,
  })

  render(<MoviePublicFeedbackPage />)

  expect(await screen.findByText(/7 ngay/i)).toBeInTheDocument()
  expect(screen.queryByRole('button', { name: /gui feedback/i })).not.toBeInTheDocument()
})

it('renders the form only when the request is submittable', async () => {
  mockFetchPublicMovieFeedbackRequest({
    canSubmit: true,
    status: 'Sent',
    expiredReason: null,
    remainingSubmissions: 2,
  })

  render(<MoviePublicFeedbackPage />)

  expect(await screen.findByRole('button', { name: /gui feedback/i })).toBeInTheDocument()
})
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `npm --prefix FRONTEND run test -- MoviePublicFeedbackPage`

Expected: FAIL because the page still normalizes most terminal cases into the same "already submitted" message and no dedicated test file exists yet.

- [ ] **Step 3: Extend the frontend API model**

```ts
export interface PublicMovieFeedbackRequestModel {
  feedbackRequestId: string;
  movieId: string;
  showtimeId: string;
  movieTitle: string;
  availableAtUtc: string;
  expiresAtUtc?: string | null;
  firstOpenedAtUtc?: string | null;
  status: string;
  expiredReason?: string | null;
  remainingSubmissions: number;
  canSubmit: boolean;
  message?: string | null;
}
```

- [ ] **Step 4: Replace generic terminal-message guessing in the page**

```tsx
function resolveFeedbackNotice(request: PublicMovieFeedbackRequestModel | null) {
  if (!request) return null;
  if (request.canSubmit) return null;
  if (request.status === 'Submitted') return 'Link feedback da du 3 lan gui.';
  if (request.expiredReason === 'OpenedNoSubmission7Days') {
    return 'Link feedback da het han vi khong gui trong 7 ngay ke tu lan mo dau tien.';
  }
  if (request.status === 'Pending') return 'Form feedback se mo sau khi suat chieu ket thuc.';
  return request.message ?? 'Link feedback khong con hop le.';
}
```

```tsx
{request?.canSubmit && (
  <form onSubmit={handleSubmit}>
    <p className="mt-1 text-sm text-gray-400">
      Con lai {request.remainingSubmissions} lan gui feedback tu link nay.
    </p>
  </form>
)}
```

- [ ] **Step 5: Keep the token route as the only form surface**

```tsx
<Route path="feedback/:token" element={<MoviePublicFeedbackPage />} />
```

This step is a regression guard: verify no additional route or page starts rendering the form.

- [ ] **Step 6: Run targeted frontend tests and build**

Run:

```bash
npm --prefix FRONTEND run test -- MoviePublicFeedbackPage
npm --prefix FRONTEND run build
```

Expected: PASS

- [ ] **Step 7: Commit**

```bash
git add FRONTEND/src/features/movies/api/moviesApi.ts FRONTEND/src/features/movies/pages/MoviePublicFeedbackPage.tsx FRONTEND/src/features/movies/routes/MovieRoutes.tsx FRONTEND/src/features/movies/pages/__tests__/MoviePublicFeedbackPage.test.tsx
git commit -m "feat: show explicit movie feedback link states"
```

## Task 6: Full Verification and Cleanup

**Files:**
- Modify: any touched files from Tasks 1-5 only if verification exposes defects
- Test: backend and frontend files from Tasks 1-5

- [ ] **Step 1: Run the backend feedback test slice**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MovieFeedback`

Expected: PASS

- [ ] **Step 2: Run the frontend feedback test slice**

Run: `npm --prefix FRONTEND run test -- MoviePublicFeedbackPage`

Expected: PASS

- [ ] **Step 3: Run a frontend production build**

Run: `npm --prefix FRONTEND run build`

Expected: PASS

- [ ] **Step 4: Review git diff for accidental scope creep**

Run: `git diff --stat --cached HEAD~1..HEAD`

Expected: only feedback lifecycle, feedback email delivery, DTO, route, and tests are included.

- [ ] **Step 5: Commit final verification fixes if needed**

```bash
git add BACKEND FRONTEND
git commit -m "test: verify movie feedback lifecycle flow"
```

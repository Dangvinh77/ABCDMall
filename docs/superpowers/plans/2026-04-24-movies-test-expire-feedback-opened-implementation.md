# Movies Test Expire Feedback Opened Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a test-only Movies admin API that moves a sent feedback request's opened timestamps beyond the 7-day window so the existing public feedback flow expires the link through normal business logic.

**Architecture:** Reuse the existing `MoviesAdminTestController` and Movies admin service/repository chain instead of creating a separate test service layer. The endpoint only mutates `MovieFeedbackRequest` lifecycle fields in booking storage; it does not set `Expired` directly, and the actual expire transition remains owned by `MovieFeedbackService`.

**Tech Stack:** ASP.NET Core controllers, existing Movies application/infrastructure layers, Entity Framework Core, xUnit

---

## File Structure

- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminTestController.cs`
  - Add the new test-only `expire-opened` endpoint alongside `finish-now`.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminOperationsDtos.cs`
  - Add the response DTO for forcing a feedback request into the opened-expired state.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminService.cs`
  - Add one admin test utility method for moving opened timestamps into the past.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/MoviesAdminService.cs`
  - Pass the new method through to the repository.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminRepository.cs`
  - Add the persistence contract for the feedback-request test mutation.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs`
  - Load the feedback request, move `FirstOpenedAtUtc` and `LastOpenedAtUtc` to `UtcNow.AddDays(-8)`, clear `ExpiredReason`, keep `Status = Sent`, and save.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceForceExpireOpenedFeedbackRequestTests.cs`
  - Cover not-found and successful mutation behavior against an in-memory booking context.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminTestControllerTests.cs`
  - Add controller tests for the new endpoint and environment blocking.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs`
  - Add a regression test confirming the public feedback service marks the request expired after the test mutation.

### Task 1: Add the feedback-request force-expire contract and repository mutation

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminOperationsDtos.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/MoviesAdminService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceForceExpireOpenedFeedbackRequestTests.cs`

- [ ] **Step 1: Write the failing service/repository tests**

```csharp
using ABCDMall.Modules.Movies.Application.Services.Admin;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using ABCDMall.Modules.Movies.Infrastructure.Repositories.Admin;
using ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class MoviesAdminServiceForceExpireOpenedFeedbackRequestTests
{
    [Fact]
    public async Task ForceExpireOpenedFeedbackRequestAsync_should_return_null_when_request_does_not_exist()
    {
        await using var catalogDbContext = CreateCatalogDbContext();
        await using var bookingDbContext = CreateBookingDbContext();
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);

        var result = await service.ForceExpireOpenedFeedbackRequestAsync(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

        Assert.Null(result);
    }

    [Fact]
    public async Task ForceExpireOpenedFeedbackRequestAsync_should_move_opened_timestamps_to_the_past_and_keep_sent_status()
    {
        await using var catalogDbContext = CreateCatalogDbContext();
        await using var bookingDbContext = CreateBookingDbContext();
        var request = await SeedFeedbackRequestAsync(bookingDbContext);
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);
        var originalUpdatedAtUtc = request.UpdatedAtUtc;

        var result = await service.ForceExpireOpenedFeedbackRequestAsync(request.Id);

        Assert.NotNull(result);
        Assert.Equal(request.Id, result!.RequestId);
        Assert.True(result.NewFirstOpenedAtUtc <= DateTime.UtcNow.AddDays(-7));
        Assert.True(result.NewLastOpenedAtUtc <= DateTime.UtcNow.AddDays(-7));
        Assert.Equal("Feedback request opened timestamps moved to the past for expiry testing.", result.Message);

        var reloaded = await bookingDbContext.MovieFeedbackRequests.FirstAsync(x => x.Id == request.Id);
        Assert.Equal(MovieFeedbackRequestStatus.Sent, reloaded.Status);
        Assert.Equal(0, reloaded.SubmittedCount);
        Assert.Null(reloaded.ExpiredReason);
        Assert.Null(reloaded.InvalidatedAtUtc);
        Assert.True(reloaded.UpdatedAtUtc >= originalUpdatedAtUtc);
    }

    private static MoviesCatalogDbContext CreateCatalogDbContext()
    {
        var options = new DbContextOptionsBuilder<MoviesCatalogDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoviesCatalogDbContext(options);
    }

    private static MoviesBookingDbContext CreateBookingDbContext()
    {
        var options = new DbContextOptionsBuilder<MoviesBookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoviesBookingDbContext(options);
    }

    private static async Task<MovieFeedbackRequest> SeedFeedbackRequestAsync(MoviesBookingDbContext dbContext)
    {
        var now = DateTime.UtcNow;
        var request = new MovieFeedbackRequest
        {
            Id = Guid.Parse("90000000-0000-0000-0000-000000000001"),
            BookingId = Guid.Parse("90000000-0000-0000-0000-000000000002"),
            MovieId = Guid.Parse("90000000-0000-0000-0000-000000000003"),
            ShowtimeId = Guid.Parse("90000000-0000-0000-0000-000000000004"),
            PurchaserEmail = "guest@example.com",
            TokenHash = "token-hash",
            Status = MovieFeedbackRequestStatus.Sent,
            AvailableAtUtc = now.AddHours(-2),
            SentAtUtc = now.AddHours(-1),
            ExpiresAtUtc = now.AddDays(30),
            FirstOpenedAtUtc = now.AddDays(-1),
            LastOpenedAtUtc = now.AddDays(-1),
            CreatedAtUtc = now.AddDays(-2),
            UpdatedAtUtc = now.AddDays(-2)
        };

        dbContext.MovieFeedbackRequests.Add(request);
        await dbContext.SaveChangesAsync();
        return request;
    }

    private sealed class NoOpTicketEmailDispatcher : ITicketEmailDispatcher
    {
        public Task SendTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminServiceForceExpireOpenedFeedbackRequestTests`

Expected: FAIL because `ForceExpireOpenedFeedbackRequestAsync` and its response DTO do not exist yet.

- [ ] **Step 3: Add the DTO and application contracts**

```csharp
public sealed class MoviesAdminForceExpireOpenedFeedbackRequestResponseDto
{
    public Guid RequestId { get; set; }
    public DateTime? PreviousFirstOpenedAtUtc { get; set; }
    public DateTime? PreviousLastOpenedAtUtc { get; set; }
    public DateTime NewFirstOpenedAtUtc { get; set; }
    public DateTime NewLastOpenedAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

```csharp
Task<MoviesAdminForceExpireOpenedFeedbackRequestResponseDto?> ForceExpireOpenedFeedbackRequestAsync(
    Guid requestId,
    CancellationToken cancellationToken = default);
```

```csharp
public Task<MoviesAdminForceExpireOpenedFeedbackRequestResponseDto?> ForceExpireOpenedFeedbackRequestAsync(
    Guid requestId,
    CancellationToken cancellationToken = default)
    => _repository.ForceExpireOpenedFeedbackRequestAsync(requestId, cancellationToken);
```

- [ ] **Step 4: Implement the repository mutation**

```csharp
public async Task<MoviesAdminForceExpireOpenedFeedbackRequestResponseDto?> ForceExpireOpenedFeedbackRequestAsync(
    Guid requestId,
    CancellationToken cancellationToken = default)
{
    var request = await _bookingDbContext.MovieFeedbackRequests
        .FirstOrDefaultAsync(x => x.Id == requestId, cancellationToken);

    if (request is null)
    {
        return null;
    }

    var utcNow = DateTime.UtcNow;
    var forcedOpenedAtUtc = utcNow.AddDays(-8);
    var previousFirstOpenedAtUtc = request.FirstOpenedAtUtc;
    var previousLastOpenedAtUtc = request.LastOpenedAtUtc;

    request.Status = MovieFeedbackRequestStatus.Sent;
    request.FirstOpenedAtUtc = forcedOpenedAtUtc;
    request.LastOpenedAtUtc = forcedOpenedAtUtc;
    request.SubmittedCount = 0;
    request.ExpiredReason = null;
    request.InvalidatedAtUtc = null;
    request.UpdatedAtUtc = utcNow;

    await _bookingDbContext.SaveChangesAsync(cancellationToken);

    return new MoviesAdminForceExpireOpenedFeedbackRequestResponseDto
    {
        RequestId = request.Id,
        PreviousFirstOpenedAtUtc = previousFirstOpenedAtUtc,
        PreviousLastOpenedAtUtc = previousLastOpenedAtUtc,
        NewFirstOpenedAtUtc = forcedOpenedAtUtc,
        NewLastOpenedAtUtc = forcedOpenedAtUtc,
        Message = "Feedback request opened timestamps moved to the past for expiry testing."
    };
}
```

- [ ] **Step 5: Run the focused test to verify it passes**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminServiceForceExpireOpenedFeedbackRequestTests`

Expected: PASS with `2 passed`.

- [ ] **Step 6: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminOperationsDtos.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/MoviesAdminService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminRepository.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceForceExpireOpenedFeedbackRequestTests.cs
git commit -m "feat: add movies feedback opened expiry test service"
```

### Task 2: Expose the test-only controller endpoint and controller tests

**Files:**
- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminTestController.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminTestControllerTests.cs`

- [ ] **Step 1: Write the failing controller tests**

```csharp
[Fact]
public async Task ForceExpireOpenedFeedbackRequest_should_return_not_found_outside_dev_or_test()
{
    var service = new FakeMoviesAdminService();
    var controller = new MoviesAdminTestController(service, new FakeHostEnvironment("Production"));

    var result = await controller.ForceExpireOpenedFeedbackRequest(Guid.NewGuid());

    Assert.IsType<NotFoundResult>(result.Result);
    Assert.Equal(0, service.ForceExpireOpenedFeedbackRequestCallCount);
}

[Fact]
public async Task ForceExpireOpenedFeedbackRequest_should_return_ok_when_environment_is_development()
{
    var response = new MoviesAdminForceExpireOpenedFeedbackRequestResponseDto
    {
        RequestId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
        PreviousFirstOpenedAtUtc = DateTime.UtcNow.AddDays(-1),
        PreviousLastOpenedAtUtc = DateTime.UtcNow.AddDays(-1),
        NewFirstOpenedAtUtc = DateTime.UtcNow.AddDays(-8),
        NewLastOpenedAtUtc = DateTime.UtcNow.AddDays(-8),
        Message = "Feedback request opened timestamps moved to the past for expiry testing."
    };

    var service = new FakeMoviesAdminService
    {
        ForceExpireOpenedFeedbackRequestResponse = response
    };
    var controller = new MoviesAdminTestController(service, new FakeHostEnvironment(Environments.Development));

    var result = await controller.ForceExpireOpenedFeedbackRequest(response.RequestId);

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var payload = Assert.IsType<MoviesAdminForceExpireOpenedFeedbackRequestResponseDto>(ok.Value);
    Assert.Equal(response.RequestId, payload.RequestId);
    Assert.Equal(1, service.ForceExpireOpenedFeedbackRequestCallCount);
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminTestControllerTests`

Expected: FAIL because the new controller action and fake-service members do not exist yet.

- [ ] **Step 3: Add the controller endpoint and update the fake service in tests**

```csharp
[HttpPost("feedback-requests/{requestId:guid}/expire-opened")]
public async Task<ActionResult<MoviesAdminForceExpireOpenedFeedbackRequestResponseDto>> ForceExpireOpenedFeedbackRequest(
    Guid requestId,
    CancellationToken cancellationToken = default)
{
    if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Test"))
    {
        return NotFound();
    }

    var result = await _moviesAdminService.ForceExpireOpenedFeedbackRequestAsync(requestId, cancellationToken);
    return result is null ? NotFound() : Ok(result);
}
```

```csharp
public int ForceExpireOpenedFeedbackRequestCallCount { get; private set; }
public MoviesAdminForceExpireOpenedFeedbackRequestResponseDto? ForceExpireOpenedFeedbackRequestResponse { get; set; }

public Task<MoviesAdminForceExpireOpenedFeedbackRequestResponseDto?> ForceExpireOpenedFeedbackRequestAsync(
    Guid requestId,
    CancellationToken cancellationToken = default)
{
    ForceExpireOpenedFeedbackRequestCallCount += 1;
    return Task.FromResult(ForceExpireOpenedFeedbackRequestResponse);
}
```

- [ ] **Step 4: Run the controller test to verify it passes**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminTestControllerTests`

Expected: PASS with the existing `finish-now` tests plus the new `expire-opened` tests all green.

- [ ] **Step 5: Commit**

```bash
git add BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminTestController.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminTestControllerTests.cs
git commit -m "feat: add test-only movies feedback expiry endpoint"
```

### Task 3: Verify the public feedback flow expires after the forced mutation

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs`

- [ ] **Step 1: Add the failing regression test**

```csharp
[Fact]
public async Task GetPublicRequestAsync_should_expire_request_after_force_expire_opened_mutation()
{
    var request = BuildSentRequest(DateTime.UtcNow.AddDays(-1), feedbackCount: 0);
    request.LastOpenedAtUtc = request.FirstOpenedAtUtc;

    var repository = new FakeMovieFeedbackRepository(request);
    request.FirstOpenedAtUtc = DateTime.UtcNow.AddDays(-8);
    request.LastOpenedAtUtc = DateTime.UtcNow.AddDays(-8);
    request.Status = MovieFeedbackRequestStatus.Sent;
    request.ExpiredReason = null;
    request.InvalidatedAtUtc = null;

    var service = CreateService(repository);

    var result = await service.GetPublicRequestAsync(PlainToken, CancellationToken.None);

    Assert.False(result.CanSubmit);
    Assert.Equal("Expired", result.Status);
    Assert.Equal("OpenedNoSubmission7Days", result.ExpiredReason);
    Assert.Equal("Feedback link expired after 7 days without a submission.", result.Message);
}
```

- [ ] **Step 2: Run the focused feedback-service test to verify it passes**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MovieFeedbackServiceTests`

Expected: PASS, confirming the service performs the real expire transition once the opened timestamps are moved into the past.

- [ ] **Step 3: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs
git commit -m "test: cover forced feedback opened expiry flow"
```

### Task 4: Run Movies verification and leave the branch clean

**Files:**
- Modify: none
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceForceExpireOpenedFeedbackRequestTests.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminTestControllerTests.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackServiceTests.cs`

- [ ] **Step 1: Run the focused regression set**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter "MoviesAdminServiceForceExpireOpenedFeedbackRequestTests|MoviesAdminTestControllerTests|MovieFeedbackServiceTests"`

Expected: PASS for the new service/controller tests and the public-feedback regression.

- [ ] **Step 2: Run the full Movies test suite**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj`

Expected: PASS for the full Movies backend suite with no regressions.

- [ ] **Step 3: Inspect git status**

Run: `git status --short`

Expected: clean working tree except for this implementation plan file if it has not been committed yet.

- [ ] **Step 4: Commit the implementation plan if needed**

```bash
git add docs/superpowers/plans/2026-04-24-movies-test-expire-feedback-opened-implementation.md
git commit -m "docs: add movies test expire feedback implementation plan"
```

Only do this if the plan file is intentionally kept under version control and is still uncommitted at the end. If it was already committed earlier, skip this step.

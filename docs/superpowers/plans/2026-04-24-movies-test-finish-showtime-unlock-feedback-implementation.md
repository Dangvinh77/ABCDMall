# Movies Test Finish Showtime Unlock Feedback Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Update the existing test-only `finish-now` Movies admin API so it also unlocks pending feedback requests for the same showtime, allowing the invitation background service to send mail on the next poll.

**Architecture:** Keep the route and controller unchanged, and extend the existing `ForceFinishShowtimeAsync` repository path to update both catalog showtime timing and booking feedback-request availability in one operation. The email background service remains unchanged; the endpoint simply mutates the exact data it already uses for eligibility.

**Tech Stack:** ASP.NET Core controllers, existing Movies application/infrastructure layers, Entity Framework Core, xUnit

---

## File Structure

- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs`
  - Extend `ForceFinishShowtimeAsync` to update `AvailableAtUtc` and `UpdatedAtUtc` for pending feedback requests on the same showtime.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceForceFinishShowtimeTests.cs`
  - Add assertions proving pending feedback requests are unlocked and unrelated/non-pending requests are left unchanged.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackInvitationBackgroundServiceTests.cs`
  - Add a regression test proving the background service can send once the request becomes selectable after the forced finish operation.

### Task 1: Extend force-finish showtime to unlock pending feedback requests

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceForceFinishShowtimeTests.cs`

- [ ] **Step 1: Write the failing repository/service tests**

```csharp
[Fact]
public async Task ForceFinishShowtimeAsync_should_move_pending_feedback_requests_for_the_same_showtime_to_the_past()
{
    await using var catalogDbContext = await CatalogSeedTestDb.CreateSeededContextAsync();
    await using var bookingDbContext = CreateBookingDbContext();
    var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
    var service = new MoviesAdminService(repository);
    var showtime = await catalogDbContext.Showtimes.OrderBy(x => x.StartAtUtc).FirstAsync();
    var sameShowtimePending = await SeedFeedbackRequestAsync(
        bookingDbContext,
        showtime.Id,
        MovieFeedbackRequestStatus.Pending,
        DateTime.UtcNow.AddHours(2));
    var sameShowtimeSent = await SeedFeedbackRequestAsync(
        bookingDbContext,
        showtime.Id,
        MovieFeedbackRequestStatus.Sent,
        DateTime.UtcNow.AddHours(2));
    var otherShowtimePending = await SeedFeedbackRequestAsync(
        bookingDbContext,
        Guid.Parse("77777777-7777-7777-7777-777777777777"),
        MovieFeedbackRequestStatus.Pending,
        DateTime.UtcNow.AddHours(2));

    var result = await service.ForceFinishShowtimeAsync(showtime.Id);

    Assert.NotNull(result);

    var pendingReloaded = await bookingDbContext.MovieFeedbackRequests.FirstAsync(x => x.Id == sameShowtimePending.Id);
    var sentReloaded = await bookingDbContext.MovieFeedbackRequests.FirstAsync(x => x.Id == sameShowtimeSent.Id);
    var otherReloaded = await bookingDbContext.MovieFeedbackRequests.FirstAsync(x => x.Id == otherShowtimePending.Id);

    Assert.Equal(result!.NewEndAtUtc, pendingReloaded.AvailableAtUtc);
    Assert.True(pendingReloaded.UpdatedAtUtc >= sameShowtimePending.UpdatedAtUtc);
    Assert.True(sentReloaded.AvailableAtUtc > DateTime.UtcNow);
    Assert.True(otherReloaded.AvailableAtUtc > DateTime.UtcNow);
}
```

```csharp
private static async Task<MovieFeedbackRequest> SeedFeedbackRequestAsync(
    MoviesBookingDbContext dbContext,
    Guid showtimeId,
    MovieFeedbackRequestStatus status,
    DateTime availableAtUtc)
{
    var now = DateTime.UtcNow;
    var request = new MovieFeedbackRequest
    {
        Id = Guid.NewGuid(),
        BookingId = Guid.NewGuid(),
        MovieId = Guid.NewGuid(),
        ShowtimeId = showtimeId,
        PurchaserEmail = "guest@example.com",
        Status = status,
        AvailableAtUtc = availableAtUtc,
        CreatedAtUtc = now.AddDays(-1),
        UpdatedAtUtc = now.AddDays(-1)
    };

    dbContext.MovieFeedbackRequests.Add(request);
    await dbContext.SaveChangesAsync();
    return request;
}
```

- [ ] **Step 2: Run the focused test to verify it fails**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminServiceForceFinishShowtimeTests`

Expected: FAIL because `ForceFinishShowtimeAsync` does not yet update pending feedback requests.

- [ ] **Step 3: Implement the minimal repository change**

```csharp
var pendingRequests = await _bookingDbContext.MovieFeedbackRequests
    .Where(x => x.ShowtimeId == showtimeId && x.Status == MovieFeedbackRequestStatus.Pending)
    .ToListAsync(cancellationToken);

foreach (var pendingRequest in pendingRequests)
{
    pendingRequest.AvailableAtUtc = forcedEndAtUtc;
    pendingRequest.UpdatedAtUtc = utcNow;
}

await _catalogDbContext.SaveChangesAsync(cancellationToken);
await _bookingDbContext.SaveChangesAsync(cancellationToken);
```

- [ ] **Step 4: Run the focused test to verify it passes**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminServiceForceFinishShowtimeTests`

Expected: PASS for the existing showtime test plus the new feedback-unlock test.

- [ ] **Step 5: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceForceFinishShowtimeTests.cs
git commit -m "feat: unlock pending feedback when forcing showtime end"
```

### Task 2: Verify the invitation background service can send after the forced finish

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackInvitationBackgroundServiceTests.cs`

- [ ] **Step 1: Add the failing regression test**

```csharp
[Fact]
public async Task ProcessPendingRequestsOnceAsync_should_send_after_force_finish_unlocks_request_availability()
{
    var request = BuildPendingRequest();
    request.AvailableAtUtc = DateTime.UtcNow.AddHours(2);

    var repository = new FakeInvitationRepository(request);
    var showtimes = new FakeShowtimeRepository(showtimeEnded: true);
    var sender = new FakeMovieFeedbackEmailSender();
    using var provider = BuildServiceProvider(repository, showtimes, sender);

    request.AvailableAtUtc = DateTime.UtcNow.AddMinutes(-1);

    var service = new MovieFeedbackInvitationBackgroundService(
        provider.GetRequiredService<IServiceScopeFactory>(),
        Options.Create(new StripeSettings { FrontendBaseUrl = "https://frontend.test" }),
        NullLogger<MovieFeedbackInvitationBackgroundService>.Instance);

    await service.ProcessPendingRequestsOnceAsync(CancellationToken.None);

    Assert.Single(sender.Messages);
    Assert.Equal(MovieFeedbackRequestStatus.Sent, request.Status);
}
```

- [ ] **Step 2: Run the focused background-service tests**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MovieFeedbackInvitationBackgroundServiceTests`

Expected: PASS, confirming that once `AvailableAtUtc` is moved to the past the existing background service sends as expected.

- [ ] **Step 3: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackInvitationBackgroundServiceTests.cs
git commit -m "test: cover forced showtime feedback unlock flow"
```

### Task 3: Run Movies verification and keep unrelated local changes untouched

**Files:**
- Modify: none
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceForceFinishShowtimeTests.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackInvitationBackgroundServiceTests.cs`

- [ ] **Step 1: Run the focused regression set**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter "MoviesAdminServiceForceFinishShowtimeTests|MovieFeedbackInvitationBackgroundServiceTests"`

Expected: PASS for the force-finish and invitation-send regression coverage.

- [ ] **Step 2: Run the full Movies test suite**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj`

Expected: PASS for the full Movies backend suite.

- [ ] **Step 3: Inspect git status and confirm unrelated files remain untouched**

Run: `git status --short`

Expected:
- no new unexpected changes from this task
- if other pre-existing local modifications remain, do not revert them

- [ ] **Step 4: Commit the implementation plan if needed**

```bash
git add docs/superpowers/plans/2026-04-24-movies-test-finish-showtime-unlock-feedback-implementation.md
git commit -m "docs: add movies test finish showtime unlock feedback plan"
```

Only do this if the plan file is intentionally tracked and still uncommitted at the end.

# Movies Test Finish Showtime Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a test-only Movies admin API that moves one showtime's `EndAtUtc` into the past so the existing feedback invitation background service sends mail through the normal flow.

**Architecture:** Keep the endpoint isolated in a dedicated `MoviesAdminTestController`, but reuse the existing Movies admin application service and repository chain to mutate the catalog `Showtime` row. Gate the endpoint at runtime with `IHostEnvironment` so it returns `404` outside `Development` and `Test`, while preserving the normal admin role requirement.

**Tech Stack:** ASP.NET Core controllers, existing Movies application/infrastructure layers, Entity Framework Core, xUnit

---

## File Structure

- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminController.cs`
  - Reference pattern only; do not add the test endpoint here.
- Create: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminTestController.cs`
  - Hosts test-only Movies admin endpoints under `/api/movies/admin/test`.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminOperationsDtos.cs`
  - Add the response DTO for force-finishing a showtime.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminService.cs`
  - Add one test utility method for force-finishing a showtime.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/MoviesAdminService.cs`
  - Pass the new method through to the repository.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminRepository.cs`
  - Add persistence contract for the force-finish operation.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs`
  - Load the showtime, move `EndAtUtc` to `DateTime.UtcNow.AddMinutes(-1)`, update `UpdatedAtUtc`, and return the DTO.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceForceFinishShowtimeTests.cs`
  - Covers not-found and successful mutation behavior against seeded/in-memory catalog data.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminTestControllerTests.cs`
  - Covers environment blocking and successful controller response mapping.

### Task 1: Add the force-finish contract and repository behavior

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminOperationsDtos.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/MoviesAdminService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceForceFinishShowtimeTests.cs`

- [ ] **Step 1: Write the failing service/repository tests**

```csharp
using ABCDMall.Modules.Movies.Application.Services.Admin;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Repositories.Admin;
using ABCDMall.Modules.Movies.Infrastructure.Services.Tickets;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class MoviesAdminServiceForceFinishShowtimeTests
{
    [Fact]
    public async Task ForceFinishShowtimeAsync_should_return_null_when_showtime_does_not_exist()
    {
        await using var catalogDbContext = await CatalogSeedTestDb.CreateSeededContextAsync();
        await using var bookingDbContext = CreateBookingDbContext();
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);

        var result = await service.ForceFinishShowtimeAsync(Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

        Assert.Null(result);
    }

    [Fact]
    public async Task ForceFinishShowtimeAsync_should_move_end_time_to_the_past_and_refresh_updated_at()
    {
        await using var catalogDbContext = await CatalogSeedTestDb.CreateSeededContextAsync();
        await using var bookingDbContext = CreateBookingDbContext();
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);
        var showtime = await catalogDbContext.Showtimes.OrderBy(x => x.StartAtUtc).FirstAsync();
        var originalUpdatedAtUtc = showtime.UpdatedAtUtc;
        var previousEndAtUtc = showtime.EndAtUtc;

        var result = await service.ForceFinishShowtimeAsync(showtime.Id);

        Assert.NotNull(result);
        Assert.Equal(showtime.Id, result!.ShowtimeId);
        Assert.Equal(previousEndAtUtc, result.PreviousEndAtUtc);
        Assert.True(result.NewEndAtUtc <= DateTime.UtcNow);

        var reloaded = await catalogDbContext.Showtimes.FirstAsync(x => x.Id == showtime.Id);
        Assert.Equal(result.NewEndAtUtc, reloaded.EndAtUtc);
        Assert.True(reloaded.UpdatedAtUtc >= originalUpdatedAtUtc);
    }

    private static MoviesBookingDbContext CreateBookingDbContext()
    {
        var options = new DbContextOptionsBuilder<MoviesBookingDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new MoviesBookingDbContext(options);
    }

    private sealed class NoOpTicketEmailDispatcher : ITicketEmailDispatcher
    {
        public Task QueueTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminServiceForceFinishShowtimeTests`

Expected: FAIL because `ForceFinishShowtimeAsync` and the response DTO do not exist yet.

- [ ] **Step 3: Add the DTO and application contracts**

```csharp
public sealed class MoviesAdminForceFinishShowtimeResponseDto
{
    public Guid ShowtimeId { get; set; }
    public DateTime? PreviousEndAtUtc { get; set; }
    public DateTime NewEndAtUtc { get; set; }
    public string Message { get; set; } = string.Empty;
}
```

```csharp
Task<MoviesAdminForceFinishShowtimeResponseDto?> ForceFinishShowtimeAsync(
    Guid showtimeId,
    CancellationToken cancellationToken = default);
```

```csharp
public Task<MoviesAdminForceFinishShowtimeResponseDto?> ForceFinishShowtimeAsync(
    Guid showtimeId,
    CancellationToken cancellationToken = default)
    => _repository.ForceFinishShowtimeAsync(showtimeId, cancellationToken);
```

- [ ] **Step 4: Implement the repository mutation**

```csharp
public async Task<MoviesAdminForceFinishShowtimeResponseDto?> ForceFinishShowtimeAsync(
    Guid showtimeId,
    CancellationToken cancellationToken = default)
{
    var showtime = await _catalogDbContext.Showtimes
        .FirstOrDefaultAsync(x => x.Id == showtimeId, cancellationToken);

    if (showtime is null)
    {
        return null;
    }

    var utcNow = DateTime.UtcNow;
    var forcedEndAtUtc = utcNow.AddMinutes(-1);
    var previousEndAtUtc = showtime.EndAtUtc;

    showtime.EndAtUtc = forcedEndAtUtc;
    showtime.UpdatedAtUtc = utcNow;

    await _catalogDbContext.SaveChangesAsync(cancellationToken);

    return new MoviesAdminForceFinishShowtimeResponseDto
    {
        ShowtimeId = showtime.Id,
        PreviousEndAtUtc = previousEndAtUtc,
        NewEndAtUtc = forcedEndAtUtc,
        Message = "Showtime end time moved to the past for feedback-email testing."
    };
}
```

- [ ] **Step 5: Run the focused test to verify it passes**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminServiceForceFinishShowtimeTests`

Expected: PASS with `2 passed`.

- [ ] **Step 6: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminOperationsDtos.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/MoviesAdminService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminRepository.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceForceFinishShowtimeTests.cs
git commit -m "feat: add movies admin force finish showtime service"
```

### Task 2: Expose the test-only controller with environment blocking

**Files:**
- Create: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminTestController.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminTestControllerTests.cs`

- [ ] **Step 1: Write the failing controller tests**

```csharp
using ABCDMall.Modules.Movies.Application.DTOs.Admin;
using ABCDMall.Modules.Movies.Application.Services.Admin;
using ABCDMall.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class MoviesAdminTestControllerTests
{
    [Fact]
    public async Task ForceFinishShowtime_should_return_not_found_outside_dev_or_test()
    {
        var service = new FakeMoviesAdminService();
        var controller = new MoviesAdminTestController(service, new FakeHostEnvironment("Production"));

        var result = await controller.ForceFinishShowtime(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result.Result);
        Assert.Equal(0, service.CallCount);
    }

    [Fact]
    public async Task ForceFinishShowtime_should_return_ok_when_environment_is_development()
    {
        var service = new FakeMoviesAdminService
        {
            Response = new MoviesAdminForceFinishShowtimeResponseDto
            {
                ShowtimeId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                PreviousEndAtUtc = DateTime.UtcNow.AddHours(2),
                NewEndAtUtc = DateTime.UtcNow.AddMinutes(-1),
                Message = "Showtime end time moved to the past for feedback-email testing."
            }
        };

        var controller = new MoviesAdminTestController(service, new FakeHostEnvironment(Environments.Development));

        var result = await controller.ForceFinishShowtime(service.Response.ShowtimeId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var payload = Assert.IsType<MoviesAdminForceFinishShowtimeResponseDto>(ok.Value);
        Assert.Equal(service.Response.ShowtimeId, payload.ShowtimeId);
        Assert.Equal(1, service.CallCount);
    }

    private sealed class FakeMoviesAdminService : IMoviesAdminService
    {
        public int CallCount { get; private set; }
        public MoviesAdminForceFinishShowtimeResponseDto? Response { get; set; }

        public Task<MoviesAdminForceFinishShowtimeResponseDto?> ForceFinishShowtimeAsync(Guid showtimeId, CancellationToken cancellationToken = default)
        {
            CallCount += 1;
            return Task.FromResult(Response);
        }

        // Throw NotSupportedException for the rest of IMoviesAdminService members.
    }

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public FakeHostEnvironment(string environmentName) => EnvironmentName = environmentName;
        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; } = "Movies.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = null!;
    }
}
```

- [ ] **Step 2: Run the test to verify it fails**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminTestControllerTests`

Expected: FAIL because `MoviesAdminTestController` does not exist yet.

- [ ] **Step 3: Add the controller**

```csharp
using ABCDMall.Modules.Movies.Application.DTOs.Admin;
using ABCDMall.Modules.Movies.Application.Services.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "MoviesAdmin,Admin")]
[Route("api/movies/admin/test")]
public sealed class MoviesAdminTestController : ControllerBase
{
    private readonly IMoviesAdminService _moviesAdminService;
    private readonly IHostEnvironment _environment;

    public MoviesAdminTestController(IMoviesAdminService moviesAdminService, IHostEnvironment environment)
    {
        _moviesAdminService = moviesAdminService;
        _environment = environment;
    }

    [HttpPost("showtimes/{showtimeId:guid}/finish-now")]
    public async Task<ActionResult<MoviesAdminForceFinishShowtimeResponseDto>> ForceFinishShowtime(
        Guid showtimeId,
        CancellationToken cancellationToken = default)
    {
        if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Test"))
        {
            return NotFound();
        }

        var result = await _moviesAdminService.ForceFinishShowtimeAsync(showtimeId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }
}
```

- [ ] **Step 4: Run the controller test to verify it passes**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminTestControllerTests`

Expected: PASS with `2 passed`.

- [ ] **Step 5: Commit**

```bash
git add BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminTestController.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminTestControllerTests.cs
git commit -m "feat: add test-only movies finish showtime endpoint"
```

### Task 3: Run full verification for the Movies feedback testing flow

**Files:**
- Modify: none
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceForceFinishShowtimeTests.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminTestControllerTests.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MovieFeedbackInvitationBackgroundServiceTests.cs`

- [ ] **Step 1: Run the focused backend regression set**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter "MoviesAdminServiceForceFinishShowtimeTests|MoviesAdminTestControllerTests|MovieFeedbackInvitationBackgroundServiceTests"`

Expected: PASS for the new force-finish tests and the existing invitation-background tests, showing the test endpoint mutates the same showtime field the mail workflow already trusts.

- [ ] **Step 2: Run the full Movies test suite**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj`

Expected: PASS for the entire Movies backend test suite with no regressions.

- [ ] **Step 3: Inspect worktree status**

Run: `git status --short`

Expected: clean working tree.

- [ ] **Step 4: Commit verification checkpoint if any follow-up test-only fixes were needed**

```bash
git add -A
git commit -m "test: verify movies finish showtime admin flow"
```

Only create this commit if Task 3 required code changes after verification. If verification passes without further edits, skip the commit and leave the branch clean.

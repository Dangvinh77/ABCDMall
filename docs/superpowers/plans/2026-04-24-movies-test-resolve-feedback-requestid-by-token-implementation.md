# Movies Test Resolve Feedback RequestId By Token Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a test-only Movies admin API that accepts a public feedback token and resolves the corresponding `MovieFeedbackRequest.Id`.

**Architecture:** Reuse the existing `MoviesAdminTestController` and Movies admin service/repository chain instead of touching the public feedback API. The lookup will hash the token with the same SHA256 rule used by the public feedback flow, query `MovieFeedbackRequest` by `TokenHash`, and return only `requestId`.

**Tech Stack:** ASP.NET Core controllers, existing Movies application/infrastructure layers, Entity Framework Core, xUnit

---

## File Structure

- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminOperationsDtos.cs`
  - Add request/response DTOs for resolving `requestId` by token.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminService.cs`
  - Add one admin test lookup method for resolving the feedback request id.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/MoviesAdminService.cs`
  - Pass the new lookup through to the repository.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminRepository.cs`
  - Add the persistence contract for token-based resolution.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs`
  - Hash the token and query `MovieFeedbackRequests` by `TokenHash`.
- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminTestController.cs`
  - Add `POST /api/movies/admin/test/feedback-links/resolve-request` with test-environment guard and bad-request handling for blank tokens.
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminTestControllerTests.cs`
  - Add controller tests for success, blank token, not found, and environment blocking.
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceResolveFeedbackRequestByTokenTests.cs`
  - Cover token hashing and repository/service lookup against an in-memory booking context.

### Task 1: Add the token-resolution contract and repository lookup

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminOperationsDtos.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/MoviesAdminService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceResolveFeedbackRequestByTokenTests.cs`

- [ ] **Step 1: Write the failing service/repository tests**

```csharp
using System.Security.Cryptography;
using System.Text;
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

public sealed class MoviesAdminServiceResolveFeedbackRequestByTokenTests
{
    [Fact]
    public async Task ResolveFeedbackRequestIdByTokenAsync_should_return_null_when_token_is_unknown()
    {
        await using var catalogDbContext = CreateCatalogDbContext();
        await using var bookingDbContext = CreateBookingDbContext();
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);

        var result = await service.ResolveFeedbackRequestIdByTokenAsync("unknown-token");

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveFeedbackRequestIdByTokenAsync_should_return_request_id_for_matching_token()
    {
        await using var catalogDbContext = CreateCatalogDbContext();
        await using var bookingDbContext = CreateBookingDbContext();
        const string token = "feedback-token";
        var request = await SeedFeedbackRequestAsync(bookingDbContext, token);
        var repository = new MoviesAdminRepository(catalogDbContext, bookingDbContext, new NoOpTicketEmailDispatcher());
        var service = new MoviesAdminService(repository);

        var result = await service.ResolveFeedbackRequestIdByTokenAsync(token);

        Assert.NotNull(result);
        Assert.Equal(request.Id, result!.RequestId);
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

    private static async Task<MovieFeedbackRequest> SeedFeedbackRequestAsync(MoviesBookingDbContext dbContext, string token)
    {
        var now = DateTime.UtcNow;
        var request = new MovieFeedbackRequest
        {
            Id = Guid.Parse("93000000-0000-0000-0000-000000000001"),
            BookingId = Guid.NewGuid(),
            MovieId = Guid.NewGuid(),
            ShowtimeId = Guid.NewGuid(),
            PurchaserEmail = "guest@example.com",
            TokenHash = HashToken(token),
            Status = MovieFeedbackRequestStatus.Sent,
            AvailableAtUtc = now.AddHours(-1),
            SentAtUtc = now.AddHours(-1),
            CreatedAtUtc = now.AddDays(-1),
            UpdatedAtUtc = now.AddDays(-1)
        };

        dbContext.MovieFeedbackRequests.Add(request);
        await dbContext.SaveChangesAsync();
        return request;
    }

    private static string HashToken(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hashBytes);
    }

    private sealed class NoOpTicketEmailDispatcher : ITicketEmailDispatcher
    {
        public Task SendTicketEmailAsync(Guid bookingId, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
```

- [ ] **Step 2: Run the focused test to verify it fails**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminServiceResolveFeedbackRequestByTokenTests`

Expected: FAIL because `ResolveFeedbackRequestIdByTokenAsync` and the request/response DTOs do not exist yet.

- [ ] **Step 3: Add DTOs and service/repository contracts**

```csharp
public sealed class MoviesAdminResolveFeedbackRequestByTokenRequestDto
{
    public string Token { get; set; } = string.Empty;
}

public sealed class MoviesAdminResolveFeedbackRequestByTokenResponseDto
{
    public Guid RequestId { get; set; }
}
```

```csharp
Task<MoviesAdminResolveFeedbackRequestByTokenResponseDto?> ResolveFeedbackRequestIdByTokenAsync(
    string token,
    CancellationToken cancellationToken = default);
```

```csharp
public Task<MoviesAdminResolveFeedbackRequestByTokenResponseDto?> ResolveFeedbackRequestIdByTokenAsync(
    string token,
    CancellationToken cancellationToken = default)
    => _repository.ResolveFeedbackRequestIdByTokenAsync(token, cancellationToken);
```

- [ ] **Step 4: Implement the repository lookup**

```csharp
public async Task<MoviesAdminResolveFeedbackRequestByTokenResponseDto?> ResolveFeedbackRequestIdByTokenAsync(
    string token,
    CancellationToken cancellationToken = default)
{
    if (string.IsNullOrWhiteSpace(token))
    {
        return null;
    }

    var normalizedToken = token.Trim();
    var tokenHash = HashToken(normalizedToken);
    var requestId = await _bookingDbContext.MovieFeedbackRequests
        .Where(x => x.TokenHash == tokenHash)
        .Select(x => (Guid?)x.Id)
        .FirstOrDefaultAsync(cancellationToken);

    return requestId.HasValue
        ? new MoviesAdminResolveFeedbackRequestByTokenResponseDto
        {
            RequestId = requestId.Value
        }
        : null;
}

private static string HashToken(string token)
{
    var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
    return Convert.ToHexString(hashBytes);
}
```

- [ ] **Step 5: Run the focused test to verify it passes**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminServiceResolveFeedbackRequestByTokenTests`

Expected: PASS with `2 passed`.

- [ ] **Step 6: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Admin/MoviesAdminOperationsDtos.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/MoviesAdminService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Admin/IMoviesAdminRepository.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Admin/MoviesAdminRepository.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceResolveFeedbackRequestByTokenTests.cs
git commit -m "feat: resolve movie feedback request ids by token"
```

### Task 2: Expose the test-only controller endpoint and validate request handling

**Files:**
- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminTestController.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminTestControllerTests.cs`

- [ ] **Step 1: Write the failing controller tests**

```csharp
[Fact]
public async Task ResolveFeedbackRequest_should_return_not_found_outside_dev_or_test()
{
    var service = new FakeMoviesAdminService();
    var controller = new MoviesAdminTestController(service, new FakeHostEnvironment("Production"));

    var result = await controller.ResolveFeedbackRequest(
        new MoviesAdminResolveFeedbackRequestByTokenRequestDto { Token = "feedback-token" });

    Assert.IsType<NotFoundResult>(result.Result);
}

[Fact]
public async Task ResolveFeedbackRequest_should_return_bad_request_when_token_is_blank()
{
    var service = new FakeMoviesAdminService();
    var controller = new MoviesAdminTestController(service, new FakeHostEnvironment(Environments.Development));

    var result = await controller.ResolveFeedbackRequest(
        new MoviesAdminResolveFeedbackRequestByTokenRequestDto { Token = "   " });

    Assert.IsType<BadRequestObjectResult>(result.Result);
}

[Fact]
public async Task ResolveFeedbackRequest_should_return_ok_when_token_matches()
{
    var response = new MoviesAdminResolveFeedbackRequestByTokenResponseDto
    {
        RequestId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd")
    };

    var service = new FakeMoviesAdminService
    {
        ResolveFeedbackRequestResponse = response
    };
    var controller = new MoviesAdminTestController(service, new FakeHostEnvironment(Environments.Development));

    var result = await controller.ResolveFeedbackRequest(
        new MoviesAdminResolveFeedbackRequestByTokenRequestDto { Token = "feedback-token" });

    var ok = Assert.IsType<OkObjectResult>(result.Result);
    var payload = Assert.IsType<MoviesAdminResolveFeedbackRequestByTokenResponseDto>(ok.Value);
    Assert.Equal(response.RequestId, payload.RequestId);
}
```

- [ ] **Step 2: Run the controller tests to verify they fail**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminTestControllerTests`

Expected: FAIL because the new action and fake-service implementation do not exist yet.

- [ ] **Step 3: Add the controller action and extend the fake service**

```csharp
[HttpPost("feedback-links/resolve-request")]
public async Task<ActionResult<MoviesAdminResolveFeedbackRequestByTokenResponseDto>> ResolveFeedbackRequest(
    [FromBody] MoviesAdminResolveFeedbackRequestByTokenRequestDto request,
    CancellationToken cancellationToken = default)
{
    if (!_environment.IsDevelopment() && !_environment.IsEnvironment("Test"))
    {
        return NotFound();
    }

    if (request is null || string.IsNullOrWhiteSpace(request.Token))
    {
        return BadRequest(new ProblemDetails
        {
            Title = "Token is required.",
            Detail = "Provide the feedback token in the request body.",
            Status = StatusCodes.Status400BadRequest
        });
    }

    var result = await _moviesAdminService.ResolveFeedbackRequestIdByTokenAsync(request.Token, cancellationToken);
    return result is null ? NotFound() : Ok(result);
}
```

```csharp
public MoviesAdminResolveFeedbackRequestByTokenResponseDto? ResolveFeedbackRequestResponse { get; set; }
public int ResolveFeedbackRequestCallCount { get; private set; }

public Task<MoviesAdminResolveFeedbackRequestByTokenResponseDto?> ResolveFeedbackRequestIdByTokenAsync(
    string token,
    CancellationToken cancellationToken = default)
{
    ResolveFeedbackRequestCallCount += 1;
    return Task.FromResult(ResolveFeedbackRequestResponse);
}
```

- [ ] **Step 4: Run the controller tests to verify they pass**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter MoviesAdminTestControllerTests`

Expected: PASS for existing controller tests plus the new resolve-request coverage.

- [ ] **Step 5: Commit**

```bash
git add BACKEND/ABCDMall.WebAPI/Controllers/MoviesAdminTestController.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminTestControllerTests.cs
git commit -m "feat: add test-only movie feedback token resolver endpoint"
```

### Task 3: Run Movies verification and keep unrelated local changes untouched

**Files:**
- Modify: none
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminServiceResolveFeedbackRequestByTokenTests.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/MoviesAdminTestControllerTests.cs`

- [ ] **Step 1: Run the focused regression set**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter "MoviesAdminServiceResolveFeedbackRequestByTokenTests|MoviesAdminTestControllerTests"`

Expected: PASS for the token-resolution service and controller coverage.

- [ ] **Step 2: Run the full Movies test suite**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj`

Expected: PASS for the full Movies backend suite.

- [ ] **Step 3: Inspect git status**

Run: `git status --short`

Expected:
- no new unexpected changes from this task
- if unrelated pre-existing local modifications remain, do not revert them

- [ ] **Step 4: Commit the implementation plan if needed**

```bash
git add docs/superpowers/plans/2026-04-24-movies-test-resolve-feedback-requestid-by-token-implementation.md
git commit -m "docs: add movies test resolve feedback requestid plan"
```

Only do this if the plan file is intentionally tracked and still uncommitted at the end.

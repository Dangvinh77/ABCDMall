# Movies Per-Seat Hold Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Start a real 10 minute hold per seat at seat-selection time, render one countdown per selected seat, release only the clicked seat hold on unselect, and let checkout create one booking from multiple hold IDs.

**Architecture:** Keep the existing backend hold duration, hold cleanup, and held-seat projection, but move hold creation from checkout into seat selection. On the backend, evolve booking creation from a single-hold flow into a multi-hold aggregation flow while keeping the seat-map contract backend-driven. On the frontend, replace the page-level countdown with per-seat hold state derived from backend `expiresAtUtc`.

**Tech Stack:** ASP.NET Core Web API, C# application services and repositories, React 19 + TypeScript + Vite, existing movies feature API adapter, xUnit test project for backend, Vitest + React Testing Library for new frontend tests.

---

## File Structure

### Backend files to modify

- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Bookings/CreateBookingRequestDto.cs`
  - Change booking input from one `HoldId` to multiple `HoldIds`.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Bookings/CreateBookingResponseDto.cs`
  - Return aggregated hold information in a shape frontend can consume consistently.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Bookings/BookingService.cs`
  - Validate and aggregate multiple holds into one booking.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Bookings/IBookingRepository.cs`
  - Add repository APIs for loading and consuming many holds atomically.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Bookings/Validators/CreateBookingRequestDtoValidator.cs`
  - Validate `HoldIds` instead of a single `HoldId`.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Bookings/BookingRepository.cs`
  - Implement multi-hold retrieval, idempotency check, and atomic persistence.
- `BACKEND/ABCDMall.WebAPI/Controllers/BookingsController.cs`
  - Keep route but accept the new booking payload shape.

### Backend files to create

- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/BookingHoldServiceTests.cs`
  - Cover one-seat hold creation and release behavior.
- `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/BookingServiceTests.cs`
  - Cover multi-hold booking creation, validation, and failure cases.

### Frontend files to modify

- `FRONTEND/src/features/movies/api/moviesApi.ts`
  - Change booking payload model to `holdIds`, keep hold/release helpers first-class.
- `FRONTEND/src/features/movies/data/booking.ts`
  - Replace single-hold booking state with per-seat hold metadata.
- `FRONTEND/src/features/movies/pages/SeatSelectionPage.tsx`
  - Create one hold per selected seat, remove page timer, render per-seat timers, release hold on unselect.
- `FRONTEND/src/features/movies/pages/CheckOutPage.tsx`
  - Load many hold IDs, quote totals from held seats, and create booking from many holds.
- `FRONTEND/package.json`
  - Add frontend test dependencies and scripts.

### Frontend files to create

- `FRONTEND/src/features/movies/pages/__tests__/SeatSelectionPage.test.tsx`
  - Verify immediate hold creation, per-seat timers, and release-on-unselect behavior.
- `FRONTEND/src/features/movies/pages/__tests__/CheckOutPage.test.tsx`
  - Verify checkout submits many hold IDs.
- `FRONTEND/src/test/setup.ts`
  - Shared Vitest + RTL setup if the frontend workspace does not already have one.
- `FRONTEND/vitest.config.ts`
  - Minimal test runner config if absent.

### Verification commands

- Backend targeted tests:
  - `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter Booking`
- Frontend targeted tests:
  - `npm run test -- SeatSelectionPage`
  - `npm run test -- CheckOutPage`
- Frontend build/lint:
  - `npm run build`
  - `npm run lint`

## Task 1: Lock Backend Booking Contract to Multi-Hold Input

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Bookings/CreateBookingRequestDto.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Bookings/Validators/CreateBookingRequestDtoValidator.cs`
- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/BookingsController.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/BookingServiceTests.cs`

- [ ] **Step 1: Write the failing backend contract test**

```csharp
using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using FluentValidation.TestHelper;

public sealed class CreateBookingRequestDtoValidatorTests
{
    [Fact]
    public void Should_require_at_least_one_hold_id()
    {
        var validator = new CreateBookingRequestDtoValidator();
        var model = new CreateBookingRequestDto
        {
            HoldIds = Array.Empty<Guid>(),
            CustomerName = "Alice",
            CustomerEmail = "alice@example.com",
            CustomerPhoneNumber = "0900000000"
        };

        var result = validator.TestValidate(model);

        result.ShouldHaveValidationErrorFor(x => x.HoldIds);
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter CreateBookingRequestDtoValidatorTests`

Expected: FAIL because `CreateBookingRequestDto` does not yet expose `HoldIds` and the validator still targets `HoldId`.

- [ ] **Step 3: Update DTO and validator with the minimal multi-hold contract**

```csharp
public sealed class CreateBookingRequestDto
{
    public IReadOnlyCollection<Guid> HoldIds { get; set; } = Array.Empty<Guid>();
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;
}
```

```csharp
RuleFor(x => x.HoldIds)
    .NotEmpty()
    .Must(ids => ids.Distinct().Count() == ids.Count)
    .WithMessage("HoldIds must contain at least one unique hold id.");
```

- [ ] **Step 4: Keep controller binding aligned with the new payload**

```csharp
[HttpPost]
public async Task<ActionResult<CreateBookingResponseDto>> CreateBooking(
    [FromBody] CreateBookingRequestDto request,
    CancellationToken cancellationToken = default)
{
    var validationResult = await _createBookingValidator.ValidateAsync(request, cancellationToken);
    if (!validationResult.IsValid)
    {
        return ValidationProblem(ToValidationProblemDetails(validationResult));
    }

    var result = await _bookingService.CreateAsync(request, cancellationToken);
    return CreatedAtAction(nameof(GetBooking), new { bookingCode = result.BookingCode }, result);
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter CreateBookingRequestDtoValidatorTests`

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Bookings/CreateBookingRequestDto.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Bookings/Validators/CreateBookingRequestDtoValidator.cs BACKEND/ABCDMall.WebAPI/Controllers/BookingsController.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/BookingServiceTests.cs
git commit -m "feat: change movies booking contract to multi-hold input"
```

## Task 2: Add Backend Tests for One-Seat Hold Creation and Release

**Files:**
- Create: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/BookingHoldServiceTests.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/FakeBookingHoldRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Bookings/BookingHoldService.cs` only if tests expose a bug

- [ ] **Step 1: Write the failing hold service tests**

```csharp
public sealed class BookingHoldServiceTests
{
    [Fact]
    public async Task CreateAsync_should_create_a_single_seat_hold_with_ten_minute_expiry()
    {
        var service = BuildService();
        var request = new CreateBookingHoldRequestDto
        {
            ShowtimeId = TestIds.ShowtimeId,
            SeatInventoryIds = new[] { TestIds.SeatA1 }
        };

        var result = await service.CreateAsync(request, CancellationToken.None);

        Assert.Single(result.Seats);
        Assert.Equal("Active", result.Status);
        Assert.InRange(result.RemainingSeconds, 590, 600);
    }

    [Fact]
    public async Task ReleaseAsync_should_only_release_the_target_hold()
    {
        var repository = new FakeBookingHoldRepository();
        var service = BuildService(repository);

        var first = await service.CreateAsync(new CreateBookingHoldRequestDto
        {
            ShowtimeId = TestIds.ShowtimeId,
            SeatInventoryIds = new[] { TestIds.SeatA1 }
        });

        var second = await service.CreateAsync(new CreateBookingHoldRequestDto
        {
            ShowtimeId = TestIds.ShowtimeId,
            SeatInventoryIds = new[] { TestIds.SeatA2 }
        });

        await service.ReleaseAsync(first.HoldId, CancellationToken.None);

        var released = await service.GetByIdAsync(first.HoldId, CancellationToken.None);
        var active = await service.GetByIdAsync(second.HoldId, CancellationToken.None);

        Assert.Equal("Released", released!.Status);
        Assert.Equal("Active", active!.Status);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter BookingHoldServiceTests`

Expected: FAIL if fake repository coverage is incomplete or hold/release behavior is not yet testable through the fake implementation.

- [ ] **Step 3: Extend the fake repository only as much as needed**

```csharp
public sealed class FakeBookingHoldRepository : IBookingHoldRepository
{
    private readonly List<BookingHold> _holds = new();

    public Task<BookingHold> AddAsync(BookingHold hold, CancellationToken cancellationToken)
    {
        _holds.Add(hold);
        return Task.FromResult(hold);
    }

    public Task<BookingHold?> GetByIdAsync(Guid holdId, CancellationToken cancellationToken = default)
        => Task.FromResult(_holds.FirstOrDefault(x => x.Id == holdId));

    public Task<bool> ReleaseAsync(Guid holdId, DateTime utcNow, CancellationToken cancellationToken = default)
    {
        var hold = _holds.FirstOrDefault(x => x.Id == holdId);
        if (hold is null || hold.Status != BookingHoldStatus.Active) return Task.FromResult(false);
        hold.Status = BookingHoldStatus.Released;
        hold.UpdatedAtUtc = utcNow;
        return Task.FromResult(true);
    }
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter BookingHoldServiceTests`

Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/BookingHoldServiceTests.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/FakeBookingHoldRepository.cs
git commit -m "test: cover single-seat hold creation and release"
```

## Task 3: Implement Backend Multi-Hold Booking Aggregation

**Files:**
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Bookings/BookingService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Bookings/IBookingRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Bookings/BookingRepository.cs`
- Modify: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Bookings/CreateBookingResponseDto.cs`
- Test: `BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/BookingServiceTests.cs`

- [ ] **Step 1: Write the failing booking aggregation tests**

```csharp
public sealed class BookingServiceTests
{
    [Fact]
    public async Task CreateAsync_should_create_one_booking_from_multiple_active_holds()
    {
        var service = BuildServiceWithRepository(out var repository);
        var holdIds = repository.SeedActiveHoldsForSameShowtime();

        var result = await service.CreateAsync(new CreateBookingRequestDto
        {
            HoldIds = holdIds,
            CustomerName = "Alice",
            CustomerEmail = "alice@example.com",
            CustomerPhoneNumber = "0900000000"
        });

        Assert.Equal(TestIds.ShowtimeId, result.ShowtimeId);
        Assert.True(result.PaymentRequired);
    }

    [Fact]
    public async Task CreateAsync_should_fail_when_any_hold_is_expired()
    {
        var service = BuildServiceWithRepository(out var repository);
        var holdIds = repository.SeedOneActiveAndOneExpiredHold();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateBookingRequestDto
            {
                HoldIds = holdIds,
                CustomerName = "Alice",
                CustomerEmail = "alice@example.com",
                CustomerPhoneNumber = "0900000000"
            }));

        Assert.Contains("expired", error.Message, StringComparison.OrdinalIgnoreCase);
    }
}
```

- [ ] **Step 2: Run tests to verify they fail**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter BookingServiceTests`

Expected: FAIL because `BookingService` still expects one `HoldId` and repository APIs still load one hold.

- [ ] **Step 3: Add repository interfaces for many holds and booking lookup**

```csharp
public interface IBookingRepository
{
    Task<Bookingg?> GetByCombinedHoldIdsAsync(IReadOnlyCollection<Guid> holdIds, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BookingHold>> GetHoldsForBookingAsync(IReadOnlyCollection<Guid> holdIds, CancellationToken cancellationToken = default);
    Task<Bookingg> AddPendingBookingAsync(Bookingg booking, GuestCustomer? newGuestCustomer, IReadOnlyCollection<Guid> holdIds, DateTime utcNow, CancellationToken cancellationToken);
}
```

- [ ] **Step 4: Update booking service with minimal aggregation logic**

```csharp
var holdIds = request.HoldIds.Distinct().ToArray();
var holds = await _bookingRepository.GetHoldsForBookingAsync(holdIds, cancellationToken);

if (holds.Count != holdIds.Length)
{
    throw new InvalidOperationException("One or more booking holds were not found.");
}

if (holds.Any(h => h.Status != BookingHoldStatus.Active))
{
    throw new InvalidOperationException("One or more booking holds are no longer active.");
}

if (holds.Any(h => h.ExpiresAtUtc <= now))
{
    throw new InvalidOperationException("One or more booking holds have expired.");
}

if (holds.Select(h => h.ShowtimeId).Distinct().Count() != 1)
{
    throw new InvalidOperationException("Booking holds must belong to the same showtime.");
}
```

- [ ] **Step 5: Build the booking from all seats and convert all consumed holds atomically**

```csharp
var booking = new Bookingg
{
    Id = Guid.NewGuid(),
    BookingCode = GenerateBookingCode(now),
    ShowtimeId = holds[0].ShowtimeId,
    Status = BookingStatus.PendingPayment,
    CustomerName = request.CustomerName.Trim(),
    CustomerEmail = request.CustomerEmail.Trim(),
    CustomerPhoneNumber = request.CustomerPhoneNumber.Trim(),
    SeatSubtotal = holds.Sum(x => x.SeatSubtotal),
    ComboSubtotal = holds.Sum(x => x.ComboSubtotal),
    ServiceFee = holds.Sum(x => x.ServiceFee),
    DiscountAmount = holds.Sum(x => x.DiscountAmount),
    GrandTotal = holds.Sum(x => x.GrandTotal),
    Currency = "VND",
    CreatedAtUtc = now,
    UpdatedAtUtc = now
};

foreach (var hold in holds.SelectMany(x => x.Seats).OrderBy(x => x.SeatCode))
{
    booking.Items.Add(new BookingItem
    {
        Id = Guid.NewGuid(),
        BookingId = booking.Id,
        ItemType = "Seat",
        ItemCode = hold.SeatCode,
        Description = $"Seat {hold.SeatCode}",
        SeatInventoryId = hold.SeatInventoryId,
        Quantity = 1,
        UnitPrice = hold.UnitPrice,
        LineTotal = hold.UnitPrice
    });
}
```

- [ ] **Step 6: Update repository transaction logic**

```csharp
await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

var holds = await _dbContext.BookingHolds
    .Include(x => x.Seats)
    .Where(x => holdIds.Contains(x.Id))
    .ToListAsync(cancellationToken);

_dbContext.Bookings.Add(booking);

foreach (var hold in holds)
{
    hold.Status = BookingHoldStatus.Converted;
    hold.UpdatedAtUtc = utcNow;
}

await _dbContext.SaveChangesAsync(cancellationToken);
await transaction.CommitAsync(cancellationToken);
```

- [ ] **Step 7: Run tests to verify they pass**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter BookingServiceTests`

Expected: PASS

- [ ] **Step 8: Commit**

```bash
git add BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Bookings/BookingService.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/Services/Bookings/IBookingRepository.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Infrastructure/Repositories/Bookings/BookingRepository.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Application/DTOs/Bookings/CreateBookingResponseDto.cs BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/BookingServiceTests.cs
git commit -m "feat: aggregate multiple seat holds into one booking"
```

## Task 4: Update Frontend API Models and Booking State for Per-Seat Holds

**Files:**
- Modify: `FRONTEND/src/features/movies/api/moviesApi.ts`
- Modify: `FRONTEND/src/features/movies/data/booking.ts`
- Test: `FRONTEND/src/features/movies/pages/__tests__/CheckOutPage.test.tsx`

- [ ] **Step 1: Write the failing checkout API payload test**

```tsx
it("submits all selected hold ids when creating a booking", async () => {
  const createBooking = vi.fn();

  await submitCheckout({
    holdIds: ["hold-a", "hold-b"],
    customerName: "Alice",
    customerEmail: "alice@example.com",
    customerPhoneNumber: "0900000000",
  });

  expect(createBooking).toHaveBeenCalledWith(
    expect.objectContaining({ holdIds: ["hold-a", "hold-b"] }),
  );
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npm run test -- CheckOutPage`

Expected: FAIL because `CreateBookingPayload` still exposes `holdId` and booking state only tracks one hold.

- [ ] **Step 3: Update API types to the new payload**

```ts
export interface CreateBookingPayload {
  holdIds: string[];
  customerName: string;
  customerEmail: string;
  customerPhoneNumber: string;
}

export interface BookingModel {
  bookingId: string;
  bookingCode: string;
  showtimeId: string;
  holdIds: string[];
  status: string;
  grandTotal: number;
  currency: string;
  paymentRequired: boolean;
}
```

```ts
export async function createBooking(payload: CreateBookingPayload) {
  const response = await api.post<CreateBookingResponseDto, CreateBookingPayload>("/bookings", {
    holdIds: payload.holdIds,
    customerName: payload.customerName,
    customerEmail: payload.customerEmail,
    customerPhoneNumber: payload.customerPhoneNumber,
  });
}
```

- [ ] **Step 4: Replace single-hold booking state with per-seat hold state**

```ts
export interface SelectedSeatHold {
  holdId: string;
  holdCode?: string;
  expiresAtUtc: string;
  remainingSeconds: number;
}

export interface SelectedSeat {
  id: string;
  type: SeatType;
  seatInventoryId?: string;
  hold?: SelectedSeatHold;
}

export interface BookingState {
  seats: SelectedSeat[];
  subtotal: number;
  serviceFee: number;
  total: number;
  comboSubtotal?: number;
  discountAmount?: number;
  combos?: SelectedSnackCombo[];
  promoId?: string | null;
  bookingDate?: string;
}
```

- [ ] **Step 5: Run test to verify it passes**

Run: `npm run test -- CheckOutPage`

Expected: PASS for the payload shape test

- [ ] **Step 6: Commit**

```bash
git add FRONTEND/src/features/movies/api/moviesApi.ts FRONTEND/src/features/movies/data/booking.ts FRONTEND/src/features/movies/pages/__tests__/CheckOutPage.test.tsx
git commit -m "feat: align movies frontend state with multi-hold booking"
```

## Task 5: Add Frontend Test Harness and Seat-Page Tests

**Files:**
- Modify: `FRONTEND/package.json`
- Create: `FRONTEND/vitest.config.ts`
- Create: `FRONTEND/src/test/setup.ts`
- Create: `FRONTEND/src/features/movies/pages/__tests__/SeatSelectionPage.test.tsx`

- [ ] **Step 1: Add failing seat-page tests for immediate hold creation and release**

```tsx
it("creates a hold immediately when an available seat is selected", async () => {
  renderSeatSelectionPage();

  await user.click(screen.getByRole("button", { name: /Seat A1/i }));

  expect(createBookingHold).toHaveBeenCalledWith(
    expect.objectContaining({ seatInventoryIds: ["seat-a1"] }),
  );
});

it("releases the specific seat hold when the selected seat is clicked again", async () => {
  createBookingHold.mockResolvedValueOnce(mockHold("hold-a1", "A1"));
  renderSeatSelectionPage();

  await user.click(screen.getByRole("button", { name: /Seat A1/i }));
  await user.click(screen.getByRole("button", { name: /Seat A1/i }));

  expect(releaseBookingHold).toHaveBeenCalledWith("hold-a1");
});
```

- [ ] **Step 2: Add minimal test tooling**

```json
{
  "scripts": {
    "test": "vitest run"
  },
  "devDependencies": {
    "@testing-library/jest-dom": "^6.7.0",
    "@testing-library/react": "^16.3.0",
    "@testing-library/user-event": "^14.6.1",
    "jsdom": "^26.1.0",
    "vitest": "^3.2.4"
  }
}
```

```ts
// vitest.config.ts
import { defineConfig } from "vitest/config";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  test: {
    environment: "jsdom",
    setupFiles: "./src/test/setup.ts",
  },
});
```

- [ ] **Step 3: Run tests to verify they fail for the right reason**

Run: `npm run test -- SeatSelectionPage`

Expected: FAIL because the current seat page still uses local selection and does not call `createBookingHold()` on click.

- [ ] **Step 4: Keep the setup file minimal**

```ts
import "@testing-library/jest-dom/vitest";
```

- [ ] **Step 5: Run the test command again to ensure harness works**

Run: `npm run test -- SeatSelectionPage`

Expected: FAIL on product behavior, not on missing test infrastructure.

- [ ] **Step 6: Commit**

```bash
git add FRONTEND/package.json FRONTEND/vitest.config.ts FRONTEND/src/test/setup.ts FRONTEND/src/features/movies/pages/__tests__/SeatSelectionPage.test.tsx
git commit -m "test: add movies frontend test harness and seat-page specs"
```

## Task 6: Implement Per-Seat Hold Creation, Release, and Timers on Seat Page

**Files:**
- Modify: `FRONTEND/src/features/movies/pages/SeatSelectionPage.tsx`
- Modify: `FRONTEND/src/features/movies/api/moviesApi.ts` only if mock/test seams require it
- Test: `FRONTEND/src/features/movies/pages/__tests__/SeatSelectionPage.test.tsx`

- [ ] **Step 1: Replace shared timer state with per-seat hold state**

```tsx
interface SelectedSeatWithHold {
  id: string;
  seatInventoryId: string;
  type: SeatType;
  holdId: string;
  holdCode: string;
  expiresAtUtc: string;
}

const [selectedSeatHolds, setSelectedSeatHolds] = useState<Record<string, SelectedSeatWithHold>>({});
const [nowMs, setNowMs] = useState(() => Date.now());

useEffect(() => {
  const id = window.setInterval(() => setNowMs(Date.now()), 1000);
  return () => window.clearInterval(id);
}, []);
```

- [ ] **Step 2: Make seat click create one-seat holds on demand**

```tsx
const handleSeatClick = async (seat: Seat) => {
  if (seat.status === "selected") {
    const selectedHold = selectedSeatHolds[seat.id];
    if (!selectedHold) return;
    await releaseBookingHold(selectedHold.holdId);
    setSelectedSeatHolds((prev) => {
      const next = { ...prev };
      delete next[seat.id];
      return next;
    });
    return;
  }

  if (seat.status !== "available" || !seat.seatInventoryId || !showtimeId) {
    return;
  }

  const hold = await createBookingHold({
    showtimeId,
    seatInventoryIds: [seat.seatInventoryId],
    snackCombos: [],
    promotionId: undefined,
    sessionId: undefined,
  });

  setSelectedSeatHolds((prev) => ({
    ...prev,
    [seat.id]: {
      id: seat.id,
      seatInventoryId: seat.seatInventoryId!,
      type: seat.type,
      holdId: hold.holdId,
      holdCode: hold.holdCode,
      expiresAtUtc: hold.expiresAtUtc,
    },
  }));
};
```

- [ ] **Step 3: Derive selected seats from hold state and render per-seat countdowns**

```tsx
const selected = useMemo(
  () => Object.values(selectedSeatHolds).map((seat) => ({
    id: seat.id,
    type: seat.type,
    seatInventoryId: seat.seatInventoryId,
  })),
  [selectedSeatHolds],
);

function getRemainingSeconds(expiresAtUtc: string, nowMs: number) {
  return Math.max(0, Math.floor((new Date(expiresAtUtc).getTime() - nowMs) / 1000));
}
```

```tsx
{Object.values(selectedSeatHolds).map((seat) => (
  <div key={seat.id}>
    <span>{seat.id}</span>
    <span>{countdown(getRemainingSeconds(seat.expiresAtUtc, nowMs))}</span>
  </div>
))}
```

- [ ] **Step 4: Expire seats locally when their individual timers hit zero**

```tsx
useEffect(() => {
  const expiredSeatIds = Object.values(selectedSeatHolds)
    .filter((seat) => getRemainingSeconds(seat.expiresAtUtc, nowMs) <= 0)
    .map((seat) => seat.id);

  if (expiredSeatIds.length === 0) return;

  setSelectedSeatHolds((prev) => {
    const next = { ...prev };
    for (const seatId of expiredSeatIds) {
      delete next[seatId];
    }
    return next;
  });

  void refreshSeatMap({ showLoading: false });
}, [nowMs, refreshSeatMap, selectedSeatHolds]);
```

- [ ] **Step 5: Run tests to verify they pass**

Run: `npm run test -- SeatSelectionPage`

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add FRONTEND/src/features/movies/pages/SeatSelectionPage.tsx FRONTEND/src/features/movies/pages/__tests__/SeatSelectionPage.test.tsx
git commit -m "feat: create and release movies seat holds from seat map"
```

## Task 7: Update Checkout Page to Consume Many Hold IDs

**Files:**
- Modify: `FRONTEND/src/features/movies/pages/CheckOutPage.tsx`
- Modify: `FRONTEND/src/features/movies/pages/__tests__/CheckOutPage.test.tsx`
- Modify: `FRONTEND/src/features/movies/api/moviesApi.ts`

- [ ] **Step 1: Write the failing checkout behavior tests**

```tsx
it("creates a booking from all selected hold ids", async () => {
  renderCheckoutPage({
    state: {
      seats: [
        { id: "A1", type: "regular", seatInventoryId: "seat-a1", hold: { holdId: "hold-a1", expiresAtUtc: futureIso() } },
        { id: "A2", type: "regular", seatInventoryId: "seat-a2", hold: { holdId: "hold-a2", expiresAtUtc: futureIso() } },
      ],
    },
  });

  await user.click(screen.getByRole("button", { name: /pay/i }));

  expect(createBooking).toHaveBeenCalledWith(
    expect.objectContaining({ holdIds: ["hold-a1", "hold-a2"] }),
  );
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npm run test -- CheckOutPage`

Expected: FAIL because checkout still reads one `holdId` from search params or navigation state.

- [ ] **Step 3: Read hold ids from seat state instead of a single query param**

```tsx
const holdIds = useMemo(
  () =>
    (bookingState?.seats ?? [])
      .map((seat) => seat.hold?.holdId)
      .filter((value): value is string => Boolean(value)),
  [bookingState?.seats],
);
```

- [ ] **Step 4: Submit and release through the multi-hold shape**

```tsx
const booking = await createBooking({
  holdIds,
  customerName: values.fullName,
  customerEmail: values.email,
  customerPhoneNumber: values.phone,
});
```

```tsx
for (const holdId of holdIds) {
  await releaseBookingHold(holdId);
}
```

- [ ] **Step 5: Run tests to verify they pass**

Run: `npm run test -- CheckOutPage`

Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add FRONTEND/src/features/movies/pages/CheckOutPage.tsx FRONTEND/src/features/movies/pages/__tests__/CheckOutPage.test.tsx FRONTEND/src/features/movies/api/moviesApi.ts
git commit -m "feat: create movies checkout bookings from multiple hold ids"
```

## Task 8: End-to-End Verification and Cleanup

**Files:**
- Modify: any files above only if verification exposes defects

- [ ] **Step 1: Run backend booking and hold tests**

Run: `dotnet test BACKEND/ABCDMall.Modules/Movies/ABCDMall.Modules.Movies.Tests/ABCDMall.Modules.Movies.Tests.csproj --filter Booking`

Expected: PASS with hold-service and booking-service coverage green

- [ ] **Step 2: Run frontend targeted tests**

Run: `npm run test -- SeatSelectionPage`

Expected: PASS

Run: `npm run test -- CheckOutPage`

Expected: PASS

- [ ] **Step 3: Run frontend lint and build**

Run: `npm run lint`

Expected: PASS

Run: `npm run build`

Expected: PASS

- [ ] **Step 4: Smoke-check the manual flow**

Run the app and verify:

- selecting `A1` immediately calls the hold endpoint
- selecting `A2` later shows a later timer than `A1`
- clicking `A1` again releases only `A1`
- refreshing the page causes previously held seats to appear as backend `Held`
- checkout creates one booking from all remaining held seats

- [ ] **Step 5: Commit final fixes**

```bash
git add BACKEND FRONTEND
git commit -m "feat: implement movies per-seat hold flow"
```

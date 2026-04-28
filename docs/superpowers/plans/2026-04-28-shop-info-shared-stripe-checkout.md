# Shop Info Shared Stripe Checkout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Route `/shop-info` rental bill payments through the shared Stripe checkout API instead of the rental-specific checkout-session endpoint.

**Architecture:** Keep rental bill authorization, bill lookup, Stripe session persistence, and webhook completion in the Users module. Add a shared payments API action that delegates rental Stripe checkout creation to the rental payment service and returns the same checkout response contract already used by payment flows.

**Tech Stack:** React, Vitest, ASP.NET Core Web API, xUnit, Stripe.net

---

### Task 1: Switch Shop Info to the shared payment route

**Files:**
- Modify: `FRONTEND/src/features/auth/pages/ShopInfo.test.jsx`
- Modify: `FRONTEND/src/features/auth/pages/ShopInfo.jsx`

- [ ] **Step 1: Write the failing test**

```jsx
await waitFor(() => {
  expect(postMock).toHaveBeenCalledWith("/payments/checkout-session/stripe/rental-bills", {
    billId: "bill-1",
  });
});
```

- [ ] **Step 2: Run test to verify it fails**

Run: `npm --prefix FRONTEND test -- src/features/auth/pages/ShopInfo.test.jsx`
Expected: FAIL because the component still posts to `/RentalPayments/bill-1/checkout-session`.

- [ ] **Step 3: Write minimal implementation**

```jsx
const result = await api.post("/payments/checkout-session/stripe/rental-bills", {
  billId,
});
```

- [ ] **Step 4: Run test to verify it passes**

Run: `npm --prefix FRONTEND test -- src/features/auth/pages/ShopInfo.test.jsx`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add FRONTEND/src/features/auth/pages/ShopInfo.jsx FRONTEND/src/features/auth/pages/ShopInfo.test.jsx
git commit -m "feat: route shop info checkout through shared payments api"
```

### Task 2: Add shared rental-bill Stripe checkout API

**Files:**
- Modify: `BACKEND/ABCDMall.WebAPI/Controllers/PaymentsController.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/Services/RentalPayments/IRentalPaymentService.cs`
- Modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure/Services/RentalPayments/RentalPaymentService.cs`
- Create or modify: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application/DTOs/RentalPayments/CreateRentalStripeCheckoutSessionRequestDto.cs`
- Test: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/RentalPaymentServiceTests.cs`

- [ ] **Step 1: Write the failing backend test**

```csharp
[Fact]
public async Task CreateCheckoutSessionAsync_returns_checkout_url_for_shared_payments_flow()
{
    var result = await service.CreateCheckoutSessionAsync("bill-1", "manager-1", "shop-1");

    Assert.Equal(ApplicationResultStatus.Ok, result.Status);
    Assert.Equal("https://checkout.stripe.test/session", result.Value!.CheckoutUrl);
}
```

- [ ] **Step 2: Run test to verify it fails or does not cover the shared controller path**

Run: `dotnet test BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/ABCDMall.Modules.Users.Tests.csproj --filter RentalPaymentServiceTests`
Expected: FAIL or missing controller entrypoint coverage for the shared route.

- [ ] **Step 3: Write minimal implementation**

```csharp
[HttpPost("checkout-session/stripe/rental-bills")]
[Authorize(Roles = "Manager")]
public async Task<IActionResult> CreateRentalBillStripeCheckoutSession(
    [FromBody] CreateRentalStripeCheckoutSessionRequestDto request,
    CancellationToken cancellationToken)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
    var shopId = User.FindFirstValue("shopId");
    var result = await _rentalPaymentService.CreateCheckoutSessionAsync(request.BillId, userId!, shopId, cancellationToken);
    return result.Status switch
    {
        ApplicationResultStatus.Ok => Ok(result.Value),
        ApplicationResultStatus.BadRequest => BadRequest(result.Error),
        ApplicationResultStatus.NotFound => NotFound(result.Error),
        ApplicationResultStatus.Unauthorized => Unauthorized(result.Error),
        _ => StatusCode(StatusCodes.Status500InternalServerError)
    };
}
```

- [ ] **Step 4: Run tests to verify they pass**

Run: `dotnet test BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/ABCDMall.Modules.Users.Tests.csproj --filter RentalPaymentServiceTests`
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add BACKEND/ABCDMall.WebAPI/Controllers/PaymentsController.cs BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Application BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Infrastructure BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/RentalPaymentServiceTests.cs
git commit -m "feat: add shared stripe checkout endpoint for rental bills"
```

### Task 3: Verify no regression in existing payment flows

**Files:**
- Modify: none unless failures require targeted fixes
- Test: `FRONTEND/src/features/auth/pages/ShopInfo.test.jsx`
- Test: `BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/RentalPaymentServiceTests.cs`

- [ ] **Step 1: Run targeted frontend test suite**

```bash
npm --prefix FRONTEND test -- src/features/auth/pages/ShopInfo.test.jsx
```

- [ ] **Step 2: Run targeted backend test suite**

```bash
dotnet test BACKEND/ABCDMall.Modules/Users/ABCDMall.Modules.Users.Tests/ABCDMall.Modules.Users.Tests.csproj --filter RentalPaymentServiceTests
```

- [ ] **Step 3: Run existing shared payments controller tests if present, otherwise note gap**

```bash
dotnet test BACKEND/ABCDMall.WebAPI/ABCDMall.WebAPI.csproj
```

- [ ] **Step 4: Confirm expected outputs**

Expected: `ShopInfo.test.jsx` passes, `RentalPaymentServiceTests` pass, and no compile errors are introduced in WebAPI.

- [ ] **Step 5: Commit**

```bash
git add .
git commit -m "test: verify shared stripe checkout for shop info"
```

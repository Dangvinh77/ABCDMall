using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Domain.Entities;
using ABCDMall.Modules.Users.Infrastructure;
using ABCDMall.Modules.Users.Infrastructure.Options;
using ABCDMall.Modules.Users.Infrastructure.Services.RentalPayments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Xunit;

namespace ABCDMall.Modules.Users.Tests;

public class RentalPaymentServiceTests
{
    [Fact]
    public async Task CreateCheckoutSessionAsync_returns_checkout_url_and_persists_session_id_for_managers_own_unpaid_bill()
    {
        await using var context = CreateContext();
        context.Users.Add(new User
        {
            Id = "manager-1",
            Email = "manager@example.com",
            FullName = "Stripe Manager",
            Role = "Manager",
            IsActive = true
        });
        context.ShopMonthlyBills.Add(new ShopMonthlyBill
        {
            Id = "bill-1",
            ShopInfoId = "shop-1",
            BillKey = "bill-key-1",
            ShopName = "Book World",
            RentalLocation = "A1-01",
            Month = "04/2026",
            UsageMonth = "03/2026",
            BillingMonthKey = "2026-04",
            UsageMonthKey = "2026-03",
            LeaseStartDate = new DateTime(2026, 4, 1),
            ElectricityUsage = "100",
            WaterUsage = "30",
            ElectricityFee = 150m,
            WaterFee = 50m,
            ServiceFee = 75m,
            TotalDue = 275m,
            PaymentStatus = "Unpaid"
        });
        await context.SaveChangesAsync();

        var service = new RentalPaymentService(
            context,
            Options.Create(new StripeSettings
            {
                SecretKey = "sk_test_123",
                FrontendBaseUrl = "http://localhost:5173"
            }),
            new FakeStripeCheckoutClient("cs_test_123", "https://checkout.stripe.test/session"));

        var result = await service.CreateCheckoutSessionAsync("bill-1", "manager-1", "shop-1");

        Assert.Equal(ApplicationResultStatus.Ok, result.Status);
        Assert.NotNull(result.Value);
        Assert.Equal("cs_test_123", result.Value!.SessionId);
        Assert.Equal("https://checkout.stripe.test/session", result.Value.CheckoutUrl);
        Assert.Equal("cs_test_123", context.ShopMonthlyBills.Single().StripeSessionId);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_rejects_bill_that_does_not_belong_to_manager_shop()
    {
        await using var context = CreateContext();
        context.ShopMonthlyBills.Add(new ShopMonthlyBill
        {
            Id = "bill-2",
            ShopInfoId = "shop-2",
            BillKey = "bill-key-2",
            ShopName = "Toy Land",
            RentalLocation = "B1-02",
            Month = "04/2026",
            UsageMonth = "03/2026",
            BillingMonthKey = "2026-04",
            UsageMonthKey = "2026-03",
            LeaseStartDate = new DateTime(2026, 4, 1),
            ElectricityUsage = "120",
            WaterUsage = "40",
            ElectricityFee = 200m,
            WaterFee = 60m,
            ServiceFee = 90m,
            TotalDue = 350m,
            PaymentStatus = "Unpaid"
        });
        await context.SaveChangesAsync();

        var checkoutClient = new FakeStripeCheckoutClient("cs_test_456", "https://checkout.stripe.test/unauthorized");
        var service = new RentalPaymentService(
            context,
            Options.Create(new StripeSettings
            {
                SecretKey = "sk_test_123",
                FrontendBaseUrl = "http://localhost:5173"
            }),
            checkoutClient);

        var result = await service.CreateCheckoutSessionAsync("bill-2", "manager-2", "shop-9");

        Assert.Equal(ApplicationResultStatus.Unauthorized, result.Status);
        Assert.False(checkoutClient.CreateSessionCalled);
        Assert.Null(context.ShopMonthlyBills.Single().StripeSessionId);
    }

    private static MallDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<MallDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new MallDbContext(options);
    }

    private sealed class FakeStripeCheckoutClient : IStripeCheckoutClient
    {
        private readonly string _sessionId;
        private readonly string _checkoutUrl;

        public FakeStripeCheckoutClient(string sessionId, string checkoutUrl)
        {
            _sessionId = sessionId;
            _checkoutUrl = checkoutUrl;
        }

        public bool CreateSessionCalled { get; private set; }

        public Task<StripeCheckoutSessionResult> CreateSessionAsync(StripeCheckoutSessionRequest request, CancellationToken cancellationToken = default)
        {
            CreateSessionCalled = true;
            return Task.FromResult(new StripeCheckoutSessionResult
            {
                SessionId = _sessionId,
                CheckoutUrl = _checkoutUrl
            });
        }
    }
}

using ABCDMall.Modules.Users.Application.Common;
using ABCDMall.Modules.Users.Application.DTOs.Bidding;
using ABCDMall.Modules.Users.Application.Services;
using ABCDMall.Modules.Users.Domain.Enums;

namespace ABCDMall.Modules.Users.Application.Services.Bidding;

public sealed class BidPaymentService : IBidPaymentService
{
    private const string PaymentCurrency = "USD";
    private const string CheckoutCompletedEvent = "checkout.session.completed";

    private readonly IBiddingRepository _repository;
    private readonly IBidStripePaymentGateway _stripePaymentGateway;
    private readonly IEmailNotificationService _emailNotificationService;

    public BidPaymentService(
        IBiddingRepository repository,
        IBidStripePaymentGateway stripePaymentGateway,
        IEmailNotificationService emailNotificationService)
    {
        _repository = repository;
        _stripePaymentGateway = stripePaymentGateway;
        _emailNotificationService = emailNotificationService;
    }

    public async Task<ApplicationResult<BidPaymentCheckoutSessionDto>> CreateCheckoutSessionAsync(
        string bidId,
        string shopId,
        CancellationToken cancellationToken = default)
    {
        var bid = await _repository.GetBidByIdAsync(bidId, cancellationToken);
        if (bid is null)
        {
            return ApplicationResult<BidPaymentCheckoutSessionDto>.NotFound("Bid not found.");
        }

        if (!string.Equals(bid.ShopId, shopId, StringComparison.OrdinalIgnoreCase))
        {
            return ApplicationResult<BidPaymentCheckoutSessionDto>.Unauthorized("You cannot pay for another shop's bid.");
        }

        if (bid.Status != CarouselBidStatus.Won)
        {
            return ApplicationResult<BidPaymentCheckoutSessionDto>.BadRequest("Only won bids can proceed to payment.");
        }

        var upcomingMonday = BiddingBusinessClock.GetUpcomingWeekMonday(DateTime.UtcNow);
        if (bid.TargetMondayDate != upcomingMonday)
        {
            return ApplicationResult<BidPaymentCheckoutSessionDto>.BadRequest("Only the upcoming week's won bids can be paid.");
        }

        var expiresAtLocal = bid.TargetMondayDate.AddMinutes(-1);
        var expiresAtUtc = BiddingBusinessClock.ConvertLocalToUtc(expiresAtLocal);
        if (expiresAtUtc <= DateTime.UtcNow)
        {
            return ApplicationResult<BidPaymentCheckoutSessionDto>.BadRequest("Payment window has expired.");
        }

        var managerContact = await _repository.GetManagerContactByShopIdAsync(shopId, cancellationToken);
        var session = await _stripePaymentGateway.CreateCheckoutSessionAsync(new BidStripeCheckoutSessionRequest
        {
            BidId = bid.Id ?? string.Empty,
            ShopId = bid.ShopId,
            ShopName = managerContact?.ShopName ?? bid.ShopId,
            TemplateType = bid.TemplateType.ToString(),
            BidAmount = bid.BidAmount,
            Currency = PaymentCurrency,
            CustomerEmail = managerContact?.Email,
            ExpiresAtUtc = expiresAtUtc
        }, cancellationToken);

        return ApplicationResult<BidPaymentCheckoutSessionDto>.Ok(new BidPaymentCheckoutSessionDto
        {
            BidId = bid.Id ?? string.Empty,
            SessionId = session.SessionId,
            CheckoutUrl = session.CheckoutUrl,
            ExpiresAtUtc = session.ExpiresAtUtc
        });
    }

    public async Task ProcessWebhookAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken = default)
    {
        var webhookEvent = _stripePaymentGateway.ParseWebhookEvent(payload, signatureHeader);
        if (!string.Equals(webhookEvent.EventType, CheckoutCompletedEvent, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(webhookEvent.BidId)
            || string.IsNullOrWhiteSpace(webhookEvent.ProviderTransactionId)
            || !webhookEvent.Amount.HasValue)
        {
            return;
        }

        var bid = await _repository.GetBidByIdAsync(webhookEvent.BidId, cancellationToken)
            ?? throw new InvalidOperationException("Bid not found.");

        if (bid.Status == CarouselBidStatus.Paid || bid.Status == CarouselBidStatus.Active)
        {
            return;
        }

        if (bid.Status != CarouselBidStatus.Won)
        {
            throw new InvalidOperationException("Bid is not eligible for payment completion.");
        }

        if (!decimal.Equals(decimal.Round(bid.BidAmount, 2), decimal.Round(webhookEvent.Amount.Value, 2)))
        {
            throw new InvalidOperationException("Payment amount does not match bid amount.");
        }

        if (!string.IsNullOrWhiteSpace(webhookEvent.Currency)
            && !string.Equals(webhookEvent.Currency, PaymentCurrency, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Payment currency does not match bidding currency.");
        }

        bid.Status = CarouselBidStatus.Paid;
        await _repository.SaveChangesAsync(cancellationToken);

        var managerContact = await _repository.GetManagerContactByShopIdAsync(bid.ShopId, cancellationToken);
        if (!string.IsNullOrWhiteSpace(managerContact?.Email))
        {
            await _emailNotificationService.SendCarouselBidPaymentSuccessEmailAsync(
                managerContact.Email,
                managerContact.FullName,
                managerContact.ShopName,
                bid.BidAmount,
                bid.TargetMondayDate);
        }
    }
}

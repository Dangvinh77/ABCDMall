using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using ABCDMall.Modules.Movies.Application.DTOs.Promotions;
using ABCDMall.Modules.Movies.Application.Services.Promotions;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings;

public sealed class BookingQuoteService : IBookingQuoteService
{
    private const decimal ServiceFeePerSeat = 10000m;

    private readonly IShowtimeReadRepository _showtimeReadRepository;
    private readonly ISeatInventoryReadRepository _seatInventoryReadRepository;
    private readonly IPromotionRepository _promotionRepository;
    private readonly IPromotionEvaluationService _promotionEvaluationService;

    public BookingQuoteService(
        IShowtimeReadRepository showtimeReadRepository,
        ISeatInventoryReadRepository seatInventoryReadRepository,
        IPromotionRepository promotionRepository,
        IPromotionEvaluationService promotionEvaluationService)
    {
        _showtimeReadRepository = showtimeReadRepository;
        _seatInventoryReadRepository = seatInventoryReadRepository;
        _promotionRepository = promotionRepository;
        _promotionEvaluationService = promotionEvaluationService;
    }

    public async Task<BookingQuoteResponseDto> QuoteAsync(
        BookingQuoteRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var showtime = await _showtimeReadRepository.GetShowtimeByIdAsync(request.ShowtimeId, cancellationToken);
        if (showtime is null)
        {
            throw new InvalidOperationException("Showtime not found.");
        }

        var seats = await _seatInventoryReadRepository.GetSeatsByIdsAsync(
            request.ShowtimeId,
            request.SeatInventoryIds,
            cancellationToken);

        if (seats.Count != request.SeatInventoryIds.Count)
        {
            throw new InvalidOperationException("One or more selected seats were not found.");
        }

        var unavailableSeats = seats
            .Where(x => !string.Equals(x.Status, "Available", StringComparison.OrdinalIgnoreCase))
            .Select(x => x.SeatCode)
            .ToArray();

        if (unavailableSeats.Length > 0)
        {
            throw new InvalidOperationException($"Selected seats are not available: {string.Join(", ", unavailableSeats)}.");
        }

        ValidateCoupleSeatSelection(seats);

        var lines = new List<BookingQuoteLineDto>();

        var seatSubtotal = 0m;
        foreach (var seat in seats.OrderBy(x => x.Row).ThenBy(x => x.Col))
        {
            seatSubtotal += seat.Price;
            lines.Add(new BookingQuoteLineDto
            {
                Type = "Seat",
                Code = seat.SeatCode,
                Label = $"Seat {seat.SeatCode}",
                Amount = seat.Price
            });
        }

        var serviceFeeTotal = ServiceFeePerSeat * seats.Count;
        if (serviceFeeTotal > 0)
        {
            lines.Add(new BookingQuoteLineDto
            {
                Type = "Fee",
                Code = "SERVICE_FEE",
                Label = "Service fee",
                Amount = serviceFeeTotal
            });
        }

        var comboSubtotal = 0m;
        if (request.SnackCombos.Count > 0)
        {
            var comboIds = request.SnackCombos.Select(x => x.ComboId).Distinct().ToArray();
            var combos = await _promotionRepository.GetSnackCombosByIdsAsync(comboIds, cancellationToken);
            var comboLookup = combos.ToDictionary(x => x.Id, x => x);

            foreach (var selectedCombo in request.SnackCombos)
            {
                if (!comboLookup.TryGetValue(selectedCombo.ComboId, out var combo))
                {
                    throw new InvalidOperationException($"Snack combo {selectedCombo.ComboId} was not found or inactive.");
                }

                var lineAmount = combo.Price * selectedCombo.Quantity;
                comboSubtotal += lineAmount;

                lines.Add(new BookingQuoteLineDto
                {
                    Type = "Combo",
                    Code = combo.Code,
                    Label = $"{combo.Name} x{selectedCombo.Quantity}",
                    Amount = lineAmount
                });
            }
        }

        var discountTotal = 0m;
        BookingQuotePromotionDto? promotion = null;

        if (request.PromotionId.HasValue)
        {
            var promotionRequest = new EvaluatePromotionRequestDto
            {
                PromotionId = request.PromotionId.Value,
                ShowtimeId = request.ShowtimeId,
                SeatInventoryIds = seats.Select(x => x.SeatInventoryId).ToArray(),
                SeatTypes = seats.Select(x => x.SeatType).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                SeatSubtotal = seatSubtotal,
                ComboSubtotal = comboSubtotal,
                PaymentProvider = request.PaymentProvider,
                Birthday = request.Birthday,
                GuestCustomerId = request.GuestCustomerId,
                BusinessDate = showtime.BusinessDate,
                SnackCombos = request.SnackCombos
                    .Select(x => new EvaluatePromotionComboDto
                    {
                        ComboId = x.ComboId,
                        Quantity = x.Quantity
                    })
                    .ToArray()
            };

            var promotionResult = await _promotionEvaluationService.EvaluateAsync(promotionRequest, cancellationToken);
            discountTotal = promotionResult.IsEligible ? promotionResult.DiscountAmount : 0m;

            promotion = new BookingQuotePromotionDto
            {
                PromotionId = promotionResult.PromotionId,
                PromotionCode = promotionResult.PromotionCode,
                Status = promotionResult.Status.ToString(),
                IsEligible = promotionResult.IsEligible,
                Message = promotionResult.Message,
                DiscountAmount = discountTotal
            };

            if (discountTotal > 0)
            {
                lines.Add(new BookingQuoteLineDto
                {
                    Type = "Discount",
                    Code = string.IsNullOrWhiteSpace(promotionResult.PromotionCode)
                        ? promotionResult.PromotionId.ToString()
                        : promotionResult.PromotionCode,
                    Label = "Promotion discount",
                    Amount = -discountTotal
                });
            }
        }

        var grandTotal = seatSubtotal + serviceFeeTotal + comboSubtotal - discountTotal;
        if (grandTotal < 0)
        {
            grandTotal = 0m;
        }

        return new BookingQuoteResponseDto
        {
            ShowtimeId = request.ShowtimeId,
            SeatSubtotal = decimal.Round(seatSubtotal, 2, MidpointRounding.AwayFromZero),
            ServiceFeeTotal = decimal.Round(serviceFeeTotal, 2, MidpointRounding.AwayFromZero),
            ComboSubtotal = decimal.Round(comboSubtotal, 2, MidpointRounding.AwayFromZero),
            DiscountTotal = decimal.Round(discountTotal, 2, MidpointRounding.AwayFromZero),
            GrandTotal = decimal.Round(grandTotal, 2, MidpointRounding.AwayFromZero),
            Promotion = promotion,
            Lines = lines
        };
    }

    private static void ValidateCoupleSeatSelection(IReadOnlyCollection<Models.SeatInventoryQuoteSnapshot> seats)
    {
        var invalidCoupleGroups = seats
            .Where(x => string.Equals(x.SeatType, "Couple", StringComparison.OrdinalIgnoreCase))
            .GroupBy(x => x.CoupleGroupCode)
            .Where(group => string.IsNullOrWhiteSpace(group.Key) || group.Count() != 2)
            .Select(group => group.Key ?? "(missing-group)")
            .ToArray();

        if (invalidCoupleGroups.Length > 0)
        {
            throw new InvalidOperationException("Couple seats must be selected as a full pair.");
        }
    }
}

namespace ABCDMall.Modules.Movies.Application.DTOs.Admin;

public sealed class MoviesAdminBookingListItemDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public Guid ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;
    public DateTime ShowtimeStartAtUtc { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class MoviesAdminBookingDetailDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public Guid ShowtimeId { get; set; }
    public string MovieTitle { get; set; } = string.Empty;
    public string CinemaName { get; set; } = string.Empty;
    public string HallName { get; set; } = string.Empty;
    public DateOnly BusinessDate { get; set; }
    public DateTime StartAtUtc { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhoneNumber { get; set; } = string.Empty;
    public decimal SeatSubtotal { get; set; }
    public decimal ComboSubtotal { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string? ProviderTransactionId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public IReadOnlyList<MoviesAdminBookingItemDto> Items { get; set; } = [];
}

public sealed class MoviesAdminBookingItemDto
{
    public string ItemType { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

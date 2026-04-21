namespace ABCDMall.Modules.Movies.Application.DTOs.Admin;

public class MoviesAdminPaymentListItemDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? ProviderTransactionId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
}

public sealed class MoviesAdminPaymentDetailDto : MoviesAdminPaymentListItemDto
{
    public string? FailureReason { get; set; }
    public string? CallbackPayloadJson { get; set; }
}

public sealed class MoviesAdminEmailLogItemDto
{
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string MovieTitle { get; set; } = string.Empty;
    public string DeliveryStatus { get; set; } = string.Empty;
    public string? PdfFileName { get; set; }
    public DateTime IssuedAtUtc { get; set; }
    public DateTime? EmailSentAtUtc { get; set; }
    public string? EmailSendError { get; set; }
    public string OutboxStatus { get; set; } = string.Empty;
    public int OutboxRetryCount { get; set; }
    public string? OutboxLastError { get; set; }
}

public sealed class MoviesAdminRevenueReportDto
{
    public DateTime? DateFromUtc { get; set; }
    public DateTime? DateToUtc { get; set; }
    public decimal TotalPaidRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int SuccessfulPayments { get; set; }
    public int FailedPayments { get; set; }
    public IReadOnlyList<MoviesAdminRevenueBreakdownDto> ByMovie { get; set; } = [];
    public IReadOnlyList<MoviesAdminRevenueBreakdownDto> ByCinema { get; set; } = [];
    public IReadOnlyList<MoviesAdminRevenueBreakdownDto> ByProvider { get; set; } = [];
}

public sealed class MoviesAdminRevenueBreakdownDto
{
    public string Label { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

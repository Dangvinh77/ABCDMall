namespace ABCDMall.Modules.Movies.Application.DTOs.Promotions;

public sealed class EvaluatePromotionRequestDto
{
    // PromotionId la khoa chuan de backend load dung promotion can evaluate.
    public Guid PromotionId { get; set; }

    // ShowtimeId giu dung contract chung voi Dev 1.
    public Guid ShowtimeId { get; set; }

    // Danh sach ghe duoc gui len de test rule seat-count va reserve shape request cho Day 4.
    public IReadOnlyCollection<Guid> SeatInventoryIds { get; set; } = Array.Empty<Guid>();

    // SeatTypes phuc vu nhom rule theo loai ghe, vi du couple seat.
    public IReadOnlyCollection<string> SeatTypes { get; set; } = Array.Empty<string>();
    public string? PaymentProvider { get; set; }

    // Birthday duoc dung cho nhom khuyen mai sinh nhat.
    public DateOnly? Birthday { get; set; }
    public decimal SeatSubtotal { get; set; }
    public decimal ComboSubtotal { get; set; }

    // SnackCombos chua comboId va quantity de rule Combo co the evaluate.
    public IReadOnlyCollection<EvaluatePromotionComboDto> SnackCombos { get; set; } = Array.Empty<EvaluatePromotionComboDto>();
    public string? CouponCode { get; set; }

    // BusinessDate giup backend xac dinh booking co nam vao cuoi tuan hay khong.
    public DateOnly? BusinessDate { get; set; }

    // ShowtimeStartAtUtc giup evaluate rule theo khung gio suat chieu, vi du Morning/Early Bird.
    public DateTime? ShowtimeStartAtUtc { get; set; }

    // GuestCustomerId la thong tin toi thieu de enforce MaxRedemptionsPerCustomer.
    public Guid? GuestCustomerId { get; set; }
}

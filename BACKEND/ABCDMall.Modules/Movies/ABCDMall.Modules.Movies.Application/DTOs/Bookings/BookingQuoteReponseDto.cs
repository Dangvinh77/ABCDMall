using ABCDMall.Modules.Movies.Application.DTOs.Bookings;

public sealed class BookingQuoteResponseDto
{
    //Day la toan bo du lieu ma backend tra ve sau khi thuc hien quote, frontend se dung du lieu nay de hien thi hoa don tam tinh cho khach xem
    public Guid ShowtimeId { get; set; } //Id cua suat chieu ma khach muon dat
    public decimal SeatSubtotal { get; set; } //Tong tien cua cac ghe
    public decimal ServiceFeeTotal { get; set; } //Tong tien phi dich vu
    public decimal ComboSubtotal { get; set; } //Tong tien cua cac combo an vat
    public decimal DiscountTotal { get; set; } //Tong tien giam gia
    public decimal GrandTotal { get; set; } //Tong tien phai tra sau khi tinh tat ca cac khoan
    public BookingQuotePromotionDto? Promotion { get; set; } //Thong tin ve promotion duoc ap dung, neu co. Neu khong co promotion duoc ap dung thi de null
    public IReadOnlyCollection<BookingQuoteLineDto> Lines { get; set; } = Array.Empty<BookingQuoteLineDto>();//Danh sach dong chi tiet de hien thi hoa don
}
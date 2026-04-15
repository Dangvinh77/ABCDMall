public sealed class ShowtimeQuoteSnapshot
{
    //snapshot la 1 ban du lieu duoc cat ra tu DB de phuc vu use case, khong phai entity day du
    //snapshot nay la cua showtime ma service can de quote.

    public Guid ShowtimeId { get; set; } //Id cua suat chieu
    public Guid MovieId { get; set; } //Id cua phim
    public Guid CinemaId { get; set; } //Id cua rap
    public Guid HallId { get; set; } //Id cua phong chieu
    public string HallType { get; set; } = string.Empty; //Loai phong chieu
    public DateOnly BusinessDate { get; set; } //Ngay kinh doanh
    public DateTime StartAtUtc { get; set; } //Thoi gian bat dau suat chieu
    public decimal BasePrice { get; set; } //Gia co ban cua suat chieu
    public string Status { get; set; } = string.Empty; //Trang thai cua suat chieu
}
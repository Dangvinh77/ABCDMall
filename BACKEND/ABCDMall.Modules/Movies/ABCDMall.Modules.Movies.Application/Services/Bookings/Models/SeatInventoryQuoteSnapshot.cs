public sealed class SeatInventoryQuoteSnapshot
{
    //Day la snapshot cua tung ghe trong showtime
    public Guid SeatInventoryId { get; set; } //Id cua ghe trong kho ghe (seat inventory)
    public Guid ShowtimeId { get; set; } //Id cua suat chieu
    public string SeatCode { get; set; } = string.Empty; //Ma ghe
    public string Row { get; set; } = string.Empty; //Hang ghe
    public int Col { get; set; } //Cot ghe
    public string SeatType { get; set; } = string.Empty; //Loai ghe
    public string Status { get; set; } = string.Empty; //Trang thai ghe
    public decimal Price { get; set; } //Gia cua ghe
    public string? CoupleGroupCode { get; set; } //Ma nhom ghe doi, neu co
}
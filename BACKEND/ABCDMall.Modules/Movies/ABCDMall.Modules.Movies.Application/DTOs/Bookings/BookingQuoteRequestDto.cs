using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Application.DTOs.Bookings
{
    public sealed class BookingQuoteRequestDto
    {
        //day la toan bo du lieu client gui len de thua hien quote
        public Guid ShowtimeId { get; set; } //Id cua suat chieu ma khach muon dat
        public IReadOnlyCollection<Guid> SeatInventoryIds { get; set; } = Array.Empty<Guid>(); //Danh sach id cua cac ghe ma khach muon dat
        public IReadOnlyCollection<BookingQuoteComboItemDto> SnackCombos { get; set; } = Array.Empty<BookingQuoteComboItemDto>();//Danh sach combo an vat ma khach muon mua kem, neu khong mua kem thi de rong
        public Guid? PromotionId { get; set; } //Id cua promotion ma khach muon ap dung, neu khong ap dung thi de null
        public string? PaymentProvider { get; set; } //Nha cung cap dich vu thanh toan ma khach muon su dung
        public DateOnly BirthDay { get; set; } //Ngay sinh cua khach hang, de xac dinh xem co duoc su dung promotion cho thang sinh nhat
        public Guid GuestCustomerId { get; set; } //Id cua khach hang (guest), de xac dinh xem co duoc su dung promotion cho khach hang moi hay khong
    }
}

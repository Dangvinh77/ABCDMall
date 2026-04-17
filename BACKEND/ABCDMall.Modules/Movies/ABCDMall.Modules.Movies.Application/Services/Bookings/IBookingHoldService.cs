using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings
{
    public interface IBookingHoldService
    {
        //interface giúp định nghĩa service của use case hold cần những chức năng gì
        Task<BookingHoldResponseDto> CreateAsync(
        CreateBookingHoldRequestDto request,
        CancellationToken cancellationToken = default); //tạo hold mới, trả về thông tin hold đã tạo

        Task<BookingHoldResponseDto?> GetByIdAsync(
        Guid holdId,
        CancellationToken cancellationToken = default); //lấy thông tin hold theo id, trả về null nếu không tìm thấy

        Task<bool> ReleaseAsync(
        Guid holdId,
        CancellationToken cancellationToken = default); //hủy hold theo id, trả về true nếu hủy thành công, false nếu không tìm thấy hoặc đã hết hạn

        Task<BookingHoldResponseDto?> ConfirmAsync(
        Guid holdId,
        CancellationToken cancellationToken = default); //DAY5 TEST-ONLY CONFIRM FLOW: xác nhận hold tối thiểu để khóa ghế khi test, chưa phải booking/payment hoàn chỉnh
    }
}

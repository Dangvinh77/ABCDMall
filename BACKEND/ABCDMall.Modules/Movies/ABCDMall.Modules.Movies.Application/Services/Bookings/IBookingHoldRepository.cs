using ABCDMall.Modules.Movies.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings
{
    public interface IBookingHoldRepository
    {
        //đây là contract repository để định nghĩa những thao tác với DB mà use case hold cânf dùng
        Task<BookingHold> AddAsync(BookingHold hold, CancellationToken cancellationToken = default);//thêm hold mới vào DB, trả về hold đã được thêm (có thể có id sau khi lưu vào DB)
        Task<BookingHold?> GetByIdAsync(Guid holdId, CancellationToken cancellationToken = default);//lấy hold theo id, trả về null nếu không tìm thấy
        Task<bool> ReleaseAsync(Guid holdId, DateTime utcNow, CancellationToken cancellationToken = default);//hủy hold theo id, trả về true nếu hủy thành công, false nếu không tìm thấy hoặc đã hết hạn
        Task<int> ExpireAsync(DateTime utcNow, CancellationToken cancellationToken = default);//hết hạn các hold, trả về số lượng hold đã hết hạn
        Task<IReadOnlySet<Guid>> GetActiveSeatInventoryIdsAsync(Guid showtimeId, DateTime utcNow, CancellationToken cancellationToken = default);//lấy danh sách ghế đang bị hold active của một suất chiếu để seat-map hiển thị đúng trạng thái
    }
}

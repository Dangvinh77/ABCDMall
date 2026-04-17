using System.Data;
using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Repositories.Bookings
{
    public sealed class BookingHoldRepository: IBookingHoldRepository
    {
        //repository là nơi làm việc trực tiếp với DB thông qua EF
        //hoặc các công cụ truy cập dữ liệu khác, nó sẽ thực hiện các thao tác lưu trữ, truy vấn và cập nhật dữ liệu liên quan đến booking hold
        private readonly MoviesBookingDbContext _dbContext;

        public BookingHoldRepository(MoviesBookingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BookingHold> AddAsync(BookingHold hold, CancellationToken cancellationToken)
        {
            //hàm này thêm 1 booking hold mới vào DB đông thời kiểm tra có các ghế có bị trùng hold hay không
            //mở 1 transaction với isolation level là serializable để đảm bảo tính cô lập, tránh các vấn đề như cùng 1 ghế bị hold bởi nhiều booking hold khác nhau
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            //lấy danh sách các ghế cần giữu
            var seatInventoryIds = hold.Seats.Select(s => s.SeatInventoryId).ToArray();
            var now = DateTime.UtcNow;
            //conflicts để kiểm tra xem có ghế nào trong danh sách bị hold bởi booking hold khác hay không, nếu có thì sẽ trả về lỗi
            var conflicts = await _dbContext.BookingHoldSeats
                .AsNoTracking()
                .Where(seat => seatInventoryIds.Contains(seat.SeatInventoryId) //chỉ lấy những ghế có trong danh sách cần giữ mà user chọn
                     && seat.BookingHold != null 
                     && seat.BookingHold.Status == BookingHoldStatus.Active //chỉ lấy những ghế đang bị hold bởi booking hold khác
                     && seat.BookingHold.ExpiresAtUtc > now) //chỉ lấy những ghế mà booking hold chưa hết hạn
                .Select(seat => seat.SeatCode)
                .Distinct()
                .ToArrayAsync(cancellationToken);
            if (conflicts.Length > 0)
            {
                throw new InvalidOperationException($"Selected seats are already being held: {string.Join(", ", conflicts)}.");
            }
            _dbContext.BookingHolds.Add(hold);//thêm booking hold mới vào DB
            await _dbContext.SaveChangesAsync(cancellationToken);//lưu thay đổi vào DB
            await transaction.CommitAsync(cancellationToken);//commit transaction để hoàn tất quá trình thêm booking hold mới

            return hold;
        }
        public async Task<BookingHold?> GetByIdAsync(Guid holdId, CancellationToken cancellationToken = default)
        {
            //lấy booking hold theo id
            return await _dbContext.BookingHolds
                .AsNoTracking() //không theo dõi đối tượng này trong DbContext vì chỉ cần đọc dữ liệu, không cần cập nhật
                .Include(x => x.Seats) //lấy luôn danh sách các ghế liên quan đến booking hold này để tránh việc phải truy vấn thêm lần nữa khi cần thông tin về ghế
                .FirstOrDefaultAsync(x => x.Id == holdId, cancellationToken);//tìm booking hold có id trùng với holdId, nếu không tìm thấy sẽ trả về null
        }
        public async Task<bool> ReleaseAsync(
        Guid holdId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
        {
            //hàm này sẽ cập nhật trạng thái của booking hold thành Released nếu tìm thấy booking hold có id trùng với holdId và chưa hết hạn
            var hold = await _dbContext.BookingHolds
                .FirstOrDefaultAsync(x => x.Id == holdId && x.ExpiresAtUtc > utcNow, cancellationToken);//tìm booking hold có id trùng với holdId và chưa hết hạn, nếu không tìm thấy sẽ trả về null
            if (hold is null)
            {
                return false;//nếu không tìm thấy booking hold
            }
            if (hold.Status == BookingHoldStatus.Active)
            {
                hold.Status = BookingHoldStatus.Released;
                hold.UpdatedAtUtc = utcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                
            }
            return true;
        }

        public async Task<int> ExpireAsync(DateTime utcNow, CancellationToken cancellationToken = default)
        {
            //hàm cập nhật những hold Active đã hết hạn thành Expired, trả về số lượng hold đã được cập nhật
            var holdsExpired = await _dbContext.BookingHolds
                .Where(x => x.Status == BookingHoldStatus.Active && x.ExpiresAtUtc <= utcNow)
                .Take(100) // Giới hạn số lượng hold được cập nhật để tránh quá tải DB
                .ToListAsync(cancellationToken);//lấy danh sách các booking hold đang ở trạng thái Active nhưng đã hết hạn

            //duyệt qua từng booking hold trong danh sách và cập nhật trạng thái thành Expired
            foreach (var hold in holdsExpired)
            {
                hold.Status = BookingHoldStatus.Expired;
                hold.UpdatedAtUtc = utcNow;
            }
            if (holdsExpired.Count == 0)
            {
                return 0;
            }
            await _dbContext.SaveChangesAsync(cancellationToken);//lưu thay đổi vào DB
            return holdsExpired.Count;
        }

        public async Task<IReadOnlySet<Guid>> GetActiveSeatInventoryIdsAsync(
            Guid showtimeId,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            // Seat-map nằm ở catalog DB, còn hold nằm ở booking DB.
            // Hàm này trả về các ghế đang bị hold active để tầng query phủ trạng thái Held lên response.
            var seatInventoryIds = await _dbContext.BookingHoldSeats
                .AsNoTracking()
                .Where(seat => seat.BookingHold != null
                    && seat.BookingHold.ShowtimeId == showtimeId
                    && seat.BookingHold.Status == BookingHoldStatus.Active
                    && seat.BookingHold.ExpiresAtUtc > utcNow)
                .Select(seat => seat.SeatInventoryId)
                .Distinct()
                .ToListAsync(cancellationToken);

            return seatInventoryIds.ToHashSet();
        }

        public async Task<BookingHold?> ConvertAsync(
            Guid holdId,
            DateTime utcNow,
            CancellationToken cancellationToken = default)
        {
            // DAY5 TEST-ONLY CONFIRM FLOW:
            // Chỉ đổi trạng thái hold sang Converted để test. Flow booking hoàn chỉnh nên có aggregate/order riêng.
            var hold = await _dbContext.BookingHolds
                .Include(x => x.Seats)
                .FirstOrDefaultAsync(x => x.Id == holdId, cancellationToken);

            if (hold is null)
            {
                return null;
            }

            if (hold.Status != BookingHoldStatus.Active)
            {
                throw new InvalidOperationException($"Booking hold is already {hold.Status}.");
            }

            if (hold.ExpiresAtUtc <= utcNow)
            {
                hold.Status = BookingHoldStatus.Expired;
                hold.UpdatedAtUtc = utcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
                throw new InvalidOperationException("Booking hold has expired.");
            }

            hold.Status = BookingHoldStatus.Converted;
            hold.UpdatedAtUtc = utcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return hold;
        }
    }
}

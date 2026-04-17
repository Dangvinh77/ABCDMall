using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Showtimes;
using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Application.Services.Bookings
{
    public sealed class BookingHoldService : IBookingHoldService
    {
        //service xử lí logic để giữ ghế cho khách hàng, tương tác với các repository để kiểm tra tình trạng ghế và tạo hold
        //service không trực tiếp thao tác với database mà sẽ gọi repository để thực hiện các thao tác lưu trữ, điều này giúp tách biệt giữa logic nghiệp vụ và logic truy cập dữ liệu, dễ dàng bảo trì và mở rộng sau này
        private static readonly TimeSpan HoldDuration = TimeSpan.FromMinutes(10); //thời gian giữ ghế mặc định là 10 phút

        private readonly IShowtimeRepository _showtimeRepository; //lấy thông tin showtime để kiểm tra tình trạng ghế
        private readonly IBookingQuoteService _bookingQuoteService; //dịch vụ để tính toán giá trị đặt chỗ
        private readonly IBookingHoldRepository _bookingHoldRepository; //repository để quản lý hold

        public BookingHoldService(IShowtimeRepository showtimeRepository, IBookingQuoteService bookingQuoteService, IBookingHoldRepository bookingHoldRepository)
        {
            _showtimeRepository = showtimeRepository;
            _bookingQuoteService = bookingQuoteService;
            _bookingHoldRepository = bookingHoldRepository;
        }

        public async Task<BookingHoldResponseDto> CreateAsync(CreateBookingHoldRequestDto request, CancellationToken cancellationToken = default)
        {
            //lấy thông tin showtime
            var showtime = await _showtimeRepository.GetShowtimeByIdAsync(request.ShowtimeId, cancellationToken);
            //kiemr tra showtime có tồn tại và đang mở bán không
            if (showtime == null)
            {
                throw new Exception("Showtime not found");
            }
            if (showtime.Status != ShowtimeStatus.Open)
            {
                throw new Exception("Showtime is not open for booking");
            }
            //lấy dữ liệu toàn bộ ghế
            var seatMap = await _showtimeRepository.GetSeatMapByShowtimeIdAsync(request.ShowtimeId, cancellationToken);
            //lọc các ghế user chọn
            var selectedSeats = seatMap
                .Where(s => request.SeatInventoryIds.Contains(s.Id)).ToList()
                .OrderBy(x => x.RowLabel) //sắp xếp theo hàng
                .ThenBy(x => x.ColumnNumber) //sắp xếp theo cột
                .ToList();//đảm bảo thứ tự ghế được giữ nguyên như user chọn
            //kiểm tra số lượng ghế tìm được so với số lượng ghế request gửi lên
            if (selectedSeats.Count != request.SeatInventoryIds.Count)
            {
                throw new Exception("Some selected seats are not available");
            }
            //lọc các ghế không có trạng thái Available 
            var unavailableSeats = selectedSeats
                .Where(x => x.Status != SeatInventoryStatus.Available)
                .Select(x => x.SeatCode)
                .ToArray();
            if (unavailableSeats.Length > 0)
            {
                throw new InvalidOperationException($"Selected seats are not available: {string.Join(", ", unavailableSeats)}.");
            }
            //gọi quote service để tính giá
            //service này không thực hiện tính giá , chỉ thực hiện hold nên gọi BookingQuoteService để lấy thông tin giá cả, khuyến mãi áp dụng,... mà không cần quan tâm đến giá trị cụ thể
            var quote = await _bookingQuoteService.QuoteAsync(new BookingQuoteRequestDto
            {
                ShowtimeId = request.ShowtimeId,
                SeatInventoryIds = request.SeatInventoryIds,
                SnackCombos = request.SnackCombos,
                PromotionId = request.PromotionId,
                PaymentProvider = request.PaymentProvider,
                Birthday = request.Birthday,
                GuestCustomerId = request.GuestCustomerId
            }, cancellationToken);

            var now = DateTime.UtcNow;
            //tạo entity BookingHold để lưu vào database
            var hold = new BookingHold
            {
                Id = Guid.NewGuid(),
                HoldCode = GenerateHoldCode(now),
                ShowtimeId = request.ShowtimeId,
                Status = BookingHoldStatus.Active,
                ExpiresAtUtc = now.Add(HoldDuration),
                SessionId = request.SessionId,
                SeatSubtotal = quote.SeatSubtotal,
                ComboSubtotal = quote.ComboSubtotal,
                DiscountAmount = quote.DiscountTotal,
                GrandTotal = quote.GrandTotal,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                //tạo bản ghi BookingHoldSeat cho từng ghế được giữ
                Seats = selectedSeats.Select(seat => new BookingHoldSeat
                {
                    Id = Guid.NewGuid(),
                    SeatInventoryId = seat.Id,
                    SeatCode = seat.SeatCode,
                    SeatType = seat.SeatType.ToString(),
                    UnitPrice = seat.Price,
                    CoupleGroupCode = seat.CoupleGroupCode
                }).ToList()
            };
            var created = await _bookingHoldRepository.AddAsync(hold, cancellationToken);
            return Map(created);

        }

        public async Task<BookingHoldResponseDto?> GetByIdAsync(Guid holdId, CancellationToken cancellationToken = default)
        {
            var hold = await _bookingHoldRepository.GetByIdAsync(holdId, cancellationToken);//lấy thông tin hold theo id, gọi repository để truy vấn database
            if (hold is null)
            {
                return null;
            }

            //nếu hold đang ở trạng thái Active nhưng đã hết hạn thì cập nhật trạng thái thành Expired
            if (hold.Status == BookingHoldStatus.Active && hold.ExpiresAtUtc <= DateTime.UtcNow)
            {
                await _bookingHoldRepository.ExpireAsync(DateTime.UtcNow, cancellationToken);
                hold.Status = BookingHoldStatus.Expired;
            }

            return Map(hold);
        }

        public Task<bool> ReleaseAsync(Guid holdId, CancellationToken cancellationToken = default)
        {
            return _bookingHoldRepository.ReleaseAsync(holdId, DateTime.UtcNow, cancellationToken);//hủy hold theo id, gọi repository để cập nhật trạng thái hold thành Released nếu tìm thấy và chưa hết hạn, trả về true nếu hủy thành công, false nếu không tìm thấy hoặc đã hết hạn
        }

        public async Task<BookingHoldResponseDto?> ConfirmAsync(Guid holdId, CancellationToken cancellationToken = default)
        {
            // DAY5 TEST-ONLY CONFIRM FLOW:
            // Đây là confirm tối thiểu để test ghế bị khóa sau khi đặt.
            // Nó chưa tạo Booking, PaymentTransaction, Ticket, customer snapshot hay email.
            // Khi làm flow hoàn chỉnh, thay đoạn này bằng use case booking/payment thật.
            var now = DateTime.UtcNow;
            var hold = await _bookingHoldRepository.GetByIdAsync(holdId, cancellationToken);
            if (hold is null)
            {
                return null;
            }

            if (hold.Status != BookingHoldStatus.Active)
            {
                throw new InvalidOperationException($"Booking hold is already {hold.Status}.");
            }

            if (hold.ExpiresAtUtc <= now)
            {
                await _bookingHoldRepository.ExpireAsync(now, cancellationToken);
                throw new InvalidOperationException("Booking hold has expired.");
            }

            await _showtimeRepository.MarkSeatsBookedAsync(
                hold.ShowtimeId,
                hold.Seats.Select(seat => seat.SeatInventoryId).ToArray(),
                now,
                cancellationToken);

            var converted = await _bookingHoldRepository.ConvertAsync(holdId, now, cancellationToken);
            return converted is null ? null : Map(converted);
        }

        private static BookingHoldResponseDto Map(BookingHold hold)
        {
            //tính toán số giây còn lại trước khi hold hết hạn
            var remainingSeconds = hold.ExpiresAtUtc <= DateTime.UtcNow
                ? 0
                : (int)(hold.ExpiresAtUtc - DateTime.UtcNow).TotalSeconds;

            //map entity BookingHold sang DTO BookingHoldResponseDto để trả về cho client
            return new BookingHoldResponseDto
            {
                HoldId = hold.Id,
                HoldCode = hold.HoldCode,
                ShowtimeId = hold.ShowtimeId,
                Status = hold.Status.ToString(),
                ExpiresAtUtc = hold.ExpiresAtUtc,
                RemainingSeconds = remainingSeconds,
                SeatSubtotal = hold.SeatSubtotal,
                ComboSubtotal = hold.ComboSubtotal,
                DiscountAmount = hold.DiscountAmount,
                GrandTotal = hold.GrandTotal,
                Seats = hold.Seats.Select(seat => new BookingHoldSeatResponseDto
                {
                    SeatInventoryId = seat.SeatInventoryId,
                    SeatCode = seat.SeatCode,
                    SeatType = seat.SeatType,
                    UnitPrice = seat.UnitPrice,
                    CoupleGroupCode = seat.CoupleGroupCode
                }).ToList()
            };
        }
        private static string GenerateHoldCode(DateTime utcNow)
        {
            return $"HOLD-{utcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..32];
        }

    }
}

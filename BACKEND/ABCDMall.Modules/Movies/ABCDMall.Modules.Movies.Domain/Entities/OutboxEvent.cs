using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Domain.Entities
{
    public class OutboxEvent
    {
        //class này dùng để lưu trữ các sự kiện đã được phát ra từ hệ thống, nhưng chưa được xử lý hoặc gửi đi.
        //Nó thường được sử dụng trong kiến trúc Event-Driven
        //để đảm bảo rằng các sự kiện quan trọng không bị mất mát và có thể được xử lý lại nếu cần thiết.
        public Guid Id { get; set; }
        public string EventType { get; set; } = string.Empty; // Loại sự kiện (ví dụ: BookingCreated, PaymentCompleted)
        public string PayloadJson { get; set; } = string.Empty; // Dữ liệu sự kiện được lưu dưới dạng JSON
        public string Status { get; set; } = "Pending"; // Trạng thái của sự kiện (Pending, Processed, Failed)
        public int RetryCount { get; set; } // Số lần thử lại xử lý sự kiện
        public string? LastError { get; set; }  // Lỗi cuối cùng nếu có (nếu trạng thái là Failed)
        public DateTime OccurredAtUtc { get; set; } // Thời điểm sự kiện xảy ra
        public DateTime? ProcessedAtUtc { get; set; } // Thời điểm sự kiện được xử lý (nếu đã được xử lý)

    }
}

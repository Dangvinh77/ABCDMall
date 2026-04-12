using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Domain.Entities
{
    public class AuditLog
    {
        //class này dùng để lưu trữ các bản ghi về các hành động đã thực hiện trong hệ thống,
        //như tạo, cập nhật, xóa dữ liệu.
        public Guid Id { get; set; }
        public string EntityName { get; set; } = string.Empty; // Tên thực thể bi tac động (ví dụ: Booking, Payment)
        public string EntityId { get; set; } = string.Empty; // ID của thực thể bị tác động
        public string Action { get; set; } = string.Empty; // Hành động đã thực hiện (Create, Update, Delete)
        public string? ActorId { get; set; } // ID của người thực hiện hành động (nếu có)
        public string? ChangesJson { get; set; } // Dữ liệu thay đổi được lưu dưới dạng JSON (nếu có)
        public DateTime CreatedAtUtc { get; set; } // Thời điểm hành động được thực hiện
    }
}

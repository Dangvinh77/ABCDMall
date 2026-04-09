using ABCDMall.Shared.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DbController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("test-db")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // 1. Kiểm tra xem có thể kết nối tới SQL Server không
                bool canConnect = await _context.Database.CanConnectAsync();

                if (!canConnect)
                {
                    return StatusCode(500, new
                    {
                        status = "Error",
                        message = "Không thể kết nối tới SQL Server. Hãy kiểm tra lại Connection String hoặc Server."
                    });
                }

                // 2. (Tùy chọn) Kiểm tra xem đã có bảng nào chưa (đã chạy Migration chưa)
                // Ví dụ kiểm tra bảng Movies
                var databaseName = _context.Database.GetDbConnection().Database;

                return Ok(new
                {
                    status = "Success",
                    message = $"Kết nối thành công tới Database: [{databaseName}]",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                // Trả về chi tiết lỗi nếu có (lỗi sai pass, sai server, timeout...)
                return StatusCode(500, new
                {
                    status = "Exception",
                    message = ex.Message,
                    detail = ex.InnerException?.Message
                });
            }
        }
    }
}

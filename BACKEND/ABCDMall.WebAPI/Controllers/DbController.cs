using ABCDMall.Shared.MongoDB;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbController : ControllerBase
    {
        private readonly MongoContext _context;

        public DbController(MongoContext context)
        {
            _context = context;
        }

        [HttpGet("test-db")]
        public async Task<IActionResult> TestConnection()
        {
            var isAlive = await _context.CheckConnection();

            if (isAlive)
            {
                return Ok(new
                {
                    status = "Success",
                    message = "Kết nối MongoDB thành công rồi nhé nhóm 2!"
                });
            }

            return StatusCode(500, "Không thể kết nối tới MongoDB. Hãy kiểm tra lại Connection String hoặc IP Whitelist trên Atlas.");
        }
    }
}

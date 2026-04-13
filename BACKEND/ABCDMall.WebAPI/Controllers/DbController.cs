using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace ABCDMall.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DbController : ControllerBase
    {
        private readonly MoviesCatalogDbContext _catalogContext;
        private readonly MoviesBookingDbContext _bookingContext;

        public DbController(
            MoviesCatalogDbContext catalogContext,
            MoviesBookingDbContext bookingContext)
        {
            _catalogContext = catalogContext;
            _bookingContext = bookingContext;
        }

        [HttpGet("test-db")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var canConnectCatalog = await _catalogContext.Database.CanConnectAsync();
                var canConnectBooking = await _bookingContext.Database.CanConnectAsync();

                if (!canConnectCatalog || !canConnectBooking)
                {
                    return StatusCode(500, new
                    {
                        status = "Error",
                        message = "Khong the ket noi toi SQL Server hoac mot trong cac DbContext khong truy cap duoc database.",
                        catalogConnected = canConnectCatalog,
                        bookingConnected = canConnectBooking
                    });
                }

                var catalogDatabaseName = _catalogContext.Database.GetDbConnection().Database;
                var bookingDatabaseName = _bookingContext.Database.GetDbConnection().Database;

                return Ok(new
                {
                    status = "Success",
                    message = "Ket noi database thanh cong.",
                    catalog = new
                    {
                        database = catalogDatabaseName,
                        schema = MoviesCatalogDbContext.DefaultSchema
                    },
                    booking = new
                    {
                        database = bookingDatabaseName,
                        schema = MoviesBookingDbContext.DefaultSchema
                    },
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
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
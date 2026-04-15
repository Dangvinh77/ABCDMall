using ABCDMall.Modules.Movies.Application.Services.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Bookings.Models;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Catalog;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Repositories.Bookings;

public sealed class CatalogReadRepository : IShowtimeReadRepository, ISeatInventoryReadRepository
{
    private readonly MoviesCatalogDbContext _dbContext;

    public CatalogReadRepository(MoviesCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ShowtimeQuoteSnapshot?> GetShowtimeByIdAsync(
        Guid showtimeId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Showtimes
            .AsNoTracking()
            .Include(x => x.Hall)
            .Where(x => x.Id == showtimeId)
            .Select(x => new ShowtimeQuoteSnapshot
            {
                ShowtimeId = x.Id,
                MovieId = x.MovieId,
                CinemaId = x.CinemaId,
                HallId = x.HallId,
                HallType = x.Hall != null ? x.Hall.HallType.ToString() : string.Empty,
                BusinessDate = x.BusinessDate,
                StartAtUtc = x.StartAtUtc,
                BasePrice = x.BasePrice,
                Status = x.Status.ToString()
            })
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SeatInventoryQuoteSnapshot>> GetSeatsByIdsAsync(
        Guid showtimeId,
        IReadOnlyCollection<Guid> seatInventoryIds,
        CancellationToken cancellationToken = default)
    {
        if (seatInventoryIds.Count == 0)
        {
            return Array.Empty<SeatInventoryQuoteSnapshot>();
        }

        return await _dbContext.ShowtimeSeatInventories
            .AsNoTracking()
            .Where(x => x.ShowtimeId == showtimeId && seatInventoryIds.Contains(x.Id))
            .Select(x => new SeatInventoryQuoteSnapshot
            {
                SeatInventoryId = x.Id,
                ShowtimeId = x.ShowtimeId,
                SeatCode = x.SeatCode,
                Row = x.RowLabel,
                Col = x.ColumnNumber,
                SeatType = x.SeatType.ToString(),
                Status = x.Status.ToString(),
                Price = x.Price,
                CoupleGroupCode = x.CoupleGroupCode
            })
            .ToListAsync(cancellationToken);
    }
}

using ABCDMall.Modules.Users.Application.DTOs.Bidding;
using ABCDMall.Modules.Users.Application.Services.Bidding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public/carousel")]
public sealed class PublicCarouselController : ControllerBase
{
    private readonly IPublicCarouselQueryService _publicCarouselQueryService;

    public PublicCarouselController(IPublicCarouselQueryService publicCarouselQueryService)
    {
        _publicCarouselQueryService = publicCarouselQueryService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PublicCarouselItemDto>>> GetActiveCarousel(CancellationToken cancellationToken = default)
        => Ok(await _publicCarouselQueryService.GetActiveCarouselItemsAsync(cancellationToken));
}

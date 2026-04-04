using Microsoft.AspNetCore.Mvc;
using ABCDMall.Modules.FoodCourt.Application.Interfaces;
using ABCDMall.Modules.FoodCourt.Application.DTOs;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using ABCDMall.Modules.FoodCourt.Application.Helpers;

[ApiController]
[Route("api/food")]
public class FoodController : ControllerBase
{
    private readonly IFoodService _service;

    public FoodController(IFoodService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FoodItemDto dto)
{
    var item = new FoodItem
    {
        Id = Guid.NewGuid().ToString(),
        Name = dto.Name,
       // Price = dto.Price,
        ImageUrl = dto.ImageUrl,
        Description = dto.Description,
        Slug = SlugHelper.GenerateSlug(dto.Name)  
       
    };

    await _service.CreateAsync(item);

    return Ok(item);
}   

   [HttpGet("{slug}")]
public async Task<IActionResult> GetBySlug(string slug)
{
    var food = await _service.GetBySlugAsync(slug);

    if (food == null)
        return NotFound();

    return Ok(food);
}
}
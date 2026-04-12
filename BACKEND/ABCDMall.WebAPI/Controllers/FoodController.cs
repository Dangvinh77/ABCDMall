using Microsoft.AspNetCore.Mvc;
using ABCDMall.Modules.FoodCourt.Application.Interfaces;
using ABCDMall.Shared.DTOs;
using ABCDMall.Modules.FoodCourt.Domain.Entities;

[ApiController]
[Route("api/food")]
public class FoodController : ControllerBase
{
    private readonly IFoodService _service;

    public FoodController(IFoodService service)
    {
        _service = service;
    }

    // GET ALL
    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _service.GetAllAsync());

    // GET BY SLUG
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var food = await _service.GetBySlugAsync(slug);
        if (food == null) return NotFound();
        return Ok(food);
    }

    // SEO ROUTE
    [HttpGet("mall/{mall}/{slug}")]
    public async Task<IActionResult> GetSeo(string mall, string slug)
    {
        var food = await _service.GetBySlugAsync(slug);
        if (food == null) return NotFound();
        return Ok(food);
    }

    // SEARCH
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        var data = await _service.SearchAsync(keyword);
        return Ok(data);
    }

    // CREATE
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] FoodItemDto dto,
        IFormFile? file)
    {
        string imageUrl = dto.ImageUrl ?? "";

        if (file != null && file.Length > 0)
        {
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            imageUrl = $"{Request.Scheme}://{Request.Host}/images/{fileName}";
        }

        var item = new FoodItem
        {
            // ✅ KHÔNG set Id — EF Core tự sinh int Id (IDENTITY)
            Name = dto.Name ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            ImageUrl = imageUrl
        };

        await _service.CreateAsync(item);
        return Ok(item);
    }

    // UPDATE
    [HttpPut("{id:int}")]  // ✅ int route constraint
    public async Task<IActionResult> Update(
        int id,            // ✅ string → int
        [FromForm] string name,
        [FromForm] string description,
        [FromForm] string? imageUrl,
        IFormFile? file)
    {
        if (file != null && file.Length > 0)
        {
            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
        }

        var dto = new FoodItemDto
        {
            Name = name,
            Description = description,
            ImageUrl = imageUrl ?? ""
        };

        var result = await _service.UpdateAsync(id, dto);  // ✅ int
        if (!result) return NotFound();

        var updated = await _service.GetByIdAsync(id);     // ✅ int
        return Ok(updated);
    }

    // DELETE
    [HttpDelete("{id:int}")]  // ✅ int route constraint
    public async Task<IActionResult> Delete(int id)        // ✅ string → int
    {
        var result = await _service.DeleteAsync(id);
        if (!result) return NotFound();
        return Ok("Deleted successfully");
    }
}
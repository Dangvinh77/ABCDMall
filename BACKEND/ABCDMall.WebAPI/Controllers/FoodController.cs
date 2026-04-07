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


//Create

[HttpPost]
public async Task<IActionResult> Create(
    [FromForm] FoodItemDto dto,
    IFormFile? file 
)
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
        Id = Guid.NewGuid().ToString(),
        Name = dto.Name,
        Description = dto.Description,
        ImageUrl = imageUrl
    };

    await _service.CreateAsync(item);

    return Ok(item);
}


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

//Update

[HttpPut("{id}")]
public async Task<IActionResult> Update(
    string id,
    [FromForm] string name,
    [FromForm] string description,
    [FromForm] string? imageUrl,
    IFormFile? file
)
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

        // 🔥 FIX QUAN TRỌNG
        imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
    }

    var dto = new FoodItemDto
    {
        Name = name,
        Description = description,
        ImageUrl = imageUrl ?? ""
    };

    var result = await _service.UpdateAsync(id, dto);

    if (!result) return NotFound();

    var updated = await _service.GetByIdAsync(id);

    return Ok(updated);
}


//     [HttpPut("{id}")]
// public async Task<IActionResult> Update(
//     string id,
//     [FromForm] string name,
//     [FromForm] string description,
//     [FromForm] string? imageUrl,
//     IFormFile? file
// )
// {
//     if (file != null)
//     {
//         var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
//         var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

//         if (!Directory.Exists(folder))
//             Directory.CreateDirectory(folder);

//         var path = Path.Combine(folder, fileName);

//         using var stream = new FileStream(path, FileMode.Create);
//         await file.CopyToAsync(stream);

//        // imageUrl = $"/uploads/{fileName}";
//         imageUrl = $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";
//     }

//     var dto = new FoodItemDto
//     {
//         Name = name,
//         Description = description,
//         ImageUrl = imageUrl ?? ""
//     };

//     var result = await _service.UpdateAsync(id, dto);

//     if (!result) return NotFound();


//     var updated = await _service.GetByIdAsync(id);

//     return Ok(updated);
// }

    // SITEMAP
    // [HttpGet("sitemap")]
    // public async Task<IActionResult> Sitemap()
    // {
    //     var foods = await _service.GetAllAsync();

    //     var urls = foods.Select(f =>
    //         $"https://yourdomain.com/mall/ABCDMall/{f.Slug}"
    //     );

    //     return Ok(urls);
    // }

    // SEARCH
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string keyword)
    {
        var data = await _service.SearchAsync(keyword);
        return Ok(data);
    }
    // delete
    [HttpDelete("{id}")] 
    public async Task<IActionResult> Delete(string id) 
    { 
        await _service.DeleteAsync(id); 
        return Ok("Deleted successfully"); 
    }

}
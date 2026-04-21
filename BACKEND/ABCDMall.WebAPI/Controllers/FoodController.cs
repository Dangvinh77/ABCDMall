using ABCDMall.Modules.FoodCourt.Application.DTOs;
using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using ABCDMall.Modules.FoodCourt.Application.Services.Foods;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FoodController : ControllerBase
{
    private readonly IFoodQueryService _foodQueryService;
    private readonly IFoodCommandService _foodCommandService;
    private readonly IValidator<FoodListQueryDto> _foodListQueryValidator;
    private readonly IValidator<CreateFoodRequestDto> _createFoodValidator;
    private readonly IValidator<UpdateFoodRequestDto> _updateFoodValidator;

    public FoodController(
        IFoodQueryService foodQueryService,
        IFoodCommandService foodCommandService,
        IValidator<FoodListQueryDto> foodListQueryValidator,
        IValidator<CreateFoodRequestDto> createFoodValidator,
        IValidator<UpdateFoodRequestDto> updateFoodValidator)
    {
        _foodQueryService = foodQueryService;
        _foodCommandService = foodCommandService;
        _foodListQueryValidator = foodListQueryValidator;
        _createFoodValidator = createFoodValidator;
        _updateFoodValidator = updateFoodValidator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<FoodItemDto>>> GetFoods(
        [FromQuery] string? keyword,
        CancellationToken cancellationToken = default)
    {
        var query = new FoodListQueryDto { Keyword = keyword };
        var validationResult = await _foodListQueryValidator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        return Ok(await _foodQueryService.GetListAsync(keyword, cancellationToken));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<FoodItemDto>> GetFoodById(string id, CancellationToken cancellationToken = default)
    {
        var item = await _foodQueryService.GetByIdAsync(id, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("slug/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<FoodItemDto>> GetFoodBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var item = await _foodQueryService.GetBySlugAsync(slug, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("mall/{mall}/{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<FoodItemDto>> GetFoodSeo(string mall, string slug, CancellationToken cancellationToken = default)
    {
        _ = mall;
        var item = await _foodQueryService.GetBySlugAsync(slug, cancellationToken);
        return item is null ? NotFound() : Ok(item);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyList<FoodItemDto>>> SearchFoods([FromQuery] string keyword = "", CancellationToken cancellationToken = default)
        => Ok(await _foodQueryService.GetListAsync(keyword, cancellationToken));

    [HttpPost]
    //[Authorize(Roles = "Admin,Manager")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateFood([FromForm] CreateFoodRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createFoodValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

       // await _foodCommandService.CreateAsync(request, cancellationToken);

       var imageUrl = await SaveImageAsync(request.ImageFile);

        request.ImageUrl = imageUrl ?? request.ImageUrl;

        await _foodCommandService.CreateAsync(request,cancellationToken);

        return Ok(new { message = "Food created successfully" });
    }

    // [HttpPut("{id}")]
    // [Authorize(Roles = "Admin,Manager")]
    // [AllowAnonymous]
    // public async Task<IActionResult> UpdateFood(string id, [FromForm] UpdateFoodRequestDto request, CancellationToken cancellationToken = default)
    // {
    //     var validationResult = await _updateFoodValidator.ValidateAsync(request, cancellationToken);
    //     if (!validationResult.IsValid)
    //     {
    //         return ValidationProblem(ToValidationProblemDetails(validationResult));
    //     }

    //     var updated = await _foodCommandService.UpdateAsync(id, request, cancellationToken);
    //     if (!updated)
    //     {
    //         return NotFound();
    //     }

    //     return Ok(new { message = "Food updated successfully" });
    // }

    [HttpPut("{id}")]
   //[Authorize(Roles = "Admin,Manager")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateFood(
        string id,
        [FromForm] UpdateFoodRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateFoodValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        // 🔥 THÊM ĐOẠN NÀY
        var imageUrl = await SaveImageAsync(request.ImageFile);
        if (imageUrl != null)
        {
            request.ImageUrl = imageUrl;
        }

        var updated = await _foodCommandService.UpdateAsync(id, request, cancellationToken);

        if (!updated)
        {
            return NotFound();
        }

        return Ok(new { message = "Food updated successfully" });
    }

    [HttpDelete("{id}")]
    //[Authorize(Roles = "Admin,Manager")]
    [AllowAnonymous]
    public async Task<IActionResult> DeleteFood(string id, CancellationToken cancellationToken = default)
    {
        var deleted = await _foodCommandService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok(new { message = "Food deleted successfully" });
    }

    private static ValidationProblemDetails ToValidationProblemDetails(FluentValidation.Results.ValidationResult validationResult)
    {
        return new ValidationProblemDetails(
            validationResult.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => x.ErrorMessage).ToArray()));
    }

    // [HttpPost("upload")]
    // public async Task<IActionResult> Upload(IFormFile file)
    // {
    // if (file == null || file.Length == 0)
    //     return BadRequest("No file");

    // var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/foodcourt");
    // if (!Directory.Exists(uploadsFolder))
    //     Directory.CreateDirectory(uploadsFolder);

    // var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    // var filePath = Path.Combine(uploadsFolder, fileName);

    // using var stream = new FileStream(filePath, FileMode.Create);
    // await file.CopyToAsync(stream);

    // return Ok(new { imageUrl = $"/images/foodcourt/{fileName}" });
    // }

    private async Task<string?> SaveImageAsync(IFormFile? file)
{
    if (file == null || file.Length == 0)
        return null;

    var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/foodcourt");

    if (!Directory.Exists(folder))
        Directory.CreateDirectory(folder);

    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
    var filePath = Path.Combine(folder, fileName);

    using var stream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(stream);

    return $"/images/foodcourt/{fileName}";
}
}

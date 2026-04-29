using ABCDMall.Modules.FoodCourt.Application.DTOs;
using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using ABCDMall.Modules.FoodCourt.Application.Services.Foods;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ABCDMall.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FoodController : ControllerBase
{
    private readonly IFoodQueryService _foodQueryService;
    private readonly IFoodCommandService _foodCommandService;
    private readonly IFoodManagerService _foodManagerService;
    private readonly IValidator<FoodListQueryDto> _foodListQueryValidator;
    private readonly IValidator<CreateFoodRequestDto> _createFoodValidator;
    private readonly IValidator<UpdateFoodRequestDto> _updateFoodValidator;
    private readonly IValidator<UpsertFoodManagerRequestDto> _upsertFoodManagerValidator;
    private readonly IValidator<UpsertFoodMenuItemRequestDto> _upsertFoodMenuItemValidator;

    public FoodController(
        IFoodQueryService foodQueryService,
        IFoodManagerService foodManagerService,
        IFoodCommandService foodCommandService,
        IValidator<FoodListQueryDto> foodListQueryValidator,
        IValidator<CreateFoodRequestDto> createFoodValidator,
        IValidator<UpdateFoodRequestDto> updateFoodValidator,
        IValidator<UpsertFoodManagerRequestDto> upsertFoodManagerValidator,
        IValidator<UpsertFoodMenuItemRequestDto> upsertFoodMenuItemValidator)
    {
        _foodQueryService = foodQueryService;
        _foodManagerService = foodManagerService;
        _foodCommandService = foodCommandService;
        _foodListQueryValidator = foodListQueryValidator;
        _createFoodValidator = createFoodValidator;
        _updateFoodValidator = updateFoodValidator;
        _upsertFoodManagerValidator = upsertFoodManagerValidator;
        _upsertFoodMenuItemValidator = upsertFoodMenuItemValidator;
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

    [HttpGet("manager")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<IReadOnlyList<FoodDetailDto>>> GetMyFoodStalls(CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (ownerShopId is null)
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        return Ok(await _foodManagerService.GetMyFoodStallsAsync(ownerShopId, cancellationToken));
    }

    [HttpGet("manager/creation-status")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<FoodManagerCreationStatusDto>> GetMyFoodCreationStatus(CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (ownerShopId is null)
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        return Ok(await _foodManagerService.GetCreationStatusAsync(ownerShopId, cancellationToken));
    }

    [HttpPost("manager")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<FoodDetailDto>> CreateMyFoodStall([FromForm] UpsertFoodManagerRequestDto request, CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (ownerShopId is null)
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        var validationResult = await _upsertFoodManagerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        var imageUrl = await SaveImageAsync(request.ImageFile);
        if (imageUrl is not null)
        {
            request.ImageUrl = imageUrl;
        }

        try
        {
            return Ok(await _foodManagerService.CreateMyFoodStallAsync(ownerShopId, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("manager/{id}")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<FoodDetailDto>> UpdateMyFoodStall(string id, [FromForm] UpsertFoodManagerRequestDto request, CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (ownerShopId is null)
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        var validationResult = await _upsertFoodManagerValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        var imageUrl = await SaveImageAsync(request.ImageFile);
        if (imageUrl is not null)
        {
            request.ImageUrl = imageUrl;
        }

        try
        {
            var updated = await _foodManagerService.UpdateMyFoodStallAsync(ownerShopId, id, request, cancellationToken);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("manager/{id}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> DeleteMyFoodStall(string id, CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (ownerShopId is null)
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        var deleted = await _foodManagerService.DeleteMyFoodStallAsync(ownerShopId, id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok(new { message = "Food stall deleted successfully" });
    }

    [HttpPost("manager/{id}/menu-items")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<FoodMenuItemDto>> AddMenuItem(string id, [FromBody] UpsertFoodMenuItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (ownerShopId is null)
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        var validationResult = await _upsertFoodMenuItemValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        var menuItem = await _foodManagerService.AddMenuItemAsync(ownerShopId, id, request, cancellationToken);
        return menuItem is null ? NotFound() : Ok(menuItem);
    }

    [HttpPut("manager/{id}/menu-items/{menuItemId}")]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<FoodMenuItemDto>> UpdateMenuItem(string id, string menuItemId, [FromBody] UpsertFoodMenuItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (ownerShopId is null)
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        var validationResult = await _upsertFoodMenuItemValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        var menuItem = await _foodManagerService.UpdateMenuItemAsync(ownerShopId, id, menuItemId, request, cancellationToken);
        return menuItem is null ? NotFound() : Ok(menuItem);
    }

    [HttpDelete("manager/{id}/menu-items/{menuItemId}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> DeleteMenuItem(string id, string menuItemId, CancellationToken cancellationToken = default)
    {
        var ownerShopId = GetOwnerShopId();
        if (ownerShopId is null)
        {
            return BadRequest("Manager account does not have a shop id.");
        }

        var deleted = await _foodManagerService.DeleteMenuItemAsync(ownerShopId, id, menuItemId, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok(new { message = "Menu item deleted successfully" });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> CreateFood([FromForm] CreateFoodRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _createFoodValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        var imageUrl = await SaveImageAsync(request.ImageFile);
        request.ImageUrl = imageUrl ?? request.ImageUrl;

        await _foodCommandService.CreateAsync(request, cancellationToken);

        return Ok(new { message = "Food created successfully" });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UpdateFood(string id, [FromForm] UpdateFoodRequestDto request, CancellationToken cancellationToken = default)
    {
        var validationResult = await _updateFoodValidator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return ValidationProblem(ToValidationProblemDetails(validationResult));
        }

        var imageUrl = await SaveImageAsync(request.ImageFile);
        if (imageUrl is not null)
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
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> DeleteFood(string id, CancellationToken cancellationToken = default)
    {
        var deleted = await _foodCommandService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        return Ok(new { message = "Food deleted successfully" });
    }

    [HttpPost("upload")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> UploadFoodImage(IFormFile file, CancellationToken cancellationToken = default)
    {
        var imageUrl = await SaveImageAsync(file);
        if (imageUrl is null)
        {
            return BadRequest(new { message = "Image file is required." });
        }

        return Ok(new { imageUrl });
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

    private async Task<string?> SaveImageAsync(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/foodcourt");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/foodcourt/{fileName}";
    }

    private string? GetOwnerShopId()
    {
        return User.FindFirstValue("shopId");
    }
}

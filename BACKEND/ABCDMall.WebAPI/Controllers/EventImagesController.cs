using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ABCDMall.WebAPI.Controllers;

[ApiController]
[Authorize(Roles = "Admin,Manager")]
[Route("api/events")]
public sealed class EventImagesController : ControllerBase
{
    [HttpPost("upload")]
    public async Task<IActionResult> UploadEventImage(IFormFile file, CancellationToken cancellationToken = default)
    {
        var imageUrl = await SaveImageAsync(file, cancellationToken);
        if (imageUrl is null)
        {
            return BadRequest(new { message = "Image file is required." });
        }

        return Ok(new { imageUrl });
    }

    private static async Task<string?> SaveImageAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
        {
            return null;
        }

        var folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/events");
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(folder, fileName);

        await using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream, cancellationToken);

        return $"/images/events/{fileName}";
    }
}

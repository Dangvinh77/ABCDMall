// using ABCDMall.Modules.FoodCourt.Application.DTOs;
// using ABCDMall.Modules.FoodCourt.Application.Interfaces;
// using ABCDMall.Modules.FoodCourt.Domain.Entities;
// using ABCDMall.Modules.FoodCourt.Application.Helpers;

// namespace ABCDMall.Modules.FoodCourt.Application.Services;

// public class FoodService : IFoodService
// {
//     private readonly IFoodRepository _repo;

//     public FoodService(IFoodRepository repo)
//     {
//         _repo = repo;
//     }

//     public async Task<List<FoodItemDto>> GetAllAsync()
//     {
//         var data = await _repo.GetAllAsync();

//         return data.Select(x => new FoodItemDto
//         {
//             Id = x.Id,
//             Name = x.Name,
//             //Price = x.Price,
//             ImageUrl = x.ImageUrl,
//             Slug = x.Slug,
//             Description = x.Description
//         }).ToList();
//     }


// public async Task<FoodItemDto?> GetByIdAsync(string id)
// {
//     var item = await _repo.GetByIdAsync(id);

//     if (item == null) return null;

//     return new FoodItemDto
//     {
//         Id = item.Id,
//         Name = item.Name,
//         Slug = item.Slug,
//         ImageUrl = item.ImageUrl,
//         Description = item.Description
//     };
// }

//     public async Task CreateAsync(FoodItem item)
//     {
//         item.Slug = SlugHelper.GenerateSlug(item.Name);
//         await _repo.CreateAsync(item);
        
//     }

//     public async Task<FoodItemDto?> GetBySlugAsync(string slug)
//     {
//     var item = await _repo.GetBySlugAsync(slug);

//     if (item == null) return null;

//     return new FoodItemDto
//     {
//         Id = item.Id,
//         Name = item.Name,
//         Slug = item.Slug,
//         ImageUrl = item.ImageUrl,
//         Description = item.Description
//     };
//     }

// //Update 

//     public async Task<bool> UpdateAsync(string id, FoodItemDto dto)
//     {
//      var existing = await _repo.GetByIdAsync(id);

//     if (existing == null) 
    
//     return false;

//     existing.Name = dto.Name;
//     existing.ImageUrl = dto.ImageUrl;
//     existing.Description = dto.Description;

//     existing.Slug = SlugHelper.GenerateSlug(dto.Name);

//     await _repo.UpdateAsync(id, existing);
//     return true;
//     }

// //Delete
//     public async Task<bool> DeleteAsync(string id)
// {
//     var existing = await _repo.GetByIdAsync(id);
//     if (existing == null) return false;

//     await _repo.DeleteAsync(id);
//     return true;
// }

// //search
// public async Task<List<FoodItemDto>> SearchAsync(string keyword)
// {
//     var data = await _repo.GetAllAsync();

//     return data
//         .Where(x => x.Name.ToLower().Contains(keyword.ToLower()))
//         .Select(x => new FoodItemDto
//         {
//             Id = x.Id,
//             Name = x.Name,
//             ImageUrl = x.ImageUrl,
//             Slug = x.Slug,
//             Description = x.Description
//         })
//         .ToList();
// }

// }


using ABCDMall.Modules.FoodCourt.Application.DTOs;
using ABCDMall.Modules.FoodCourt.Application.Interfaces;
using ABCDMall.Modules.FoodCourt.Domain.Entities;
using ABCDMall.Modules.FoodCourt.Application.Helpers;

namespace ABCDMall.Modules.FoodCourt.Application.Services;

public class FoodService : IFoodService
{
    private readonly IFoodRepository _repo;

    public FoodService(IFoodRepository repo)
    {
        _repo = repo;
    }

    // ================= GET ALL =================
    public async Task<List<FoodItemDto>> GetAllAsync()
    {
        var data = await _repo.GetAllAsync();

        return data.Select(Map).ToList();
    }

    // ================= GET BY ID =================
    public async Task<FoodItemDto?> GetByIdAsync(string id)
    {
        var item = await _repo.GetByIdAsync(id);
        if (item == null) return null;

        return Map(item);
    }

    // ================= CREATE =================
    public async Task CreateAsync(FoodItem item)
    {
        if (string.IsNullOrEmpty(item.Name))
            throw new Exception("Name is required");

        item.Slug = SlugHelper.GenerateSlug(item.Name);

        await _repo.CreateAsync(item);
    }

    // ================= GET BY SLUG =================
    public async Task<FoodItemDto?> GetBySlugAsync(string slug)
    {
        var item = await _repo.GetBySlugAsync(slug);
        if (item == null) return null;

        return Map(item);
    }

    // ================= UPDATE =================
    public async Task<bool> UpdateAsync(string id, FoodItemDto dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return false;

        existing.Name = dto.Name;
        existing.ImageUrl = dto.ImageUrl;
        existing.Description = dto.Description;

        // 🔥 FIX CHÍNH: luôn update slug theo name mới
       // existing.Slug = SlugHelper.GenerateSlug(dto.Name);

        existing.Slug = string.IsNullOrEmpty(dto.Slug)
            ? SlugHelper.GenerateSlug(dto.Name)
            : SlugHelper.GenerateSlug(dto.Slug);

        await _repo.UpdateAsync(id, existing);
        return true;
    }

    // ================= DELETE =================
    public async Task<bool> DeleteAsync(string id)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return false;

        await _repo.DeleteAsync(id);
        return true;
    }

    // ================= SEARCH =================
    public async Task<List<FoodItemDto>> SearchAsync(string keyword)
    {
        var data = await _repo.GetAllAsync();

        keyword = keyword?.Trim().ToLower() ?? "";

        return data
            .Where(x => !string.IsNullOrEmpty(x.Name) &&
                        x.Name.ToLower().Contains(keyword))
            .Select(Map)
            .ToList();
    }

    // ================= MAPPER =================
    private static FoodItemDto Map(FoodItem x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        ImageUrl = x.ImageUrl,
        Slug = x.Slug,
        Description = x.Description
    };
}
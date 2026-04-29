using ABCDMall.Modules.FoodCourt.Application.DTOs;

namespace ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;

public sealed class FoodDetailDto : FoodItemDto
{
    public IReadOnlyList<FoodMenuItemDto> MenuItems { get; set; } = [];
}

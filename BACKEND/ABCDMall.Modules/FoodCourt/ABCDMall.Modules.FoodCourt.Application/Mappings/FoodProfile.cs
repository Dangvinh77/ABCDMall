using ABCDMall.Modules.FoodCourt.Application.DTOs;
using ABCDMall.Modules.FoodCourt.Domain.Entities;

namespace ABCDMall.Modules.FoodCourt.Application.Mappings;

public sealed class FoodProfile : AutoMapper.Profile
{
    public FoodProfile()
    {
        CreateMap<FoodItem, FoodItemDto>();
    }
}


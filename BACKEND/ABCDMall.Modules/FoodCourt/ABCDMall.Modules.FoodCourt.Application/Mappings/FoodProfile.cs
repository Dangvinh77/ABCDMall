using System.Text.Json;
using ABCDMall.Modules.FoodCourt.Application.DTOs;
using ABCDMall.Modules.FoodCourt.Application.DTOs.Foods;
using ABCDMall.Modules.FoodCourt.Domain.Entities;

namespace ABCDMall.Modules.FoodCourt.Application.Mappings;

public sealed class FoodProfile : AutoMapper.Profile
{
    public FoodProfile()
    {
        CreateMap<FoodItem, FoodItemDto>();
        CreateMap<FoodItem, FoodDetailDto>();
        CreateMap<FoodMenuItem, FoodMenuItemDto>()
            .ForMember(dest => dest.Ingredients, opt => opt.MapFrom(src => ParseIngredients(src.IngredientsJson)));
    }

    private static IReadOnlyList<string> ParseIngredients(string? ingredientsJson)
    {
        if (string.IsNullOrWhiteSpace(ingredientsJson))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(ingredientsJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}


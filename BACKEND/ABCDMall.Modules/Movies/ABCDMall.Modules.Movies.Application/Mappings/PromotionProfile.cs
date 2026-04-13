using AutoMapper;
using ABCDMall.Modules.Movies.Application.DTOs.Promotions;
using ABCDMall.Modules.Movies.Domain.Entities;

namespace ABCDMall.Modules.Movies.Application.Mappings
{
    public sealed class PromotionProfile : Profile
    {
        public PromotionProfile()
        {
            CreateMap<Promotion, PromotionResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<Promotion, PromotionDetailResponseDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<PromotionRule, PromotionRuleDto>()
                .ForMember(dest => dest.RuleType, opt => opt.MapFrom(src => src.RuleType.ToString()));
        }
    }
}

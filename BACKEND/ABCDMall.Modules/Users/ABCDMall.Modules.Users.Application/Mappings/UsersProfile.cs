using ABCDMall.Modules.Users.Application.DTOs.Auth;
using ABCDMall.Modules.Users.Application.DTOs.RentalAreas;
using ABCDMall.Modules.Users.Application.DTOs.ShopInfos;
using ABCDMall.Modules.Users.Domain.Entities;

namespace ABCDMall.Modules.Users.Application.Mappings;

public sealed class UsersProfile : AutoMapper.Profile
{
    public UsersProfile()
    {
        CreateMap<User, UserProfileResponseDto>();
        CreateMap<User, UserSummaryResponseDto>();
        CreateMap<ProfileUpdateHistory, ProfileUpdateHistoryResponseDto>();
        CreateMap<RentalArea, RentalAreaResponseDto>();
        CreateMap<ShopMonthlyBill, ShopMonthlyBillResponseDto>()
            .ForMember(dest => dest.LeaseStartDate, opt => opt.MapFrom(src => src.LeaseStartDate.ToString("yyyy-MM-dd")));
    }
}

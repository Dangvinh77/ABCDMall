namespace ABCDMall.Modules.UtilityMap.Application.DTOs.Maps;

/// <summary>
/// Request body khi Admin gán một MapLocation cho một ShopInfo (tenant).
/// </summary>
public sealed class AssignSlotRequestDto
{
    /// <summary>ShopInfo.Id của Manager vừa được tạo tài khoản.</summary>
    public string ShopInfoId { get; set; } = string.Empty;
}
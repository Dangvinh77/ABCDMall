namespace ABCDMall.Modules.Users.Application.DTOs.Bidding;

public sealed class BidPublishSummaryDto
{
    public DateTime TargetMondayDate { get; set; }

    public int ActiveBidCount { get; set; }

    public bool MovieAdIncluded { get; set; }

    public int TotalSlots { get; set; }
}

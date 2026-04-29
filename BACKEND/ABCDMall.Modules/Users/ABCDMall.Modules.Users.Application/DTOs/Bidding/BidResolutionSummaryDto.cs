namespace ABCDMall.Modules.Users.Application.DTOs.Bidding;

public sealed class BidResolutionSummaryDto
{
    public DateTime TargetMondayDate { get; set; }

    public int TotalPending { get; set; }

    public int WonCount { get; set; }

    public int LostCount { get; set; }
}

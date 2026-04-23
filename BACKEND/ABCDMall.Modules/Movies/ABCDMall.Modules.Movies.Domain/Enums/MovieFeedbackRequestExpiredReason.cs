namespace ABCDMall.Modules.Movies.Domain.Enums;

public enum MovieFeedbackRequestExpiredReason
{
    None = 0,
    OpenedNoSubmission7Days = 1,
    SubmissionLimitReached = 2,
    Cancelled = 3
}

using ABCDMall.Modules.Movies.Domain.Entities;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class MovieFeedbackRequestLifecycleShapeTests
{
    [Fact]
    public void Should_start_without_open_tracking()
    {
        var request = new MovieFeedbackRequest();

        Assert.Null(request.FirstOpenedAtUtc);
        Assert.Null(request.LastOpenedAtUtc);
        Assert.Null(request.ExpiredReason);
    }
}

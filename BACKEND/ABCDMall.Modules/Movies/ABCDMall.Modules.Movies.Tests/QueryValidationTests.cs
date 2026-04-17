using ABCDMall.Modules.Movies.Application.DTOs.Movies;
using ABCDMall.Modules.Movies.Application.DTOs.Showtimes;
using ABCDMall.Modules.Movies.Application.Services.Movies.Validators;
using ABCDMall.Modules.Movies.Application.Services.Showtimes.Validators;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class QueryValidationTests
{
    [Fact]
    public async Task MovieListValidator_ShouldRejectUnknownStatus()
    {
        var validator = new MovieListQueryDtoValidator();

        var result = await validator.ValidateAsync(new MovieListQueryDto
        {
            Status = "ShowingNow"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(MovieListQueryDto.Status));
    }

    [Fact]
    public async Task MovieListValidator_ShouldAcceptKnownStatus()
    {
        var validator = new MovieListQueryDtoValidator();

        var result = await validator.ValidateAsync(new MovieListQueryDto
        {
            Status = "NowShowing"
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ShowtimeListValidator_ShouldRejectUnknownHallTypeAndLanguage()
    {
        var validator = new ShowtimeListQueryDtoValidator();

        var result = await validator.ValidateAsync(new ShowtimeListQueryDto
        {
            HallType = "ScreenX",
            Language = "VietSub"
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ShowtimeListQueryDto.HallType));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(ShowtimeListQueryDto.Language));
    }

    [Fact]
    public async Task ShowtimeListValidator_ShouldAcceptKnownEnumNames()
    {
        var validator = new ShowtimeListQueryDtoValidator();

        var result = await validator.ValidateAsync(new ShowtimeListQueryDto
        {
            HallType = "Standard2D",
            Language = "Subtitle"
        });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ShowtimeListValidator_ShouldAcceptSharedContractAliases()
    {
        var validator = new ShowtimeListQueryDtoValidator();

        var result = await validator.ValidateAsync(new ShowtimeListQueryDto
        {
            HallType = "IMAX",
            Language = "Sub"
        });

        Assert.True(result.IsValid);
    }
}

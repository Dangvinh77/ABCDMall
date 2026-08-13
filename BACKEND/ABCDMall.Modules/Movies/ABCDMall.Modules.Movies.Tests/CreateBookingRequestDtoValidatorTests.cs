using ABCDMall.Modules.Movies.Application.DTOs.Bookings;
using ABCDMall.Modules.Movies.Application.Services.Bookings.Validators;
using Xunit;

namespace ABCDMall.Modules.Movies.Tests;

public sealed class CreateBookingRequestDtoValidatorTests
{
    [Fact]
    public async Task Should_require_at_least_one_hold_id()
    {
        var validator = new CreateBookingRequestDtoValidator();
        var model = new CreateBookingRequestDto
        {
            CustomerName = "Alice",
            CustomerEmail = "alice@example.com",
            CustomerPhoneNumber = "0900000000"
        };

        var result = await validator.ValidateAsync(model);

        Assert.Contains(result.Errors, error => error.PropertyName == "HoldIds");
    }
}

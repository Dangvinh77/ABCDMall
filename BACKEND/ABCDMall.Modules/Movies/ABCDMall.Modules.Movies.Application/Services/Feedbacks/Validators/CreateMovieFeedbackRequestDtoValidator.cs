using ABCDMall.Modules.Movies.Application.DTOs.Feedbacks;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Feedbacks.Validators;

public sealed class CreateMovieFeedbackRequestDtoValidator : AbstractValidator<CreateMovieFeedbackRequestDto>
{
    public CreateMovieFeedbackRequestDtoValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.Comment)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.DisplayName)
            .MaximumLength(120)
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        RuleFor(x => x.Email)
            .EmailAddress()
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleForEach(x => x.Tags)
            .MaximumLength(40);
    }
}

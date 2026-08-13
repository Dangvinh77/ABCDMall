using ABCDMall.Modules.Movies.Application.DTOs.Feedbacks;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Feedbacks.Validators;

public sealed class SubmitMovieFeedbackByTokenRequestDtoValidator : AbstractValidator<SubmitMovieFeedbackByTokenRequestDto>
{
    public SubmitMovieFeedbackByTokenRequestDtoValidator()
    {
        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5);

        RuleFor(x => x.Comment)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(x => x.DisplayName)
            .MaximumLength(120)
            .When(x => !string.IsNullOrWhiteSpace(x.DisplayName));

        RuleForEach(x => x.Tags)
            .MaximumLength(40);
    }
}

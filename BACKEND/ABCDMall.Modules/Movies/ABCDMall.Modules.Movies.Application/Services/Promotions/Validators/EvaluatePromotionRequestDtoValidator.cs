using ABCDMall.Modules.Movies.Application.DTOs.Promotions;
using FluentValidation;

namespace ABCDMall.Modules.Movies.Application.Services.Promotions.Validators;

public sealed class EvaluatePromotionRequestDtoValidator : AbstractValidator<EvaluatePromotionRequestDto>
{
    public EvaluatePromotionRequestDtoValidator()
    {
        // Day 3 validator chi chan cac input toi thieu de engine evaluate khong nhan du lieu rac.
        RuleFor(x => x.PromotionId).NotEmpty();
        RuleFor(x => x.ShowtimeId).NotEmpty();
        RuleFor(x => x.SeatSubtotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ComboSubtotal).GreaterThanOrEqualTo(0);

        RuleForEach(x => x.SnackCombos)
            .ChildRules(combo =>
            {
                combo.RuleFor(x => x.ComboId).NotEmpty();
                combo.RuleFor(x => x.Quantity).GreaterThan(0);
            });
    }
}

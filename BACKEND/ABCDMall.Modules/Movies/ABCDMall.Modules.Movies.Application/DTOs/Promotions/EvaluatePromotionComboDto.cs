namespace ABCDMall.Modules.Movies.Application.DTOs.Promotions;

public sealed class EvaluatePromotionComboDto
{
    // ComboId duoc tra ve tu endpoint /api/snack-combos va gui nguoc lai khi evaluate.
    public Guid ComboId { get; set; }
    public int Quantity { get; set; }
}

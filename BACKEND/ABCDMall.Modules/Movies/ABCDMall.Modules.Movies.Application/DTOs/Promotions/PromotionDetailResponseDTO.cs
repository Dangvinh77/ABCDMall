using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCDMall.Modules.Movies.Application.DTOs.Promotions
{
    public sealed class PromotionDetailResponseDTO
    {
        //class nay se duoc su dung de tra ve chi tiet thong tin cua mot promotion
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal? PercentageValue { get; set; } // neu la khuyen mai giam theo phan tram thi PercentageValue se co gia tri,
        public decimal? FlatDiscountValue { get; set; }// neu la khuyen mai giam theo so tien co dinh thi FlatDiscountValue se co gia tri
        public decimal? MaximumDiscountAmount { get; set; } // neu la khuyen mai giam theo phan tram thi MaximumDiscountAmount se co gia tri, gioi han so tien duoc giam toi da
        public decimal? MinimumSpendAmount { get; set; } // neu la khuyen mai co dieu kien ve so tien chi tieu thi MinimumSpendAmount se co gia tri, gioi han so tien phai chi tieu toi thieu de duoc ap dung khuyen mai
        public int? MaxRedemptions { get; set; } // gioi han so lan su dung cua khuyen mai, neu MaxRedemptions = 100 thi khuyen mai chi duoc su dung toi da 100 lan
        public int? MaxRedemptionsPerCustomer { get; set; } // gioi han so lan su dung cua khuyen mai cho moi khach hang, neu MaxRedemptionsPerCustomer = 1 thi moi khach hang chi duoc su dung khuyen mai 1 lan
        public bool IsAutoApplied { get; set; } // cho biet khuyen mai co duoc ap dung tu dong hay khong, neu IsAutoApplied = true thi khuyen mai se duoc ap dung tu dong khi
        public IReadOnlyCollection<PromotionRuleDto> Rules { get; set; } = Array.Empty<PromotionRuleDto>();
    }
}
